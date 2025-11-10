using UnityEngine;

public class RaycastDebug3D : MonoBehaviour
{
    public LayerMask mask = ~0;
    void Update()
    {
        if (!Camera.main) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 10000f, mask))
            Debug.Log($"Hit: {hit.transform.name} (layer {LayerMask.LayerToName(hit.transform.gameObject.layer)})");
    }
}