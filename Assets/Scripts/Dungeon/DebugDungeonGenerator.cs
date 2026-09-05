using System.Collections.Generic;
using UnityEngine;

public class DebugDungeonGenerator : MonoBehaviour
{
    [SerializeField] int roomCount = 3;
    [SerializeField] float roomWidth = 120f;
    [SerializeField] float roomHeight = 70f;

    [Header("Enemy Prefabs")]
    [SerializeField] GameObject[] normalEnemyPrefabs;
    [SerializeField] GameObject bossEnemyPrefab;

    [Header("Scale")]
    [SerializeField] float bossScale = 2f;

    [Header("Boss Room Size")]
    [SerializeField] float bossRoomWidth = 40f;
    [SerializeField] float bossRoomHeight = 20f;

    [Header("Sprites (없으면 단색 블록으로 대체)")]
    [SerializeField] Sprite floorSprite;
    [SerializeField] Sprite wallSprite;
    [SerializeField] Sprite platformSprite;
    [SerializeField] Sprite doorSprite;

    [Header("Background Tiles (여러 개 할당하면 랜덤 배치)")]
    [SerializeField] Sprite[] floorVariants;
    [SerializeField] Color bgTint = new Color(0.35f, 0.35f, 0.4f, 1f);

    [Header("Hazards")]
    [SerializeField] Sprite[] spikeFrames;

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
        float[] startTiers = { 1.5f, 4f };
        float[] allTiers = { 1.5f, 4f, 7f };
        var connY = new float[Mathf.Max(roomCount - 1, 0)];
        for (int i = 0; i < connY.Length; i++)
        {
            var tiers = i == 0 ? startTiers : allTiers;
            connY[i] = tiers[Random.Range(0, tiers.Length)];
        }

        var rooms = new List<Room>();
        float xPos = 0f;
        for (int i = 0; i < roomCount; i++)
        {
            var type = i == 0 ? RoomType.Start
                     : i == roomCount - 1 ? RoomType.Boss
                     : RoomType.Normal;
            float lY = i > 0 ? connY[i - 1] : 1.5f;
            float rY = i < roomCount - 1 ? connY[i] : 1.5f;
            var room = MakeRoom(i, type, lY, rY);
            room.transform.position = new Vector3(xPos, 0f, 0f);
            SpawnEnemies(type, room.transform.position, room.transform, roomHeight * 0.4f);
            rooms.Add(room);
            xPos += room.roomWidth;
        }
        return rooms;
    }

    void LinkRooms(List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].PrevRoom = i > 0 ? rooms[i - 1] : null;
            rooms[i].NextRoom = i < rooms.Count - 1 ? rooms[i + 1] : null;
            if (rooms[i].leftDoor != null) rooms[i].leftDoor.gameObject.SetActive(i > 0);
            if (rooms[i].rightDoor != null) rooms[i].rightDoor.gameObject.SetActive(i < rooms.Count - 1);
        }
    }

    Room MakeRoom(int index, RoomType type, float leftDoorY = 1.5f, float rightDoorY = 1.5f)
    {
        float origW = roomWidth, origH = roomHeight;
        if (type == RoomType.Boss) { roomWidth = bossRoomWidth; roomHeight = bossRoomHeight; }

        var go = new GameObject($"Room_{type}_{index}");
        var room = go.AddComponent<Room>();
        room.roomType = type;
        room.roomWidth = roomWidth;
        room.roomHeight = roomHeight;

        Color wallColor = type == RoomType.Start ? new Color(0.3f, 0.6f, 0.3f)
                        : type == RoomType.Boss ? new Color(0.6f, 0.2f, 0.2f)
                        : new Color(0.4f, 0.4f, 0.5f);
        Color floorColor = wallColor * 0.75f; floorColor.a = 1f;

        float midY = roomHeight * 0.4f;

        AddTiledBackground(go.transform);

        // 외벽 4면
        AddBlock(go.transform, new Vector2(roomWidth + 2f, 1f), new Vector3(roomWidth * .5f, -0.5f), floorColor, floorSprite);
        AddBlock(go.transform, new Vector2(1f, roomHeight), new Vector3(-0.5f, roomHeight * .5f), wallColor, wallSprite);
        AddBlock(go.transform, new Vector2(1f, roomHeight), new Vector3(roomWidth + 0.5f, roomHeight * .5f), wallColor, wallSprite);
        AddBlock(go.transform, new Vector2(roomWidth + 2f, 1f), new Vector3(roomWidth * .5f, roomHeight + .5f), wallColor, wallSprite);

        if (type == RoomType.Start)
            AddTownZones(go.transform);
        else
            AddMidFloor(go.transform, wallColor * 1.1f, midY);

        if (type != RoomType.Start)
        {
            bool isMaze = type == RoomType.Normal && Random.value < 0.5f;
            if (isMaze) AddMazeBlocks(go.transform, wallColor * 1.3f, midY);
            else AddPlatforms(go.transform, wallColor * 1.3f, midY);
            AddSpikeTraps(go.transform, midY);
        }

        Color entryColor = wallColor * 1.3f; entryColor.a = 1f;
        AddBlock(go.transform, new Vector2(7f, 1f), new Vector3(3.5f, leftDoorY - 1f), entryColor, wallSprite);
        AddBlock(go.transform, new Vector2(7f, 1f), new Vector3(roomWidth - 3.5f, rightDoorY - 1f), entryColor, wallSprite);

        room.leftEntry = MakeMarker(go.transform, "LeftEntry", new Vector3(5f, leftDoorY + 0.5f));
        room.rightEntry = MakeMarker(go.transform, "RightEntry", new Vector3(roomWidth - 5f, rightDoorY + 0.5f));

        room.leftDoor = AddDoor(go.transform, DoorConnector.Side.Left, new Vector3(1.5f, leftDoorY));
        room.rightDoor = AddDoor(go.transform, DoorConnector.Side.Right, new Vector3(roomWidth - 1.5f, rightDoorY));

        roomWidth = origW; roomHeight = origH;
        return room;
    }

    // 중간층: one-way 플랫폼 — 아래서 점프로 통과, S+점프로 낙하
    void AddMidFloor(Transform parent, Color color, float midY)
    {
        AddPlatform(parent, roomWidth - 2f, new Vector3(roomWidth * 0.5f, midY), color);
    }

    // ponytail: 단일 Tiled 렌더러 — 스프라이트 Import Settings Mesh Type=Full Rect 필요
    void AddTiledBackground(Transform parent)
    {
        bool hasVariants = floorVariants != null && floorVariants.Length > 0;
        var bg = new GameObject("Background");
        bg.transform.SetParent(parent, false);
        bg.transform.localPosition = new Vector3(roomWidth * 0.5f, roomHeight * 0.5f, 0f);

        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = hasVariants ? floorVariants[Random.Range(0, floorVariants.Length)] : _whiteSprite;
        sr.color = hasVariants ? bgTint : new Color(0.1f, 0.1f, 0.15f);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(roomWidth, roomHeight);
        sr.sortingOrder = -10;
    }

    void AddBlock(Transform parent, Vector2 size, Vector3 localPos, Color fallbackColor, Sprite sprite = null)
    {
        var go = new GameObject("Block");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.layer = LayerMask.NameToLayer("Ground");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : _whiteSprite;
        sr.color = sprite != null ? Color.white : fallbackColor;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;

        go.AddComponent<BoxCollider2D>().size = size;
    }

    void AddPlatform(Transform parent, float width, Vector3 localPos, Color fallbackColor)
    {
        var go = new GameObject("Platform");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.layer = LayerMask.NameToLayer("Ground");

        var size = new Vector2(width, 0.5f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = platformSprite != null ? platformSprite : _whiteSprite;
        sr.color = platformSprite != null ? Color.white : fallbackColor;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
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
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = doorSprite;
            sr.sortingOrder = 1;
        }

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.5f, 1.5f);

        var door = go.AddComponent<DoorConnector>();
        door.side = side;
        return door;
    }

    void SpawnEnemies(RoomType type, Vector3 roomPos, Transform roomTransform, float midY)
    {
        if (type == RoomType.Start) return;

        if (type == RoomType.Boss)
        {
            if (bossEnemyPrefab != null)
            {
                var e = Instantiate(bossEnemyPrefab, roomPos + new Vector3(roomWidth * 0.5f, 2f), Quaternion.identity);
                e.transform.SetParent(roomTransform);
                e.transform.localScale = Vector3.one * bossScale;
                e.SetActive(false);
            }
            return;
        }

        if (normalEnemyPrefabs == null || normalEnemyPrefabs.Length == 0) return;

        var groundMask = LayerMask.GetMask("Ground");
        Physics2D.SyncTransforms();

        float[] groundRatios = { 0.12f, 0.28f, 0.50f, 0.72f, 0.88f };
        float[] upperRatios  = { 0.18f, 0.38f, 0.58f, 0.78f };

        // 1층: 3~5마리
        var usedX = new List<float>();
        for (int i = 0; i < Random.Range(3, 6); i++)
        {
            float x = PickSpawnX(groundRatios, usedX, roomPos, groundMask);
            SpawnNormal(roomPos + new Vector3(x, 1f), roomTransform);
        }

        // 중간층 플랫폼 위: 2~4마리
        usedX.Clear();
        for (int i = 0; i < Random.Range(2, 5); i++)
        {
            float x = PickSpawnX(upperRatios, usedX, roomPos, groundMask);
            SpawnNormal(roomPos + new Vector3(x, midY + 0.5f), roomTransform);
        }
    }

    void SpawnNormal(Vector3 pos, Transform parent)
    {
        var prefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
        if (prefab == null) return;
        var e = Instantiate(prefab, pos, Quaternion.identity);
        e.transform.SetParent(parent);
        e.SetActive(false);
    }

    float PickSpawnX(float[] zoneRatios, List<float> used, Vector3 roomPos, int groundMask)
    {
        var idxs = new List<int>();
        for (int i = 0; i < zoneRatios.Length; i++) idxs.Add(i);
        for (int i = idxs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (idxs[i], idxs[j]) = (idxs[j], idxs[i]);
        }

        foreach (int idx in idxs)
        {
            float x = roomWidth * zoneRatios[idx];
            bool tooClose = false;
            foreach (float u in used)
                if (Mathf.Abs(x - u) < 10f) { tooClose = true; break; }
            if (tooClose) continue;
            if (Physics2D.OverlapCircle(roomPos + new Vector3(x, 1.5f), 0.4f, groundMask) == null)
            {
                used.Add(x);
                return x;
            }
        }
        return roomWidth * zoneRatios[Random.Range(0, zoneRatios.Length)];
    }

    void AddMazeBlocks(Transform parent, Color fallbackColor, float midY)
    {
        float third = roomWidth / 3f;
        // 하단 층
        for (int zone = 0; zone < 3; zone++)
        {
            float cx = third * zone + third * 0.5f;
            switch (Random.Range(0, 3))
            {
                case 0:
                    AddBlock(parent, new Vector2(2f, midY * 0.3f), new Vector3(cx - third * 0.2f, midY * 0.15f), fallbackColor, wallSprite);
                    AddBlock(parent, new Vector2(2f, midY * 0.3f), new Vector3(cx + third * 0.2f, midY * 0.15f), fallbackColor, wallSprite);
                    break;
                case 1:
                    AddBlock(parent, new Vector2(third * 0.35f, 1f), new Vector3(cx, midY * 0.3f), fallbackColor, wallSprite);
                    AddPlatform(parent, third * 0.4f, new Vector3(cx, midY * 0.55f), fallbackColor);
                    break;
                case 2:
                    AddBlock(parent, new Vector2(2f, midY * 0.4f), new Vector3(cx, midY * 0.2f), fallbackColor, wallSprite);
                    break;
            }
        }
        // 상단 층 — 발판만 (솔리드 블록 없음)
        float topH = roomHeight - midY - 1f;
        for (int zone = 0; zone < 3; zone++)
        {
            float cx = third * zone + third * 0.5f;
            float h = midY + 1f + Random.Range(topH * 0.2f, topH * 0.6f);
            AddPlatform(parent, Random.Range(8f, 14f), new Vector3(cx, h), fallbackColor);
        }
    }

    void AddPlatforms(Transform parent, Color fallbackColor, float midY)
    {
        float third = roomWidth / 3f;
        float topH = roomHeight - midY - 1f;

        // 하단 층: 구역당 1개
        for (int zone = 0; zone < 3; zone++)
        {
            float ox = third * zone;
            float h = Random.Range(midY * 0.2f, midY * 0.6f);
            AddPlatform(parent, Random.Range(10f, 16f), new Vector3(ox + third * 0.5f, h), fallbackColor);
        }

        // 상단 층: 구역당 2개
        for (int zone = 0; zone < 3; zone++)
        {
            float ox = third * zone;
            float h1 = midY + 1f + Random.Range(topH * 0.15f, topH * 0.4f);
            float h2 = midY + 1f + Random.Range(topH * 0.5f, topH * 0.75f);
            AddPlatform(parent, Random.Range(10f, 16f), new Vector3(ox + third * 0.35f, h1), fallbackColor);
            AddPlatform(parent, Random.Range(8f, 14f), new Vector3(ox + third * 0.65f, h2), fallbackColor);
        }
    }

    void AddSpikeTraps(Transform parent, float midY)
    {
        if (spikeFrames == null || spikeFrames.Length == 0) return;
        int count = Random.Range(4, 9);
        for (int i = 0; i < count; i++)
        {
            float y = i < count / 2 ? 0f : midY + 1f;
            AddSpike(parent, new Vector3(Random.Range(8f, roomWidth - 8f), y));
        }
    }

    void AddSpike(Transform parent, Vector3 localPos)
    {
        var go = new GameObject("SpikeTrap");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spikeFrames[0];
        sr.sortingOrder = 1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        go.AddComponent<SpikeTrap>().frames = spikeFrames;
    }

    void PlacePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        player.transform.position = new Vector3(roomWidth * .5f, 3f, 0f);
    }

    void AddTownZones(Transform parent)
    {
        // 상점 존: 왼쪽 1/4 지점
        AddZone(parent, TownZone.ZoneType.Shop,
            new Color(0.9f, 0.8f, 0.2f),
            new Vector3(roomWidth * 0.2f, 1f));

        // 어빌리티 존: 오른쪽 3/4 지점
        AddZone(parent, TownZone.ZoneType.Ability,
            new Color(0.4f, 0.6f, 1f),
            new Vector3(roomWidth * 0.8f, 1f));
    }

    void AddZone(Transform parent, TownZone.ZoneType zoneType, Color color, Vector3 localPos)
    {
        // 장식용 카운터 블록
        AddBlock(parent, new Vector2(4f, 2f), localPos + new Vector3(0f, 0.5f), color, null);

        // 트리거 존 (플레이어 감지 영역)
        var go = new GameObject($"Zone_{zoneType}");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos + new Vector3(0f, 2f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(6f, 3f);

        var zone = go.AddComponent<TownZone>();
        zone.Init(zoneType);
        go.name = $"Zone_{zoneType}";
    }

    static Sprite MakeWhiteSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1f);
    }
}
