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
        if (_playerNearby && Keyboard.current.fKey.wasPressedThisFrame)
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
