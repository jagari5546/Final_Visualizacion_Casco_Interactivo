using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class HelmetDragScript : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference pointerPress;
    [SerializeField] private InputActionReference pointerDelta;
    [SerializeField] private InputActionReference pointerPosition;

    [Header("Rotación Manual")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private bool invertY = false;

    [Header("Rotación Automática")]
    [SerializeField] private float autoRotateSpeed = 10f;     // grados/seg
    [SerializeField] private float inertiaDamping = 3f;       // entre 1–10
    [SerializeField] private float uprightReturnSpeed = 2f;   // velocidad para enderezarse

    private bool dragging = false;
    private int activePointerId = -1;

    // --- Nuevas variables ---
    private Vector2 lastInputDelta;
    private Vector3 lastAngularVelocity;
    private Quaternion targetUprightRotation;

    void OnEnable()
    {
        if (pointerPress != null)
        {
            pointerPress.action.started   += OnPointerPressed;
            pointerPress.action.canceled  += OnPointerReleased;
            pointerPress.action.Enable();
        }
        if (pointerDelta != null) pointerDelta.action.Enable();
        if (pointerPosition != null) pointerPosition.action.Enable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        targetUprightRotation = Quaternion.identity;
    }

    void OnDisable()
    {
        if (pointerPress != null)
        {
            pointerPress.action.started  -= OnPointerPressed;
            pointerPress.action.canceled -= OnPointerReleased;
            pointerPress.action.Disable();
        }
        if (pointerDelta != null) pointerDelta.action.Disable();
        if (pointerPosition != null) pointerPosition.action.Disable();
    }

    void Update()
    {
        if (dragging)
        {
            Vector2 delta = pointerDelta != null ? pointerDelta.action.ReadValue<Vector2>() : Vector2.zero;
            if (delta.sqrMagnitude <= Mathf.Epsilon) return;

            float dx = delta.x * rotationSpeed * Time.deltaTime;
            float dy = delta.y * rotationSpeed * Time.deltaTime * (invertY ? 1f : -1f);

            Transform camTf = cam != null ? cam.transform : Camera.main.transform;

            // Rotaciones
            transform.Rotate(transform.up, dx, Space.World);
            transform.Rotate(camTf.right, dy, Space.World);

            // Guarda última velocidad angular (para inercia)
            lastAngularVelocity = new Vector3(dy, dx, 0f);
            lastInputDelta = delta;
        }
        else
        {
            // 🔹 Inercia: el casco sigue girando suavemente
            if (lastAngularVelocity.sqrMagnitude > 0.001f)
            {
                transform.Rotate(Vector3.up, lastAngularVelocity.y, Space.World);
                transform.Rotate(cam.transform.right, lastAngularVelocity.x, Space.World);

                // Frenado gradual
                lastAngularVelocity = Vector3.Lerp(lastAngularVelocity, Vector3.zero, Time.deltaTime * inertiaDamping);
            }

            // 🔹 Rotación automática continua (eje Y)
            transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);

            // 🔹 Reajuste vertical (vuelve boca arriba)
            UprightCorrection();
        }
    }

    private void UprightCorrection()
    {
        // Alinea el casco para que su 'up' vuelva a alinearse con el Vector3.up
        Vector3 forward = transform.forward;
        forward.y = 0; // mantiene horizontal
        if (forward.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(forward.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * uprightReturnSpeed);
        }
    }

    private void OnPointerPressed(InputAction.CallbackContext ctx)
    {
        if (IsPointerOverUI(out int pointerId))
            return;

        if (!ScreenRayHitsThis(pointerId))
            return;

        dragging = true;
        activePointerId = pointerId;
        lastAngularVelocity = Vector3.zero; // detiene inercia anterior
    }

    private void OnPointerReleased(InputAction.CallbackContext ctx)
    {
        dragging = false;
        activePointerId = -1;
    }

    private bool IsPointerOverUI(out int pointerId)
    {
        pointerId = -1;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            pointerId = PointerInputModule.kMouseLeftId;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return true;
        }

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
        Ray ray = Camera.main.ScreenPointToRay(pos);

        if (Physics.Raycast(ray, out var hit, 1000f, interactMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }
        return false;
    }
}
