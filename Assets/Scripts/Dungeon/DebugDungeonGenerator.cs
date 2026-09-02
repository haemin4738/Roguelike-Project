using System.Collections.Generic;
using UnityEngine;

// 프리팹 없이 런타임에 테스트 방을 생성. 실제 방 프리팹 준비되면 DungeonGenerator로 교체.
public class DebugDungeonGenerator : MonoBehaviour
{
    [SerializeField] int roomCount = 3;
    [SerializeField] float roomWidth = 40f;
    [SerializeField] float roomHeight = 15f;

    [Header("Enemy Prefabs")]
    [SerializeField] GameObject[] normalEnemyPrefabs;
    [SerializeField] GameObject bossEnemyPrefab;

    [Header("Sprites (없으면 단색 블록으로 대체)")]
    [SerializeField] Sprite floorSprite;
    [SerializeField] Sprite wallSprite;
    [SerializeField] Sprite platformSprite;
    [SerializeField] Sprite doorSprite;

    [Header("Background Tiles (여러 개 할당하면 랜덤 배치)")]
    [SerializeField] Sprite[] floorVariants;   // floor_1 ~ floor_8
    [SerializeField] Color bgTint = new Color(0.35f, 0.35f, 0.4f, 1f);

    Sprite _whiteSprite;

    void Awake() => _whiteSprite = MakeWhiteSprite();

    void Start()
    {
        var rooms = BuildRooms();
        LinkRooms(rooms);
        RoomManager.Instance.SetStartRoom(rooms[0]);
        PlacePlayer();
    }

    List<Room> BuildRooms()
    {
        var rooms = new List<Room>();
        for (int i = 0; i < roomCount; i++)
        {
            var type = i == 0 ? RoomType.Start
                     : i == roomCount - 1 ? RoomType.Boss
                     : RoomType.Normal;
            var room = MakeRoom(i, type);
            room.transform.position = new Vector3(i * roomWidth, 0f, 0f);
            SpawnEnemies(type, room.transform.position, room.transform);
            rooms.Add(room);
        }
        return rooms;
    }

    void LinkRooms(List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].PrevRoom = i > 0 ? rooms[i - 1] : null;
            rooms[i].NextRoom = i < rooms.Count - 1 ? rooms[i + 1] : null;
            if (rooms[i].leftDoor  != null) rooms[i].leftDoor.gameObject.SetActive(i > 0);
            if (rooms[i].rightDoor != null) rooms[i].rightDoor.gameObject.SetActive(i < rooms.Count - 1);
        }
    }

    Room MakeRoom(int index, RoomType type)
    {
        var go = new GameObject($"Room_{type}_{index}");
        var room = go.AddComponent<Room>();
        room.roomType  = type;
        room.roomWidth = roomWidth;

        Color wallColor = type == RoomType.Start ? new Color(0.3f, 0.6f, 0.3f)
                        : type == RoomType.Boss  ? new Color(0.6f, 0.2f, 0.2f)
                        : new Color(0.4f, 0.4f, 0.5f);
        Color floorColor = wallColor * 0.75f; floorColor.a = 1f;

        // 배경: 바닥 타일 랜덤 배치
        AddTiledBackground(go.transform);

        // 바닥 / 좌벽 / 우벽 / 천장
        AddBlock(go.transform, new Vector2(roomWidth + 2f, 1f),    new Vector3(roomWidth * .5f, -0.5f),            floorColor, floorSprite);
        AddBlock(go.transform, new Vector2(1f, roomHeight),        new Vector3(-0.5f, roomHeight * .5f),            wallColor,  wallSprite);
        AddBlock(go.transform, new Vector2(1f, roomHeight),        new Vector3(roomWidth + 0.5f, roomHeight * .5f), wallColor,  wallSprite);
        AddBlock(go.transform, new Vector2(roomWidth + 2f, 1f),    new Vector3(roomWidth * .5f, roomHeight + .5f),  wallColor,  wallSprite);

        // Normal·Boss: 중간 발판 (하향점프 가능)
        if (type != RoomType.Start)
            AddPlatform(go.transform, 12f, new Vector3(roomWidth * .5f, roomHeight * .4f), wallColor * 1.3f);

        room.leftEntry  = MakeMarker(go.transform, "LeftEntry",  new Vector3(5f, 2f));
        room.rightEntry = MakeMarker(go.transform, "RightEntry", new Vector3(roomWidth - 5f, 2f));

        room.leftDoor  = AddDoor(go.transform, DoorConnector.Side.Left,  new Vector3(1.5f, 1.5f));
        room.rightDoor = AddDoor(go.transform, DoorConnector.Side.Right, new Vector3(roomWidth - 1.5f, 1.5f));

        return room;
    }

    // 1×1 unit 타일을 격자로 배치 — floorVariants 배열에서 랜덤 선택
    void AddTiledBackground(Transform parent)
    {
        bool hasVariants = floorVariants != null && floorVariants.Length > 0;
        int cols = Mathf.RoundToInt(roomWidth);
        int rows = Mathf.RoundToInt(roomHeight);

        var bgRoot = new GameObject("Background");
        bgRoot.transform.SetParent(parent, false);

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                var tile = new GameObject($"T{x}_{y}");
                tile.transform.SetParent(bgRoot.transform, false);
                tile.transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0f);

                var sr = tile.AddComponent<SpriteRenderer>();
                if (hasVariants)
                {
                    sr.sprite = floorVariants[Random.Range(0, floorVariants.Length)];
                    sr.color  = bgTint;
                }
                else
                {
                    sr.sprite = _whiteSprite;
                    sr.color  = new Color(0.1f, 0.1f, 0.15f);
                    sr.drawMode = SpriteDrawMode.Tiled;
                    sr.size     = Vector2.one;
                }
                sr.sortingOrder = -10;
            }
        }
    }

    void AddBlock(Transform parent, Vector2 size, Vector3 localPos, Color fallbackColor, Sprite sprite = null)
    {
        var go = new GameObject("Block");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one;
        go.layer = LayerMask.NameToLayer("Ground");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = sprite != null ? sprite : _whiteSprite;
        sr.color    = sprite != null ? Color.white : fallbackColor;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size     = size;

        go.AddComponent<BoxCollider2D>().size = size;
    }

    void AddPlatform(Transform parent, float width, Vector3 localPos, Color fallbackColor)
    {
        var go = new GameObject("Platform");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one;
        go.layer = LayerMask.NameToLayer("Ground");

        var size = new Vector2(width, 0.5f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = platformSprite != null ? platformSprite : _whiteSprite;
        sr.color    = platformSprite != null ? Color.white : fallbackColor;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size     = size;

        var col = go.AddComponent<BoxCollider2D>();
        col.size           = size;
        col.usedByEffector = true;
        go.AddComponent<PlatformEffector2D>().useOneWay = true;
    }

    Transform MakeMarker(Transform parent, string name, Vector3 localPos)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = localPos;
        return t;
    }

    DoorConnector AddDoor(Transform parent, DoorConnector.Side side, Vector3 localPos)
    {
        var go = new GameObject($"Door_{side}");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        if (doorSprite != null)
        {
            var visual = new GameObject("DoorVisual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = new Vector3(2f, 3f, 1f);

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite       = doorSprite;
            sr.sortingOrder = 1;
        }

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(3f, 4f);

        var door = go.AddComponent<DoorConnector>();
        door.side = side;
        return door;
    }

    void SpawnEnemies(RoomType type, Vector3 roomPos, Transform roomTransform)
    {
        if (type == RoomType.Start) return;

        if (type == RoomType.Boss)
        {
            if (bossEnemyPrefab != null)
            {
                var e = Instantiate(bossEnemyPrefab, roomPos + new Vector3(roomWidth * 0.5f, 2f), Quaternion.identity);
                e.transform.SetParent(roomTransform);
                e.SetActive(false);
            }
            return;
        }

        if (normalEnemyPrefabs == null || normalEnemyPrefabs.Length == 0) return;
        float[] spawnX = { 10f, 30f };
        foreach (float x in spawnX)
        {
            var prefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
            if (prefab != null)
            {
                var e = Instantiate(prefab, roomPos + new Vector3(x, 1f), Quaternion.identity);
                e.transform.SetParent(roomTransform);
                e.SetActive(false);
            }
        }
    }

    void PlacePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = new Vector3(roomWidth * .5f, 3f, 0f);
    }

    static Sprite MakeWhiteSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1f);
    }
}
