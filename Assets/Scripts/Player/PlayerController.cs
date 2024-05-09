using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static PlayerInput;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NetworkBehaviour, IPlayerActions
{
    private PlayerInput _playerInput;
    private Vector2 _moveInput = new();
    private Vector2 _cursorLocation;

    private Transform _shipTransform;
    private Rigidbody2D _rb;
    private Health _health;


    private Transform turretPivotTransform;


    public UnityAction<bool> onFireEvent;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float shipRotationSpeed = 100f;
    [SerializeField] private float turretRotationSpeed = 4f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        InitialSetup();
        Respawn();
    }

    public void InitialSetup()
    {
        if (!(IsOwner||IsServer)) return;

        if (_playerInput == null)
        {
            _playerInput = new();
            _playerInput.Player.SetCallbacks(this);
        }
        _playerInput.Player.Enable();

        _rb = GetComponent<Rigidbody2D>();
        _shipTransform = transform;
        turretPivotTransform = transform.Find("PivotTurret");
        _health = GetComponent<Health>();
        _health.OnHealthZeroClient.AddListener(Death);

        if (turretPivotTransform == null) Debug.LogError("PivotTurret is not found", gameObject);
    }

    public void Respawn()
    {
        if (!IsOwner && !IsServer) return;
        transform.position = new Vector3( 0.0f, 0.0f, 0.0f );
        if(_health == null) _health = GetComponent<Health>();
        _health.RequestHealthChange_ServerRPC(HealthChangeReason.death);
    }

    public void Death()
    {
        if (!IsOwner) return;

        Debug.Log("died" + this);
        Respawn();
    }

    public void OnFire(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onFireEvent.Invoke(true);
        }
        else if (context.canceled)
        {
            onFireEvent.Invoke(false);
        }
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        _rb.velocity = transform.up * _moveInput.y * movementSpeed;
        _rb.MoveRotation(_rb.rotation + _moveInput.x * -shipRotationSpeed * Time.fixedDeltaTime);
    }
    private void LateUpdate()
    {
        if (!IsOwner) return;
        Vector2 screenToWorldPosition = Camera.main.ScreenToWorldPoint(_cursorLocation);
        Vector2 targetDirection = new Vector2(screenToWorldPosition.x - turretPivotTransform.position.x, screenToWorldPosition.y - turretPivotTransform.position.y).normalized;
        Vector2 currentDirection = Vector2.Lerp(turretPivotTransform.up, targetDirection, Time.deltaTime * turretRotationSpeed);
        turretPivotTransform.up = currentDirection;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        _cursorLocation = context.ReadValue<Vector2>();
    }

}
