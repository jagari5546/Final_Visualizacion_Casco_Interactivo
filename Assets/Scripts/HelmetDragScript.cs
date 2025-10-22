using UnityEngine;
using Unity.Cinemachine;

public class HelmetDragScript : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera; 

    private Vector3 mPrevPos = Vector3.zero;
    private Vector3 mPosDelta = Vector3.zero; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Vector3.Dot(transform.up, Vector3.up)>= 0)
            {
                transform.Rotate(transform.up, -Vector3.Dot(mPosDelta, _camera.transform.right),Space.World);

            }
            else
            {
                transform.Rotate(transform.up, Vector3.Dot(mPosDelta, _camera.transform.right),Space.World);

            }
            transform.Rotate(_camera.transform.right, Vector3.Dot(mPosDelta, _camera.transform.right),Space.World);
        }
        mPrevPos = Input.mousePosition;
    }
}
