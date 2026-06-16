using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerControl : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float upperLookLimit = 80f;
    public float lowerLookLimit = -80f;

    private CharacterController _controller;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private Vector3 _velocity;
    private float _cameraPitch = 0f;

    private bool moving = true;
    private bool rotating = true;

    public bool IsDisabled(){return !moving;}

    public void SetPlayerRotation(bool canRotate)
    {
        if(rotating == canRotate) return;
        rotating = canRotate;
    }
    public void SetPlayerMovement(bool canMove){
        if(moving == canMove) return;
        moving = canMove;
    }

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        
        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    // These methods are called by the PlayerInput component via "SendMessage" 
    // or by linking them in the Inspector "Events" section.
    public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => _lookInput = value.Get<Vector2>();
    public void OnJump(InputValue value)
    {
        if (_controller.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void Update()
    {
        if(rotating) HandleRotation();
        if(moving) HandleMovement();
    }

    private void HandleMovement()
    {
        // Ground check
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        // Calculate movement relative to transform
        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _controller.Move(move * walkSpeed * Time.deltaTime);

        // Apply Gravity
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        // Horizontal Rotation (Rotate the whole player)
        transform.Rotate(Vector3.up * _lookInput.x * mouseSensitivity);

        // Vertical Rotation (Rotate only the camera)
        _cameraPitch -= _lookInput.y * mouseSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, lowerLookLimit, upperLookLimit);

        cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }
}