using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = System.Random;

[RequireComponent(typeof(FineManager))]
[RequireComponent(typeof(StressManager))]
[RequireComponent(typeof(MoneyManager))]
public class GameMode : MonoBehaviour
{
    [FormerlySerializedAs("timeToFinish")] [Header("Game Variables")] [SerializeField]
    private int realTimeToFinish;

    [SerializeField] private int workdaySalary;
    [SerializeField] private int allParcelsDeliveredBonus;
    [SerializeField] private int finePerUndeliveredParcel;
    [SerializeField] private int finePerDamagedParcel;
    [SerializeField] private int numberOfParcels;
    [SerializeField] private int startingDay = 1;
    [SerializeField] private int numberOfDays = 5;
    [SerializeField] private Transform playerSpawnLocation;

    [SerializeField] private int debtDefaultAmount;

    [Header("Daily Charges")] [SerializeField]
    private int minCorporatePropertyCharge;

    [SerializeField] private int maxCorporatePropertyCharge;
    [SerializeField] private int minBills;
    [SerializeField] private int maxBills;
    [SerializeField] private int studentDebt;

    [Header("Endings")]
    [SerializeField] private int goodEndingMoney;
    [SerializeField] private int badEndingMoney;
    [SerializeField] private int uglyEndingMoney;
    [SerializeField] private int failEndingMoney;
    
    [Header("Prefabs")] [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerControllerPrefab;
    [SerializeField] private GameObject parcelPrefab;
    [SerializeField] private GameObject hudPrefab;
    [SerializeField] private GameObject gameEndUIPrefab;

    private FineManager _fineManager;
    private StressManager _stressManager;
    private MoneyManager _moneyManager;
    private PlayerInput _globalInput;

    private HUD _hud;
    private PlayerController _playerController;
    private PlayerVehicle _playerVehicle;

    private List<DeliveryLocation> _deliveryLocations = new();
    private WarehouseParkingLocation _warehouseParkingLocation;

    public bool IsInPlay { get; private set; } = true;

    private float _minuteLength;

    private int _deliveredParcels;
    private int _totalDamagedPenalty;

    private int _dayNumber;

    private bool _canPause = true;

    private bool _allParcelsDelivered;

    private void Awake()
    {
        _minuteLength = realTimeToFinish / 480f;
        _dayNumber = startingDay;
        _globalInput = GetComponent<PlayerInput>();
        _globalInput.currentActionMap.FindAction("Pause", true).performed += OnPause;
    }

    private void Start()
    {
        // Create shuttle if needed
        if (Shuttle.Instance == null)
            Shuttle.CreateShuttle();

        // Set up day number
        if (Shuttle.Instance.previousDay != -1)
            _dayNumber = Shuttle.Instance.previousDay + 1;


        // Init money manager
        _moneyManager = GetComponent<MoneyManager>();
        _moneyManager.Init();
        
        // Spawn player controller
        _playerController = Instantiate(playerControllerPrefab).GetComponent<PlayerController>();
        _playerController.Init();
        
        // Spawn player
        GameObject player = Instantiate(playerPrefab, playerSpawnLocation.position, playerSpawnLocation.rotation);
        _playerVehicle = player.GetComponent<PlayerVehicle>();
        _playerController.Possess(_playerVehicle);
        _playerVehicle.AllParcelsDelivered += OnAllParcelsDelivered;

        // Init stress manager
        _stressManager = GetComponent<StressManager>();
        _stressManager.Init(_playerVehicle);
        _stressManager.StressMaxed += OnStressMaxed;

        // Init fine manager
        _fineManager = GetComponent<FineManager>();
        _fineManager.Init(_playerVehicle);

        // This is fine because they are all the same.
        _warehouseParkingLocation = FindObjectOfType<WarehouseParkingLocation>();
        WarehouseParkingLocation.WarehouseVisited += OnWarehouseVisited;

        // Get all delivery locations in level and subscribe to their events
        _deliveryLocations.AddRange(FindObjectsOfType<DeliveryLocation>());

        // Add parcels to inventory
        for (int i = 0; i < numberOfParcels; i++)
        {
            GameObject parcel = GenerateParcel();
            parcel.GetComponent<Parcel>().Delivered += OnParcelDelivered;
            _playerVehicle.Parcels.Add(parcel);
            _playerVehicle.StoreParcel(parcel);
        }

        // Init player
        _playerVehicle.Init();
        
        // Spawn and init HUD
        _hud = Instantiate(hudPrefab).GetComponent<HUD>();
        _hud.Init(_stressManager, _playerVehicle);

        foreach (GameObject parcel in _playerVehicle.Parcels)
        {
            parcel.GetComponent<Parcel>().Damaged += OnParcelDamaged;
        }

        _hud.SetParcelsDelivered(0, numberOfParcels);

        // Start timer
        StartCoroutine(Countdown());
    }

    private void OnDestroy()
    {
        WarehouseParkingLocation.WarehouseVisited -= OnWarehouseVisited;
        _stressManager.StressUpdated -= _hud.SetStress;

        // _globalInput.currentActionMap.FindAction("Pause", true).performed -= OnPause;

        Shuttle.Instance.previousDay = _dayNumber;
    }

    private void FixedUpdate()
    {
        if (_canPause == false)
            _canPause = true;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator Countdown()
    {
        for (int i = 480; i > 0; i--)
        {
            _hud.SetClock(480 - i);
            yield return new WaitForSeconds(_minuteLength);
        }

        // Timer over
        _hud.SetClock(480);
        IsInPlay = false;
        _playerVehicle.SetDestinationLocation(_warehouseParkingLocation.transform.position);

        // Lock locations after shift is over 
        foreach (DeliveryLocation location in _deliveryLocations)
        {
            location.Lock();
        }

        _hud.DisplayMessage("Shift is over - return to the warehouse.");
    }

    private void OnStressMaxed()
    {
        LoseGame("WENT POSTAL");
    }

    private void OnWarehouseVisited()
    {
        if (IsInPlay && _allParcelsDelivered == false)
            return;

        IsInPlay = false;

        int deliveredParcels = 0;
        int parcelsInWrongPlace = 0;
        int parcelsOutsideVan = 0;
        int damagedParcels = 0;
        foreach (GameObject parcelObject in _playerVehicle.Parcels)
        {
            Parcel parcel = parcelObject.GetComponent<Parcel>();
            if (parcel.IsDelivered && !parcel.IsDeliveredToWrongPlace && !parcel.IsDamaged)
                deliveredParcels++;

            if (parcel.IsDeliveredToWrongPlace)
                parcelsInWrongPlace++;

            if (!parcel.IsDelivered && parcel.gameObject.activeSelf)
                parcelsOutsideVan++;

            if (parcel.IsDamaged)
                damagedParcels++;
        }

        int undeliveredParcels = _playerVehicle.Parcels.Count - deliveredParcels;
        int misplacedParcels = parcelsInWrongPlace + parcelsOutsideVan;

        Random random = new();
        int corporatePropertyCharge = random.Next(minCorporatePropertyCharge, maxCorporatePropertyCharge);
        int bills = random.Next(minBills, maxBills);

        int earnings = workdaySalary
                       + (_allParcelsDelivered ? allParcelsDeliveredBonus : 0)
                       - (finePerUndeliveredParcel * (undeliveredParcels + misplacedParcels))
                       - corporatePropertyCharge
                       - bills
                       - studentDebt;

        MoneyManager.Instance.CurrentMoney += earnings;

        WorkdayStats stats = new();
        stats.DayNumber = _dayNumber;
        stats.Salary = workdaySalary;
        stats.UndeliveredParcels = undeliveredParcels;
        stats.MisplacedParcels = misplacedParcels;
        stats.MisplacedParcelsPenalty = finePerUndeliveredParcel * misplacedParcels;
        stats.DamagedParcels = damagedParcels;
        stats.DamagedParcelsPenalty = _totalDamagedPenalty;
        stats.FinesIncurred = _fineManager.TotalFines;
        stats.FinesIncurredPenalty = _fineManager.TotalFinesAmount;
        stats.CorporatePropertyCharge = corporatePropertyCharge;
        stats.Bills = bills;
        stats.StudentDebt = studentDebt;
        
        if (_allParcelsDelivered)
        {
            stats.UndeliveredParcelsPenalty = allParcelsDeliveredBonus;
            stats.AllParcelsDelivered = true;
        }
        else
        {
            stats.UndeliveredParcelsPenalty = finePerUndeliveredParcel * undeliveredParcels;
        }

        GameEndUI gameEndUI = Instantiate(gameEndUIPrefab).GetComponent<GameEndUI>();
        gameEndUI.Populate(stats);
        gameEndUI.DayEndFinished += NextDay;

        _playerController.Unpossess();
    }

    private void OnAllParcelsDelivered()
    {
        _playerVehicle.SetDestinationLocation(_warehouseParkingLocation.transform.position);
        _allParcelsDelivered = true;
        _hud.DisplayMessage("All parcels delivered! Return to warehouse.");
    }

    private void NextDay()
    {
        // Ending check
        if (_moneyManager.CurrentMoney <= debtDefaultAmount)
        {
            LoseGame("DEFAULTED DEBT");
        }
        else if (_dayNumber >= numberOfDays)
        {
            _hud.FinishedFadeToBlack += () =>
            {
                SceneManager.LoadSceneAsync("Ending", LoadSceneMode.Single);
            };

            int money = _moneyManager.CurrentMoney;

            if (money < failEndingMoney)
                Shuttle.Instance.endingType = Ending.Types.Fail;
            else if (money < uglyEndingMoney)
                Shuttle.Instance.endingType = Ending.Types.Ugly;
            else if (money < badEndingMoney)
                Shuttle.Instance.endingType = Ending.Types.Bad;
            else
                Shuttle.Instance.endingType = Ending.Types.Good;
            
            _hud.FadeToBlack();
        }
        // Go to next day
        else
        {
            _hud.FinishedFadeToBlack += () =>
            {
                SceneManager.LoadSceneAsync("GameLevel", LoadSceneMode.Single);
            };
            
            _hud.FadeToBlack();
        }
    }
    
    private void LoseGame(string reason)
    {
        _playerController.Unpossess();

        _hud.FadeToBlack();
        _hud.FinishedFadeToBlack += () =>
        {
            GameOverUI gameOverUI = Resources.Load<GameOverUI>("UI/GameOverUI");
            gameOverUI.reason = reason;
            Instantiate(gameOverUI);
        };
    }

    private GameObject GenerateParcel()
    {
        Random random = new();

        // Choose a delivery location
        int deliveryIndex = random.Next(_deliveryLocations.Count);
        int recipientIndex = random.Next(_deliveryLocations[deliveryIndex].GetRecipients().Count);
        int value = random.Next(5, 500);

        GameObject parcel = Instantiate(parcelPrefab, _playerVehicle.transform);
        Parcel parcelComp = parcel.GetComponent<Parcel>();
        parcelComp.DeliveryLocation = _deliveryLocations[deliveryIndex];
        parcelComp.RecipientName = _deliveryLocations[deliveryIndex].GetRecipients()[recipientIndex];
        parcelComp.Value = value;

        return parcel;
    }

    private void OnParcelDelivered(Parcel parcel)
    {
        _deliveredParcels++;
        _hud.SetParcelsDelivered(_deliveredParcels, numberOfParcels);
    }

    

    private void OnParcelDamaged(Parcel parcel)
    {
        int penalty = finePerDamagedParcel + parcel.Value;
        MoneyManager.Instance.CurrentMoney -= Mathf.RoundToInt(penalty);
        _totalDamagedPenalty += penalty;
        _hud.DisplayMessage("Parcel damaged!");
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (_canPause)
            Instantiate(Resources.Load("UI/PauseMenu"));

        _canPause = false;
    }
}