using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] FloorConfig config;

    readonly List<Room> _rooms = new();

    void Start() => Generate();

    void Generate()
    {
        PlaceRooms(BuildSequence());
        RoomManager.Instance.SetStartRoom(_rooms[0]);
    }

    List<Room> BuildSequence()
    {
        var seq = new List<Room> { config.startRoomPrefab };

        var pool = new List<Room>(config.normalRoomPrefabs);
        for (int i = 0; i < config.normalRoomCount; i++)
        {
            if (pool.Count == 0) pool.AddRange(config.normalRoomPrefabs);
            int idx = Random.Range(0, pool.Count);
            seq.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        seq.Add(config.bossRoomPrefab);
        return seq;
    }

    void PlaceRooms(List<Room> prefabs)
    {
        float x = 0f;
        Room prev = null;

        for (int i = 0; i < prefabs.Count; i++)
        {
            var room = Instantiate(prefabs[i], new Vector3(x, 0f, 0f), Quaternion.identity);
            room.PrevRoom = prev;
            if (prev != null) prev.NextRoom = room;

            if (room.leftDoor != null)  room.leftDoor.gameObject.SetActive(i > 0);
            if (room.rightDoor != null) room.rightDoor.gameObject.SetActive(i < prefabs.Count - 1);

            _rooms.Add(room);
            prev = room;
            x += room.roomWidth;
        }
    }
}
