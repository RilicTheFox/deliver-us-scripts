using System;
using UnityEngine;

enum FineReasons
{
    Crashed,
    Speeding
}

public class FineManager : MonoBehaviour
{
    [Header("Crash Fine")]
    [SerializeField] private int minimumCrashFine;
    [SerializeField] private int maximumCrashFine;
    [SerializeField] private float maxFineSpeed;

    [Header("Speeding Fines")]
    [SerializeField] private int baseSpeedingFine;
    [SerializeField] private float multiplierPerMps;
    [SerializeField] public static float SpeedLimit = 6.5f; // translates to roughly 30 """kph""" when multiplied by 5, like on the HUD.

    public int TotalFines { get; private set; }
    public int TotalFinesAmount { get; private set; }

    public void Init(PlayerVehicle playerVehicle)
    {
        playerVehicle.Crashed += CrashedFine;
        SpeedTrap.PlayerCaughtSpeeding += SpeedingFine;
    }

    private void Fine(int amount, FineReasons reason)
    {
        string text = reason switch
        {
            FineReasons.Crashed => "CRASHED",
            FineReasons.Speeding => "SPEEDING",
            _ => "lol wot????"
        };

        Debug.Log($"You were fined! Reason: {text}");
        MoneyManager.Instance.CurrentMoney -= amount;
        
        TotalFinesAmount += amount;
        TotalFines++;
    }

    private void CrashedFine(float crashVelocity, GameObject collidedObject)
    {
        // Calculate damage cost from velocity
        float finePercent = Mathf.Min(crashVelocity / maxFineSpeed, 1);
        
        float damageCost = minimumCrashFine + (maximumCrashFine - minimumCrashFine) * finePercent;
        
        Fine(Mathf.RoundToInt(damageCost), FineReasons.Crashed);
    }

    private void SpeedingFine(float playerSpeed)
    {
        int amount =
            Mathf.RoundToInt(baseSpeedingFine
                             + baseSpeedingFine
                             * multiplierPerMps
                             * (playerSpeed - SpeedLimit));
        
        Fine(amount, FineReasons.Speeding);
    }

    private void OnDestroy()
    {
        SpeedTrap.PlayerCaughtSpeeding -= SpeedingFine;
    }
}
