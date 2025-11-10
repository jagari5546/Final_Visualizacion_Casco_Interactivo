using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class HelmetDragScript : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Cámara real (la que tiene CinemachineBrain). Si se deja vacío usa Camera.main o busca una con Brain.")]
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private CinemachineCamera referenceCinemachine;

    [Header("Layers clickeables (padre + hijos)")]
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference pointerPress;
    [SerializeField] private InputActionReference pointerDelta;
    [SerializeField] private InputActionReference pointerPosition;

    [Header("Rotación manual")]
    [SerializeField] private float rotationSpeed = 0.35f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private bool invertX = false;


    [Header("Auto-rotación / Inercia / Enderezado")]
    [SerializeField] private float autoRotateSpeed = 15f;
    [SerializeField] private float inertiaDamping = 3.5f;
    [SerializeField] private float uprightReturnSpeed = 2f;

    private Camera cam;
    private bool dragging;
    private Vector3 lastAngularVel; // (x=pitch, y=yaw)

    void OnEnable()
    {
        pointerPress?.action.Enable();
        pointerDelta?.action.Enable();
        pointerPosition?.action.Enable();
    }
    void OnDisable()
    {
        pointerPress?.action.Disable();
        pointerDelta?.action.Disable();
        pointerPosition?.action.Disable();
    }

    void Awake() { ResolveCamera(); Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

    void ResolveCamera()
    {
        if (raycastCamera) { cam = raycastCamera; return; }
        cam = Camera.main;
        if (!cam)
        {
            var brain = FindObjectOfType<CinemachineBrain>();
            if (brain) cam = brain.GetComponent<Camera>();
        }
        if (!cam)
        {
            var any = FindObjectOfType<Camera>();
            if (any && any.enabled) cam = any;
        }
    }

    void Update()
    {
        if (!cam) ResolveCamera();

        if (pointerPress && pointerPress.action.WasPressedThisFrame())
        {
            if (RayHitsThis())
            {
                dragging = true;
                lastAngularVel = Vector3.zero;
            }
        }

        // STOP drag
        if (pointerPress && pointerPress.action.WasReleasedThisFrame())
        {
            dragging = false;
        }

        if (dragging)
        {
            Vector2 delta = pointerDelta ? pointerDelta.action.ReadValue<Vector2>() : Vector2.zero;
            if (delta.sqrMagnitude > Mathf.Epsilon)
            {
                float dx = delta.x * rotationSpeed * Time.deltaTime * (invertX ? -1f : 1f);
                float dy = delta.y * rotationSpeed * Time.deltaTime * (invertY ? 1 : -1);

                Transform camTf = referenceCinemachine ? referenceCinemachine.transform : cam.transform;

                transform.Rotate(transform.up, dx, Space.World);
                transform.Rotate(camTf.right, dy, Space.World);

                lastAngularVel = new Vector3(dy, dx, 0f);
            }
            return;
        }

        if (lastAngularVel.sqrMagnitude > 0.0001f)
        {
            Transform camTf = referenceCinemachine ? referenceCinemachine.transform : cam.transform;
            transform.Rotate(transform.up, lastAngularVel.y, Space.World);
            transform.Rotate(camTf.right, lastAngularVel.x, Space.World);
            lastAngularVel = Vector3.Lerp(lastAngularVel, Vector3.zero, Time.deltaTime * inertiaDamping);
        }

        transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);

        Vector3 f = transform.forward; f.y = 0f; if (f.sqrMagnitude < 0.0001f) f = Vector3.forward;
        Quaternion target = Quaternion.LookRotation(f.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * uprightReturnSpeed);
    }

    bool RayHitsThis()
    {
        if (pointerPosition == null || cam == null) return false;
        Vector2 pos = pointerPosition.action.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out var hit, 5000f, interactMask, QueryTriggerInteraction.Ignore))
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        return false;
    }
}
