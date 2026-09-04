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

        var cam = GetComponent<Camera>();
        var room = RoomManager.Instance?.CurrentRoom;
        if (cam != null && room != null)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            nx = Mathf.Clamp(nx, room.CamMinX + halfW, room.CamMaxX - halfW);
            ny = Mathf.Clamp(ny, room.CamMinY - 1f + halfH, room.CamMaxY + 1f - halfH);
        }

        transform.position = new Vector3(nx, ny, transform.position.z);
    }
}
