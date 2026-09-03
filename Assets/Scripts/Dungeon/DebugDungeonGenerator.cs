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

    [Header("Hazards")]
    [SerializeField] Sprite[] spikeFrames;  // floor_spikes_anim_f0 ~ f3

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
        // 방 간 문 연결 높이: Start→Normal은 낮게(1.5/4), 이후 연결은 자유롭게(1.5/4/7)
        float[] startTiers = { 1.5f, 4f };
        float[] allTiers   = { 1.5f, 4f, 7f };
        var connY = new float[Mathf.Max(roomCount - 1, 0)];
        for (int i = 0; i < connY.Length; i++)
        {
            var tiers = i == 0 ? startTiers : allTiers;
            connY[i] = tiers[Random.Range(0, tiers.Length)];
        }

        var rooms = new List<Room>();
        for (int i = 0; i < roomCount; i++)
        {
            var type = i == 0 ? RoomType.Start
                     : i == roomCount - 1 ? RoomType.Boss
                     : RoomType.Normal;
            float lY = i > 0             ? connY[i - 1] : 1.5f;
            float rY = i < roomCount - 1 ? connY[i]     : 1.5f;
            var room = MakeRoom(i, type, lY, rY);
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

    Room MakeRoom(int index, RoomType type, float leftDoorY = 1.5f, float rightDoorY = 1.5f)
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

        // Normal·Boss: 오픈형(발판) or 미로형(솔리드 블록) 랜덤 선택
        if (type != RoomType.Start)
        {
            bool isMaze = type == RoomType.Normal && Random.value < 0.5f;
            if (isMaze) AddMazeBlocks(go.transform, wallColor * 1.3f);
            else        AddPlatforms(go.transform, wallColor * 1.3f);
            AddSpikeTraps(go.transform);
        }

        // 문 앞 발판 — 벽에 붙게(x=3.5), 맵 블록과 동일한 스프라이트
        Color entryColor = wallColor * 1.3f; entryColor.a = 1f;
        AddBlock(go.transform, new Vector2(7f, 1f), new Vector3(3.5f,              leftDoorY  - 1f), entryColor, wallSprite);
        AddBlock(go.transform, new Vector2(7f, 1f), new Vector3(roomWidth - 3.5f, rightDoorY - 1f), entryColor, wallSprite);

        room.leftEntry  = MakeMarker(go.transform, "LeftEntry",  new Vector3(5f,              leftDoorY  + 0.5f));
        room.rightEntry = MakeMarker(go.transform, "RightEntry", new Vector3(roomWidth - 5f, rightDoorY + 0.5f));

        room.leftDoor  = AddDoor(go.transform, DoorConnector.Side.Left,  new Vector3(1.5f,              leftDoorY));
        room.rightDoor = AddDoor(go.transform, DoorConnector.Side.Right, new Vector3(roomWidth - 1.5f, rightDoorY));

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
            visual.transform.localScale = new Vector3(1f, 1f, 1f);

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite       = doorSprite;
            sr.sortingOrder = 1;
        }

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(1.5f, 1.5f);

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

        // 후보 x 위치들 — Ground 콜라이더와 겹치지 않는 곳에 스폰
        float[] candidatesLeft  = { 8f,  6f, 12f, 16f };
        float[] candidatesRight = { 32f, 34f, 28f, 24f };
        var groundMask = LayerMask.GetMask("Ground");

        foreach (var candidates in new[] { candidatesLeft, candidatesRight })
        {
            float spawnX = candidates[0];
            foreach (float cx in candidates)
            {
                if (Physics2D.OverlapCircle(roomPos + new Vector3(cx, 1.5f), 0.4f, groundMask) == null)
                { spawnX = cx; break; }
            }
            var prefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
            if (prefab != null)
            {
                var e = Instantiate(prefab, roomPos + new Vector3(spawnX, 1f), Quaternion.identity);
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

    // 미로형: 사방이 막힌 솔리드 블록으로 통로 분할 — 4종 랜덤
    void AddMazeBlocks(Transform parent, Color fallbackColor)
    {
        switch (Random.Range(0, 4))
        {
            case 0: // 문형 — 좌우 기둥(4u) + 상단 발판 2개
                AddBlock(parent, new Vector2(2f, 4f), new Vector3(12f, 2f),  fallbackColor, wallSprite);
                AddBlock(parent, new Vector2(2f, 4f), new Vector3(28f, 2f),  fallbackColor, wallSprite);
                AddPlatform(parent, 12f, new Vector3(20f, 7f),  fallbackColor);
                AddPlatform(parent, 8f,  new Vector3(20f, 10f), fallbackColor);
                break;
            case 1: // 낮은 천장 — 좌우에 천장 블록, 중앙만 뚫림
                AddBlock(parent, new Vector2(13f, 1f), new Vector3(9f,  8f),  fallbackColor, wallSprite);
                AddBlock(parent, new Vector2(13f, 1f), new Vector3(31f, 10f), fallbackColor, wallSprite);
                AddPlatform(parent, 8f, new Vector3(20f, 5f), fallbackColor);
                break;
            case 2: // 계단 블록 — 좌측 낮은 솔리드 / 우측 높은 솔리드
                AddBlock(parent, new Vector2(10f, 3f), new Vector3(9f,  1.5f), fallbackColor, wallSprite);
                AddBlock(parent, new Vector2(10f, 5f), new Vector3(31f, 2.5f), fallbackColor, wallSprite);
                AddPlatform(parent, 7f, new Vector3(9f, 5f), fallbackColor);
                break;
            case 3: // 3단 기둥 — 3개 기둥으로 4개 통로 생성
                AddBlock(parent, new Vector2(2f, 3f), new Vector3(11f, 1.5f), fallbackColor, wallSprite);
                AddBlock(parent, new Vector2(2f, 5f), new Vector3(20f, 2.5f), fallbackColor, wallSprite);
                AddBlock(parent, new Vector2(2f, 3f), new Vector3(29f, 1.5f), fallbackColor, wallSprite);
                AddPlatform(parent, 10f, new Vector3(20f, 8f), fallbackColor);
                break;
        }
    }

    // 던그리드 스타일: 4종 레이아웃 중 랜덤 — 계단(↗), 계단(↖), 대칭 2단, 비대칭
    // 플랫폼 높이: y=3 / 5.5 / 8 (단계당 ~2.5u 차이, 기본 점프로 연속 도달 가능)
    void AddPlatforms(Transform parent, Color fallbackColor)
    {
        float[][][] layouts =
        {
            new[] { new[]{0.20f,0.20f,10f}, new[]{0.55f,0.37f,8f}, new[]{0.80f,0.53f,7f} }, // 계단(↗)
            new[] { new[]{0.80f,0.20f,10f}, new[]{0.45f,0.37f,8f}, new[]{0.20f,0.53f,7f} }, // 계단(↖)
            new[] { new[]{0.22f,0.33f,10f}, new[]{0.78f,0.33f,10f}, new[]{0.50f,0.53f,8f} }, // 대칭 2단
            new[] { new[]{0.25f,0.20f,12f}, new[]{0.75f,0.37f,7f}, new[]{0.40f,0.53f,6f} }, // 비대칭
        };

        foreach (var p in layouts[Random.Range(0, layouts.Length)])
            AddPlatform(parent, p[2], new Vector3(roomWidth * p[0], roomHeight * p[1]), fallbackColor);
    }

    void AddSpikeTraps(Transform parent)
    {
        if (spikeFrames == null || spikeFrames.Length == 0) return;
        int count = Random.Range(1, 4);
        for (int i = 0; i < count; i++)
            AddSpike(parent, new Vector3(Random.Range(8f, roomWidth - 8f), 0f));
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
        col.size   = new Vector2(1f, 1f);
        col.offset = new Vector2(0f, 0f);

        go.AddComponent<SpikeTrap>().frames = spikeFrames;
    }

    static Sprite MakeWhiteSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1f);
    }
}
