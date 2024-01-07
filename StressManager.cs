using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StressManager : MonoBehaviour
{
    public Action<float> StressUpdated;
    public Action StressMaxed;

    [Header("Settings")]
    [SerializeField] private float stressTickTime;
    [SerializeField] private float stressPerTick;
    [SerializeField] private float stressRestDeduction;
    [Header("Stress Relief Values")]
    [SerializeField] private float speedingStressRelief;
    [SerializeField] private float throwingStressRelief;

    private GameMode _gameMode;

    private Coroutine _stressCoroutine;
    
    public float Stress { get; private set; }

    public void Init(PlayerVehicle playerVehicle)
    {
        // Read from shuttle if value has been set previously
        if (Shuttle.Instance.stress != -1)
        {
            Stress = Shuttle.Instance.stress;
            Stress -= stressRestDeduction;
        }

        PlayerCharacter.ThrownParcel += OnPlayerThrow;
        
        _gameMode = GetComponent<GameMode>();
        
        playerVehicle.Speeding += OnPlayerSpeeding;
        
        _stressCoroutine = StartCoroutine(StressTickRoutine());
    }
    
    private void OnPlayerSpeeding()
    {
        RelieveStress(speedingStressRelief);
    }

    private IEnumerator StressTickRoutine()
    {
        while (_gameMode.IsInPlay)
        {
            Stress += stressPerTick;
            Stress = Mathf.Clamp(Stress, 0, 1);
            StressUpdated?.Invoke(Stress);

            if (Stress == 1)
            {
                StressMaxed?.Invoke();
                StopCoroutine(_stressCoroutine);
            }
            
            yield return new WaitForSeconds(stressTickTime);     
        }

        yield return new WaitUntil(() => _gameMode.IsInPlay);
        _stressCoroutine = StartCoroutine(StressTickRoutine());
    }

    private void RelieveStress(float value)
    {
        Stress -= value;
        StressUpdated?.Invoke(Stress);
    }

    private void OnPlayerThrow()
    {
        RelieveStress(throwingStressRelief);
    }

    private void OnDestroy()
    {
        Shuttle.Instance.stress = Stress;
        PlayerCharacter.ThrownParcel -= OnPlayerThrow;
    }
}
