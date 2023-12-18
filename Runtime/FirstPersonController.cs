using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jimothy.FPC
{
    
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    #region Component References
    [field: SerializeField] public Camera PlayerCamera { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    #endregion Component References

    #region Feature Toggles
    [Header("Feature Toggles")]
    [SerializeField] private bool jumpingEnabled = true;
    [SerializeField] private bool slidingEnabled = true;
    #endregion Feature Toggles

    #region CameraSettings
    [Header("Camera Settings")]
    [SerializeField] private float cameraHeight = 1.5f;
    [SerializeField] private float cameraFOV = 80f;
    [SerializeField] private float cameraOffsetZ = 0.1f;
    #endregion CameraSettings
    
    #region Look Settings
    [Header("Look Settings")]
    [Range(0, 90)] [SerializeField] private float minLookAngle = 90f;
    [Range(0, 90)] [SerializeField] private float maxLookAngle = 90f;
    #endregion Look Settings

    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5.0f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float slopeSpeed = 8f;
    #endregion Movement Settings

    #region Input Settings
    [Header("Input Settings")]
    [SerializeField] private float mouseSensitivityX = 3.5f;
    [SerializeField] private float mouseSensitivityY = 3.5f;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    #endregion Input Settings

    #region Private Member Variables
    private Transform _playerCameraTransform;
    private float _rotationX;
    private float _rotationY;
    private Transform _transform;
    private Vector2 _inputVector;
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _playerHeight;
    private Vector3 _groundNormal;
    private float _slopeLimit;
    private Vector3 _playerCenter;
    #endregion Private Member Variables;

    #region Properties
    private float MovementSpeed => movementSpeed;
    private bool ShouldJump => jumpingEnabled && Input.GetKeyDown(jumpKey) && _isGrounded;
    #endregion Properties
    
    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        SetReferences();
        SetUpCamera();
    }

    private void SetUpCamera()
    {
        PlayerCamera.fieldOfView = cameraFOV;
        _playerCameraTransform.transform.localPosition = new Vector3(0, cameraHeight, cameraOffsetZ);
    }

    private void SetReferences()
    {
        SetCamera();
        SetCharacterController();
    }

    private void SetCamera()
    {
        if (PlayerCamera == null)
            PlayerCamera = Camera.main!;

        _playerCameraTransform = PlayerCamera.transform;
        _playerCameraTransform.SetParent(transform);
        _playerCameraTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void SetCharacterController()
    {
        if (CharacterController == null)
            CharacterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        SetPrivateVariables();
        
        HandleMouseLook();
        HandleGravity();
        HandleJump();
        HandleSlide();

        ApplyMovement();

        HandleDebug();
    }

    private void HandleSlide()
    {
        if (!slidingEnabled || !_isGrounded)
            return;

        SetGroundNormal();

        if (_groundNormal == Vector3.zero)
            return;

        if (Vector3.Angle(_groundNormal, Vector3.up) < _slopeLimit)
            return;

        _velocity += new Vector3(_groundNormal.x, -_groundNormal.y, _groundNormal.z) * slopeSpeed;
    }

    private void SetGroundNormal()
    {
        if (!Physics.Raycast(_transform.position, Vector3.down, out RaycastHit slopeHit,
                _playerHeight))
            _groundNormal = Vector3.zero;


        _groundNormal = slopeHit.normal;
    }

    private void HandleDebug()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"{_isGrounded} - {Time.time}");
            Debug.Log($"{_groundNormal} - {Time.time}");
        }
    }

    private void HandleJump()
    {
        if (!ShouldJump)
            return;

        _velocity.y += jumpForce;
    }

    private void HandleGravity()
    {
        if (!_isGrounded)
            _velocity.y -= gravity * Time.deltaTime;
        else
            _velocity.y = -1f;
    }

    private void ApplyMovement()
    {
        CharacterController.Move(_velocity * Time.deltaTime);
    }

    private void SetPrivateVariables()
    {
        _inputVector = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")).normalized;
        
        var velocityY = _velocity.y;
        _velocity = _transform.forward * _inputVector.x + transform.right * _inputVector.y;
        _velocity *= MovementSpeed;
        _velocity.y = velocityY;
        
        _isGrounded = CharacterController.isGrounded;
        _playerHeight = CharacterController.height;
        _playerCenter = CharacterController.center;
        _slopeLimit = CharacterController.slopeLimit;
    }

    private void HandleMouseLook()
    {
        _rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
        _rotationX = Mathf.Clamp(_rotationX, -maxLookAngle, minLookAngle);
        _playerCameraTransform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

        _rotationY = Input.GetAxis("Mouse X") * mouseSensitivityX;
        transform.rotation *= Quaternion.Euler(0, _rotationY, 0);
    }
}
}
