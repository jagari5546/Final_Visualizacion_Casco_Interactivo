using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Unity.Cinemachine; // si no usas Cinemachine, cambia el tipo a Camera

public class HelmetDragScript : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CinemachineCamera cam; // o Camera cam
    [Tooltip("Capa(s) clickeables del casco")]
    [SerializeField] private LayerMask interactMask = ~0; // por defecto todo

    [Header("Input (New Input System)")]
    // Asigna desde tu InputActionAsset (Pointer/press, Pointer/delta, Pointer/position)
    [SerializeField] private InputActionReference pointerPress;    // Button
    [SerializeField] private InputActionReference pointerDelta;    // Vector2
    [SerializeField] private InputActionReference pointerPosition; // Vector2

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private bool invertY = false;

    private bool dragging = false;
    private int activePointerId = -1; // mouse = -1; touch = touchId

    void OnEnable()
    {
        if (pointerPress != null)
        {
            pointerPress.action.started   += OnPointerPressed;
            pointerPress.action.canceled  += OnPointerReleased;
            pointerPress.action.Enable();
        }
        if (pointerDelta != null)   pointerDelta.action.Enable();
        if (pointerPosition != null) pointerPosition.action.Enable();

        // ¡No bloquees el cursor si quieres UI!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void OnDisable()
    {
        if (pointerPress != null)
        {
            pointerPress.action.started  -= OnPointerPressed;
            pointerPress.action.canceled -= OnPointerReleased;
            pointerPress.action.Disable();
        }
        if (pointerDelta != null)   pointerDelta.action.Disable();
        if (pointerPosition != null) pointerPosition.action.Disable();
    }

    void Update()
    {
        if (!dragging) return;

        Vector2 delta = pointerDelta != null ? pointerDelta.action.ReadValue<Vector2>() : Vector2.zero;
        if (delta.sqrMagnitude <= Mathf.Epsilon) return;

        float dx = delta.x * rotationSpeed * Time.deltaTime;
        float dy = delta.y * rotationSpeed * Time.deltaTime * (invertY ? 1f : -1f);

        // Ejes de referencia
        Transform camTf = cam != null ? cam.transform : Camera.main.transform;

        // 1) Yaw alrededor del 'up' del casco (drag horizontal)
        transform.Rotate(transform.up, dx, Space.World);

        // 2) Pitch alrededor del 'right' de la cámara (drag vertical)
        transform.Rotate(camTf.right, dy, Space.World);
    }

    // ---------- INPUT HANDLERS ----------
    private void OnPointerPressed(InputAction.CallbackContext ctx)
    {
        // Si el puntero está sobre UI, no empezamos drag
        if (IsPointerOverUI(out int pointerId))
            return;

        // Raycast pantalla -> mundo, solo empezamos drag si clic sobre este objeto (o su capa)
        if (!ScreenRayHitsThis(pointerId))
            return;

        dragging = true;
        activePointerId = pointerId; // recuerda quién inició el drag (mouse/touch)
    }

    private void OnPointerReleased(InputAction.CallbackContext ctx)
    {
        dragging = false;
        activePointerId = -1;
    }

    // ---------- HELPERS ----------
    private bool IsPointerOverUI(out int pointerId)
    {
        pointerId = -1;

        // Mouse
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            pointerId = PointerInputModule.kMouseLeftId; // -1
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return true;
        }

        // Touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId))
                return true;
        }

        return false;
    }

    private bool ScreenRayHitsThis(int pointerId)
    {
        if (pointerPosition == null) return false;

        Vector2 pos = pointerPosition.action.ReadValue<Vector2>();
        var camTf = cam != null ? cam.transform : Camera.main.transform;
        var ray = (cam != null ? cam.Lens : null) != null
            ? new Ray(cam.transform.position, cam.transform.forward) // fallback simple
            : Camera.main.ScreenPointToRay(pos);

        // Si usas CinemachineCamera, lo normal es usar Camera.main para el ray:
        if (cam != null && Camera.main != null)
            ray = Camera.main.ScreenPointToRay(pos);

        if (Physics.Raycast(ray, out var hit, 1000f, interactMask, QueryTriggerInteraction.Ignore))
        {
            // true si golpeó este objeto o un hijo suyo
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }
        return false;
    }
}
