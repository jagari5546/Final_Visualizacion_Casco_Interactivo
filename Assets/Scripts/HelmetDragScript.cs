using System;
using NUnit.Framework.Internal;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class HelmetDragScript : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _mainCamera;
    [SerializeField] private InputActionAsset _action;

    private Vector3 mPrevPos = Vector3.zero;
    private Vector3 mPosDelta = Vector3.zero;

    public InputActionAsset action
    {
        get => _action;
        set => _action = value;
    }
    
    protected InputAction leftClickPressedInputAction {get; set;}
    
    protected InputAction mouseLockInputAction {get; set;}

    private bool _rotateAllowed;
    
    [SerializeField] private float _rotationSpeed = 0.2f;
    [SerializeField] private bool _inverted;

    private void Awake()
    {
        InitializeInputSystem();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!_rotateAllowed)
            return;

        Vector2 MouseDelta = GetMouseLookInput();
        
        MouseDelta *= _rotationSpeed *Time.deltaTime;
        
        transform.Rotate(Vector3.up *(_inverted ? 1 : -1), MouseDelta.x, Space.World);
        transform.Rotate(Vector3.right *(_inverted ? 1 : -1), MouseDelta.y, Space.World);
        
    }

    private void InitializeInputSystem()
    {
        leftClickPressedInputAction = action.FindAction("Attack");
        if (leftClickPressedInputAction != null)
        {
            leftClickPressedInputAction.started += OnLeftClickPressed;
            leftClickPressedInputAction.performed += OnLeftClickPressed;
            leftClickPressedInputAction.canceled += OnLeftClickPressed;
        }
        
        mouseLockInputAction = action.FindAction("Lock");
        
        action.Enable();

    }

    protected virtual void OnLeftClickPressed(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            _rotateAllowed = true;
        }
        else if (context.canceled)
        {
            _rotateAllowed = false;
        }
    }

    protected virtual Vector2 GetMouseLookInput()
    {
        if (mouseLockInputAction != null)
        {
            return mouseLockInputAction.ReadValue<Vector2>();
        }
        return Vector2.zero;
    }
    
}
