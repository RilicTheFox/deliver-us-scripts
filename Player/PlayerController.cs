using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    public PlayerInput PlayerInput { get; private set; }

    private PlayerControllable _target;

    public void Init()
    {
        PlayerInput = GetComponent<PlayerInput>();
    }

    public void Possess(PlayerControllable newTarget)
    {
        Unpossess();
        
        _target = newTarget;
        _target.Bind(this);

        virtualCamera.Follow = newTarget.transform;
    }

    public void Unpossess()
    {
        if (_target == null)
            return;
        
        _target.Unbind();
        _target = null;
    }
}