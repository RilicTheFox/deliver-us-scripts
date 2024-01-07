using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : PlayerControllable
{
    [Header("Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float throwForce;
    [SerializeField] private float interactRange;
    [SerializeField] public bool canMove = true;
    [Header("References")]
    [SerializeField] private Transform parcelHoldingPosition;
    [SerializeField] private Animator animator;

    public static Action ThrownParcel;
    
    public bool IsHoldingParcel()
    {
        return HeldParcel != null;
    }

    public Parcel HeldParcel { get; private set; }

    private Rigidbody2D _rigidbody;
    private Vector2 _input;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        // Move around
        _rigidbody.MovePosition(_rigidbody.position + _input * (walkSpeed * Time.fixedDeltaTime));
        // Turn to movement
        if (_input.magnitude > 0 ) transform.up = _input.normalized;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (IsHoldingParcel())
        {
            DropParcel();
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            interactRange, 
            ~(1 << LayerMask.NameToLayer("Player"))
            );
        
        // Loop through to find the closest interactable
        float lastDistance = 0.0f;
        IInteractable interactable = null;
        foreach (Collider2D c in colliders)
        {
            IInteractable currentInteractable = c.gameObject.GetComponent<IInteractable>();
            if (currentInteractable == null)
                continue;

            float distanceBetweenObjects = Vector3.Distance(transform.position, c.transform.position);
            
            if (lastDistance == 0)
            {
                interactable = currentInteractable;
                lastDistance = distanceBetweenObjects;
            }
            else
            {
                if (distanceBetweenObjects >= lastDistance) continue;
                
                interactable = currentInteractable;
                lastDistance = distanceBetweenObjects;
            }
        }

        if (interactable != null)
        {
            interactable?.Interact(this);
        }

        if (isActiveAndEnabled)
            UpdateAnimation();
    }

    private void OnThrow(InputAction.CallbackContext context)
    {
        if (IsHoldingParcel())
            ThrowParcel();
    }

    private void OnWalk(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        
        // Animation code
        UpdateAnimation();
    }

    public override void Bind(PlayerController playerController)
    {
        _currentController = playerController;
        _currentController.PlayerInput.SwitchCurrentActionMap("OnFoot");
        
        _currentController.PlayerInput.currentActionMap.FindAction("Walk", true).performed += OnWalk;
        _currentController.PlayerInput.currentActionMap.FindAction("Walk", true).canceled += OnWalk;

        _currentController.PlayerInput.currentActionMap.FindAction("Interact", true).performed += OnInteract;
        _currentController.PlayerInput.currentActionMap.FindAction("Throw", true).performed += OnThrow;
    }

    public override void Unbind()
    {
        _currentController.PlayerInput.currentActionMap.FindAction("Walk", true).performed -= OnWalk;
        _currentController.PlayerInput.currentActionMap.FindAction("Walk", true).canceled -= OnWalk;

        _currentController.PlayerInput.currentActionMap.FindAction("Interact", true).performed -= OnInteract;
        _currentController.PlayerInput.currentActionMap.FindAction("Throw", true).performed -= OnThrow;
    }

    public void DropParcel()
    {
        HeldParcel.gameObject.layer = LayerMask.NameToLayer("Parcels");
        HeldParcel.GetComponent<Rigidbody2D>().isKinematic = false;
        HeldParcel.GetComponent<BoxCollider2D>().enabled = true;
        HeldParcel.transform.parent = null;
        HeldParcel = null;
    }

    public void ThrowParcel()
    {
        HeldParcel.GetComponent<Rigidbody2D>().isKinematic = false;
        HeldParcel.GetComponent<Rigidbody2D>().AddForce(transform.up * throwForce, ForceMode2D.Impulse);
        HeldParcel.OnThrown();
        ThrownParcel?.Invoke();
        DropParcel();
    }

    public void PickUpParcel(GameObject parcel)
    {
        if (IsHoldingParcel())
            DropParcel();

        if (!parcel.activeSelf)
            parcel.SetActive(true);
        
        parcel.transform.parent = parcelHoldingPosition;
        parcel.transform.position = parcelHoldingPosition.position;
        parcel.layer = LayerMask.NameToLayer("Player");
        
        HeldParcel = parcel.GetComponent<Parcel>();
        HeldParcel.GetComponent<Rigidbody2D>().isKinematic = true;
        HeldParcel.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void UpdateAnimation()
    {
        // Animation code
        if (_input.magnitude > 0)
            animator.Play(IsHoldingParcel() ? "PlayerBoxWalk" : "PlayerWalk");
        else
            animator.Play(IsHoldingParcel() ? "PlayerBoxIdle" : "PlayerIdle");
    }
}
