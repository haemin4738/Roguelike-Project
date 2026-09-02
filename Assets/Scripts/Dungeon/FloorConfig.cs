using UnityEngine;

[CreateAssetMenu(fileName = "FloorConfig", menuName = "Roguelike/Floor Config")]
public class FloorConfig : ScriptableObject
{
    public int floorNumber = 1;
    [Range(2, 5)] public int normalRoomCount = 3;
    public Room startRoomPrefab;
    public Room[] normalRoomPrefabs;
    public Room bossRoomPrefab;
}
