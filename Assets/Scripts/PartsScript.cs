using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PartsScript : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    IPointerClickHandler, ISubmitHandler
{
    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string colorProperty = "_BaseColor"; // URP: _BaseColor | Built-in: _Color
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor  = new Color(0.95f, 0.95f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.85f, 0.85f, 1f);

    [Header("Behaviour")]
    [SerializeField] private bool interactable = true;

    [Header("Events (como Button UI)")]
    public UnityEvent onClick;

    [Header("Fallback (New Input System, sin EventSystem)")]
    [SerializeField] private Camera raycastCamera; // si vacío, usa Camera.main
    [SerializeField] private LayerMask rayMask = ~0;
    [SerializeField] private InputActionReference pointerPress;    // <Pointer>/press (opcional)
    [SerializeField] private InputActionReference pointerPosition; // <Pointer>/position (opcional)
    [SerializeField] private bool enableManualRaycast = true;

    bool hovered, pressed;
    MaterialPropertyBlock mpb;
    Camera cam;

    void Reset() { targetRenderer = GetComponentInChildren<Renderer>(); }

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();

        // Autodetect propiedad de color
        if (targetRenderer && targetRenderer.sharedMaterial)
        {
            var mat = targetRenderer.sharedMaterial;
            if (!mat.HasProperty(colorProperty))
                colorProperty = mat.HasProperty("_BaseColor") ? "_BaseColor" :
                                (mat.HasProperty("_Color") ? "_Color" : colorProperty);
        }
        RefreshVisual();
    }

    void OnEnable()
    {
        cam = raycastCamera ? raycastCamera : Camera.main;
        pointerPress?.action.Enable();
        pointerPosition?.action.Enable();
    }

    void OnDisable()
    {
        pointerPress?.action.Disable();
        pointerPosition?.action.Disable();
    }

    void Update()
    {
        if (!enableManualRaycast) return;
        if (!interactable) return;

        if (EventSystem.current != null) return; // si hay EventSystem, deja que IPointer maneje

        // Fallback manual: raycast desde la cámara
        if (!cam) cam = Camera.main;
        if (cam == null || pointerPosition == null) return;

        Vector2 p = pointerPosition.action.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(p);

        bool hitMe = false;
        if (Physics.Raycast(ray, out var hit, 5000f, rayMask, QueryTriggerInteraction.Ignore))
            hitMe = (hit.transform == transform || hit.transform.IsChildOf(transform));

        // Hover
        if (hitMe && !hovered) { hovered = true; pressed = false; RefreshVisual(); }
        else if (!hitMe && hovered) { hovered = false; pressed = false; RefreshVisual(); }

        // Press/Click
        if (pointerPress != null)
        {
            if (pointerPress.action.WasPressedThisFrame() && hitMe) { pressed = true; RefreshVisual(); }
            if (pointerPress.action.WasReleasedThisFrame())
            {
                if (pressed && hitMe) onClick?.Invoke();
                pressed = false; RefreshVisual();
            }
        }
    }

    public bool Interactable { get => interactable; set { interactable = value; RefreshVisual(); } }

    void RefreshVisual()
    {
        if (!targetRenderer) return;
        var c = normalColor;
        if (!interactable) c = new Color(0.7f, 0.7f, 0.7f);
        else if (pressed)  c = pressedColor;
        else if (hovered)  c = hoverColor;

        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorProperty, c);
        targetRenderer.SetPropertyBlock(mpb);
    }

    // ---------- EventSystem (si está configurado) ----------
    public void OnPointerEnter(PointerEventData e) { if (!interactable) return; hovered = true;  RefreshVisual(); }
    public void OnPointerExit (PointerEventData e) { if (!interactable) return; hovered = false; pressed = false; RefreshVisual(); }
    public void OnPointerDown (PointerEventData e) { if (!interactable) return; pressed = true;  RefreshVisual(); }
    public void OnPointerUp   (PointerEventData e) { if (!interactable) return; pressed = false; RefreshVisual(); }
    public void OnPointerClick(PointerEventData e) { if (!interactable) return; onClick?.Invoke(); }
    public void OnSubmit(BaseEventData e)          { if (!interactable) return; onClick?.Invoke(); }
}
