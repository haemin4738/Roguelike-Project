using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float xSmoothTime = 0.12f;
    [SerializeField] float ySmoothTime = 0.25f;
    [SerializeField] Vector2 offset;

    float _vx, _vy;

    void LateUpdate()
    {
        if (target == null) return;

        float tx = target.position.x + offset.x;
        float ty = target.position.y + offset.y;

        float nx = Mathf.SmoothDamp(transform.position.x, tx, ref _vx, xSmoothTime);
        float ny = Mathf.SmoothDamp(transform.position.y, ty, ref _vy, ySmoothTime);
        transform.position = new Vector3(nx, ny, transform.position.z);
    }
}
