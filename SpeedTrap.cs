using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpeedTrap : MonoBehaviour
{
    [SerializeField] private AudioSource cameraShutterSfx;
    public static Action<float> PlayerCaughtSpeeding;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerVehicle playerVehicle = other.GetComponent<PlayerVehicle>();
        
        Debug.Log("a");
        
        if (playerVehicle == null)
            return;

        float playerSpeed = playerVehicle.GetComponent<CarController>().VelocityVsUp;
        
        if (playerSpeed < FineManager.SpeedLimit)
            return;
        
        PlayerCaughtSpeeding.Invoke(playerSpeed);
        cameraShutterSfx.Play();
    }
}
