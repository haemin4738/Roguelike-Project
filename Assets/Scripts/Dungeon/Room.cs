using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public RoomType roomType;
    public float roomWidth = 40f;
    public float roomHeight = 15f;

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
    public float CamMinY => transform.position.y;
    public float CamMaxY => transform.position.y + roomHeight;
}
