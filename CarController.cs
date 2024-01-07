using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class CarController : MonoBehaviour
{
    public Action<float> SpeedChanged;
    
    [Header("Car Options")]
    [SerializeField] private float maxForwardSpeed;
    [SerializeField] private float maxReverseSpeed;
    [SerializeField] private float steerRate;
    [SerializeField] private float acceleration;
    [SerializeField] private float brakingForce;
    [SerializeField][Range(0f, 1f)] private float driftAmount;
    [SerializeField] private float minimumDriftSpeed;
    [SerializeField] private float maximumDrag;
    [SerializeField] private float timeToChangeGear;

    [Header("Sound Settings")]
    [SerializeField] private float minEnginePitch;
    [SerializeField] private float maxEnginePitch;
    [SerializeField] private float maxPitchSpeed;

    [NonSerialized] public float SteerInput;
    [NonSerialized] public float AccelerationInput;
    [NonSerialized] public float BrakeInput;
    
    private bool _isReversing;
    private bool _isEngineOn;
    private float _rotationAngle;
    private float _changeGearTimer;

    private AudioSource _engineSound;
    
    public float VelocityVsUp { get; private set; }

    private PlayerInput _playerInput;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _engineSound = GetComponent<AudioSource>();
        _changeGearTimer = timeToChangeGear;
    }

    private void FixedUpdate()
    {
        Vector2 up = transform.up;
        
        // Calculate our "forward" velocity, as opposed to actual velocity
        VelocityVsUp = Vector2.Dot(up, _rigidbody.velocity);
        
        // Add acceleration and control engine
        ApplyEngineForce(up);
        
        // Drag
        _rigidbody.drag = maximumDrag * (1 - Mathf.Abs(AccelerationInput)); // Account for analogue input
        
        // Steering
        float turningCircle = Mathf.Clamp01(_rigidbody.velocity.magnitude / minimumDriftSpeed); // Limits cars ability to turn while slow
        _rotationAngle -= SteerInput * steerRate * turningCircle * Time.fixedDeltaTime;
        _rigidbody.MoveRotation(_rotationAngle);
        
        // Drifting
        Vector2 forwardVelocity = up * Vector2.Dot(_rigidbody.velocity, up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(_rigidbody.velocity, transform.right);
        _rigidbody.velocity = forwardVelocity + rightVelocity * driftAmount;
        
        SpeedChanged?.Invoke(_rigidbody.velocity.magnitude);
        
        // Sound logic
        float pitchPercent = Mathf.Abs(VelocityVsUp) / maxPitchSpeed;
        pitchPercent = Mathf.Clamp01(pitchPercent);
        _engineSound.pitch = (maxEnginePitch - minEnginePitch) * pitchPercent + minEnginePitch;
    }

    private void ApplyEngineForce(Vector2 up)
    {
        Vector2 engineForce = new();
        
        float accel;
        float brake;
        float maxSpeed;
        Vector2 direction;

        if (_isReversing)
        {
            accel = BrakeInput;
            brake = AccelerationInput;
            maxSpeed = maxReverseSpeed;
            direction = -up;
        }
        else
        {
            accel = AccelerationInput;
            brake = BrakeInput;
            maxSpeed = maxForwardSpeed;
            direction = up;
        }

        // Acceleration
        if (accel > 0)
        {
            if (VelocityVsUp > maxSpeed) // Limit forward speed
                return;
            
            if (_rigidbody.velocity.sqrMagnitude > maxSpeed * maxSpeed) // Limit speed in any direction while accelerating
                return;
            
            float currentAcceleration = accel * acceleration;
            engineForce = direction * (currentAcceleration * Time.fixedDeltaTime);
        }

        // Braking
        float currentBrake = brake * brakingForce;
        if (brake > 0)
        {
            if (Mathf.Abs(VelocityVsUp) > 0.1)
                engineForce += -direction * (currentBrake * Time.fixedDeltaTime);
            else
                _rigidbody.velocity = Vector2.zero;
        }

        // Apply forces
        _rigidbody.AddForce(engineForce);
        
        // Switch reversing using timer
        if (brake > 0 && VelocityVsUp == 0)
        {
            _changeGearTimer -= Time.fixedDeltaTime;
            if (_changeGearTimer <= 0)
            {
                _isReversing = !_isReversing;
                _changeGearTimer = timeToChangeGear;
            }
        }
        else
        {
            _changeGearTimer = timeToChangeGear;
        }
    }
    
    public void ToggleEngine(bool engineOn)
    {
        if (engineOn)
        {
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
