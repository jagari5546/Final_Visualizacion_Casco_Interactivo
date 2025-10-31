using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
    [SerializeField] private Color hoverColor = new Color(0.95f, 0.95f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.85f, 0.85f, 1f);

    [Header("Behaviour")]
    [SerializeField] private bool interactable = true;
    public bool Interactable
    {
        get => interactable;
        set { interactable = value; RefreshVisual(); }
    }

    [Header("Events (como un Button UI)")]
    public UnityEvent onClick;

    bool hovered, pressed;
    MaterialPropertyBlock mpb;

    void Reset()
    {
        targetRenderer = GetComponentInChildren<Renderer>();
    }

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
        RefreshVisual();
    }

    void RefreshVisual()
    {
        if (!targetRenderer) return;

        var c = normalColor;
        if (!interactable)       c = new Color(0.7f,0.7f,0.7f);
        else if (pressed)        c = pressedColor;
        else if (hovered)        c = hoverColor;

        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorProperty, c);
        targetRenderer.SetPropertyBlock(mpb);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!interactable) return;
        hovered = true; RefreshVisual();
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!interactable) return;
        hovered = false; pressed = false; RefreshVisual();
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!interactable) return;
        pressed = true; RefreshVisual();
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!interactable) return;
        pressed = false; RefreshVisual();
    }

    // Click “clásico” de uGUI (up dentro)
    public void OnPointerClick(PointerEventData e)
    {
        if (!interactable) return;
        onClick?.Invoke();
    }

    // Soporte de teclado/gamepad (Submit = Enter/A)
    public void OnSubmit(BaseEventData e)
    {
        if (!interactable) return;
        onClick?.Invoke();
    }
}
