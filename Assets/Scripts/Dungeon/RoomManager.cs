using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    public Room CurrentRoom { get; private set; }

    float _cooldown;

    void Awake() => Instance = this;
    void Update() { if (_cooldown > 0f) _cooldown -= Time.deltaTime; }

    public void SetStartRoom(Room room)
    {
        CurrentRoom = room;
        ActivateEnemies(room);
    }

    void ActivateEnemies(Room room)
    {
        foreach (var enemy in room.GetComponentsInChildren<EnemyBase>(true))
            enemy.gameObject.SetActive(true);
    }

    public void Transition(Room from, DoorConnector.Side side)
    {
        if (from != CurrentRoom || _cooldown > 0f) return;

        Room next = side == DoorConnector.Side.Right ? from.NextRoom : from.PrevRoom;
        if (next == null) return;

        Transform entry = side == DoorConnector.Side.Right ? next.leftEntry : next.rightEntry;
        if (entry == null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.transform.position = entry.position;

        // 카메라 스냅 (CameraFollow가 이후 부드럽게 추적)
        var cam = Camera.main;
        if (cam != null)
        {
            var pos = cam.transform.position;
            pos.x = entry.position.x;
            pos.y = entry.position.y;
            cam.transform.position = pos;
        }

        CurrentRoom = next;
        _cooldown = 0.5f;
        ActivateEnemies(next);
    }
}
