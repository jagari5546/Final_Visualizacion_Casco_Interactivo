using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using Unity.Cinemachine;

[DefaultExecutionOrder(-1000)]
public class EventSystemBootstrap3D : MonoBehaviour
{
    [Header("Camera that renders world & UI raycasts")]
    [Tooltip("Leave empty to auto-find Camera.main, else a Camera with CinemachineBrain.")]
    [SerializeField] private Camera targetCamera;

    [Header("Auto-find if targetCamera is not set")]
    [SerializeField] private bool autoFindCamera = true;

    void Awake()
    {
        var es = FindObjectOfType<EventSystem>();
        if (es == null)
        {
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
        }

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

        if (targetCamera == null && autoFindCamera)
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
            {
                var brain = FindObjectOfType<CinemachineBrain>();
                if (brain != null) targetCamera = brain.GetComponent<Camera>();
            }

            if (targetCamera == null)
            {
                var anyCam = FindObjectOfType<Camera>();
                if (anyCam != null && anyCam.enabled) targetCamera = anyCam;
            }
        }

        if (targetCamera != null && targetCamera.GetComponent<PhysicsRaycaster>() == null)
        {
            targetCamera.gameObject.AddComponent<PhysicsRaycaster>();
        }
#if UNITY_2D
        // If you raycast 2D colliders too, you can also add:
        // if (targetCamera.GetComponent<Physics2DRaycaster>() == null)
        //     targetCamera.gameObject.AddComponent<Physics2DRaycaster>();
#endif
    }
}
