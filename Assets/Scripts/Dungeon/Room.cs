using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public RoomType roomType;
    public float roomWidth = 40f;

    [Header("Player Entry Points")]
    public Transform leftEntry;
    public Transform rightEntry;

    [Header("Doors")]
    public DoorConnector leftDoor;
    public DoorConnector rightDoor;

    [HideInInspector] public Room PrevRoom;
    [HideInInspector] public Room NextRoom;

    public float CamMinX => transform.position.x;
    public float CamMaxX => transform.position.x + roomWidth;
}
