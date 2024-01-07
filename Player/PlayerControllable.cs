using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerControllable : MonoBehaviour
{
    protected PlayerController _currentController;

    public abstract void Bind(PlayerController playerController);
    public abstract void Unbind();
}