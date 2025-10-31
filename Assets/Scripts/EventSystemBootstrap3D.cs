using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// Asegura EventSystem + módulo UI correcto y PhysicsRaycaster en la cámara.
/// Pon este script en un GameObject vacío de la escena (o en el mismo objeto del casco).
[DefaultExecutionOrder(-1000)]
public class EventSystemBootstrap3D : MonoBehaviour
{
    [SerializeField] private Camera targetCamera; // si lo dejas vacío usa Camera.main

    void Awake()
    {
        // EventSystem
        var es = FindObjectOfType<EventSystem>();
        if (es == null)
        {
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
        }

        // Módulo UI según sistema de input activo
#if ENABLE_INPUT_SYSTEM
        if (es.GetComponent<InputSystemUIInputModule>() == null &&
            es.GetComponent<StandaloneInputModule>() == null)
        {
            es.gameObject.AddComponent<InputSystemUIInputModule>();
        }
#else
        if (es.GetComponent<StandaloneInputModule>() == null &&
            es.GetComponent<BaseInputModule>() == null)
        {
            es.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif

        // PhysicsRaycaster en la cámara
        var cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam != null && cam.GetComponent<PhysicsRaycaster>() == null)
        {
            cam.gameObject.AddComponent<PhysicsRaycaster>();
        }
    }
}