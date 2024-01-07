using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerVehicle : PlayerControllable, IInteractable
{
    [SerializeField] private float minimumStoppedSpeed;
    [SerializeField] private GameObject playerCharacterPrefab;
    [SerializeField] private Transform playerSpawnPosLeft;
    [SerializeField] private Transform playerSpawnPosRight;
    [SerializeField] private ParcelMenu parcelMenu;
    [SerializeField] private GameObject rearInteractPoint;
    [SerializeField] private GameObject arrowPivot;
    [FormerlySerializedAs("carDoor")]
    [Header("Sounds")]
    [SerializeField] private AudioSource carDoorSfx;
    [SerializeField] private float bigCrashSpeed;
    [SerializeField] private AudioSource smallCrashSfx;
    [SerializeField] private AudioSource bigCrashSfx;

    public Action PlayerStopped;
    public Action<float, GameObject> Crashed;
    public Action<Parcel> NextParcelUpdated;
    public Action AllParcelsDelivered;
    public Action Speeding;

    private Vector3 _destinationLocation; 
    
    private CarController _carController;
    private PlayerInput _playerInput;
    private Rigidbody2D _rigidbody;
    private GameObject _playerCharacter;
    private AudioSource _engineSound;

    private bool _hasStopped;
    private bool _isSpeeding;
    private float _previousVelocity;
    private int _currentParcelIndex;

    private IEnumerator _speedingCoroutine;

    [NonSerialized] public readonly List<GameObject> Parcels = new();
    
    private void Awake()
    {
        _playerCharacter = Instantiate(playerCharacterPrefab, transform);
        _playerCharacter.SetActive(false);
        
        _carController = GetComponent<CarController>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _engineSound = GetComponent<AudioSource>();
        rearInteractPoint.SetActive(false);
    }

    public void Init()
    {
        // Set up arrow destination
        Parcel firstParcel = Parcels[_currentParcelIndex].GetComponent<Parcel>();
        _destinationLocation = firstParcel.DeliveryLocation.transform.position;
        firstParcel.Delivered += OnParcelDelivered;
    }

    private void FixedUpdate()
    {
        // Let GameObjects know when the player has stopped.
        if (_rigidbody.velocity.magnitude <= minimumStoppedSpeed && !_hasStopped)
        {
            PlayerStopped?.Invoke();
            _hasStopped = true;
        }
        else if (_rigidbody.velocity.magnitude > minimumStoppedSpeed && _hasStopped)
        {
            _hasStopped = false;
        }

        Vector3 heading = _destinationLocation - arrowPivot.transform.position;
        Vector3 dir = heading / heading.magnitude;
        arrowPivot.transform.up = dir;
        
        float playerSpeed = _rigidbody.velocity.magnitude;
        if (playerSpeed > FineManager.SpeedLimit && !_isSpeeding)
        {
            _isSpeeding = true;
            _speedingCoroutine = SpeedingTick();
            StartCoroutine(_speedingCoroutine);
        }
        else if (playerSpeed <= FineManager.SpeedLimit && _isSpeeding)
        {
            _isSpeeding = false;
            StopCoroutine(_speedingCoroutine);
        }
    }
    
    private IEnumerator SpeedingTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            Speeding?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_hasStopped) return;
        
        Crashed?.Invoke(col.relativeVelocity.magnitude, col.gameObject);
        
        if (col.relativeVelocity.magnitude < bigCrashSpeed)
            smallCrashSfx.Play();
        else
            bigCrashSfx.Play();
        
        Debug.Log("Crashed!");
    }

    private void OnParcelDelivered(Parcel parcel)
    {
        int newParcelIndex = _currentParcelIndex;
        for (int i = _currentParcelIndex + 1; i < Parcels.Count; i++)
        {
            if (Parcels[i].GetComponent<Parcel>().IsDelivered)
                continue;
            
            newParcelIndex = i;
            break;
        }
        
        // Assume all parcels delivered
        if (newParcelIndex == _currentParcelIndex)
        {
            AllParcelsDelivered?.Invoke();
            NextParcelUpdated?.Invoke(Parcels[_currentParcelIndex].GetComponent<Parcel>());
            return;
        }

        _currentParcelIndex = newParcelIndex;

        Parcel nextParcel = Parcels[_currentParcelIndex].GetComponent<Parcel>();
        _destinationLocation = nextParcel.DeliveryLocation.transform.position;
        nextParcel.Delivered += OnParcelDelivered;
        NextParcelUpdated?.Invoke(nextParcel);
    }

    public void SetDestinationLocation(Vector3 location)
    {
        _destinationLocation = location;
    }

    private void OnAccelerationChanged(InputAction.CallbackContext context)
    {
        _carController.AccelerationInput = context.ReadValue<float>();
    }

    private void OnBrakeChanged(InputAction.CallbackContext context)
    {
        _carController.BrakeInput = context.ReadValue<float>();
    }
    
    private void OnSteerChanged(InputAction.CallbackContext context)
    {
        _carController.SteerInput = context.ReadValue<float>();
    }

    private void OnExit(InputAction.CallbackContext context)
    {
        // Only exit the vehicle if the vehicle has stopped
        if (!_hasStopped) return;

        Collider2D[] hit = Physics2D.OverlapCircleAll(
            playerSpawnPosLeft.position, 
            1.0f, 
            ~(1 << LayerMask.NameToLayer("Vehicles"))
        );

        _playerCharacter.transform.position = hit.Length == 0 ? playerSpawnPosLeft.position: playerSpawnPosRight.position;
        
        _playerCharacter.SetActive(true);
        rearInteractPoint.SetActive(true);
        arrowPivot.SetActive(false);
        _currentController.Possess(_playerCharacter.GetComponent<PlayerControllable>());
        
        _engineSound.mute = true;
        carDoorSfx.Play();
    }

    public override void Bind(PlayerController playerController)
    {
        _currentController = playerController;
        
        _currentController.PlayerInput.SwitchCurrentActionMap("Vehicle");
        
        // Bind input actions
        _currentController.PlayerInput.currentActionMap.FindAction("Acceleration", true).performed += OnAccelerationChanged;
        _currentController.PlayerInput.currentActionMap.FindAction("Acceleration", true).canceled += OnAccelerationChanged;
        
        _currentController.PlayerInput.currentActionMap.FindAction("Braking", true).performed += OnBrakeChanged;
        _currentController.PlayerInput.currentActionMap.FindAction("Braking", true).canceled += OnBrakeChanged;
        
        _currentController.PlayerInput.currentActionMap.FindAction("Steering", true).performed += OnSteerChanged;
        _currentController.PlayerInput.currentActionMap.FindAction("Steering", true).canceled += OnSteerChanged;
        
        _currentController.PlayerInput.currentActionMap.FindAction("Exit", true).performed += OnExit;
        
        _carController.ToggleEngine(true);
    }

    public override void Unbind()
    {
        _currentController.PlayerInput.currentActionMap.FindAction("Acceleration", true).performed -= OnAccelerationChanged;
        _currentController.PlayerInput.currentActionMap.FindAction("Acceleration", true).canceled -= OnAccelerationChanged;
        
        _currentController.PlayerInput.currentActionMap.FindAction("Braking", true).performed -= OnBrakeChanged;
        _currentController.PlayerInput.currentActionMap.FindAction("Braking", true).canceled -= OnBrakeChanged;
        
        _currentController.PlayerInput.currentActionMap.FindAction("Steering", true).performed -= OnSteerChanged;
        _currentController.PlayerInput.currentActionMap.FindAction("Steering", true).canceled -= OnSteerChanged;
        
        _currentController.PlayerInput.currentActionMap.FindAction("Exit", true).performed -= OnExit;
        
        _carController.ToggleEngine(false);
    }

    public void Interact(PlayerCharacter playerCharacter)
    {
        if (playerCharacter.IsHoldingParcel()) return;
        
        _currentController.Possess(this);
        arrowPivot.SetActive(true);
        _playerCharacter.SetActive(false);
        rearInteractPoint.SetActive(false);
        
        _engineSound.mute = false;
        carDoorSfx.Play();
    }

    public void StoreParcel (GameObject parcel)
    {
        parcel.transform.parent = transform;
        parcel.transform.position = Vector2.zero;
        parcel.SetActive(false);
    }
}
