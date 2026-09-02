using UnityEngine;
using UnityEngine.InputSystem;

public class DoorConnector : MonoBehaviour
{
    public enum Side { Left, Right }
    public Side side;

    Room _room;
    Room Room => _room ??= GetComponentInParent<Room>();

    bool _playerNearby;

    void Update()
    {
        if (!_playerNearby || !Keyboard.current.fKey.wasPressedThisFrame) return;

        if (Room.roomType == RoomType.Normal && side == Side.Right
            && Room.GetComponentsInChildren<EnemyBase>().Length > 0)
            return;

        RoomManager.Instance.Transition(Room, side);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = false;
    }
}
