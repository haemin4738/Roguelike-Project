using UnityEngine;
using UnityEditor;

public class EnemyPrefabGenerator
{
    [MenuItem("Tools/Generate Enemy Prefabs")]
    static void Generate()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Enemies");

        var coinSprites = new Sprite[4];
        for (int i = 0; i < 4; i++)
            coinSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Sprites/Items/coin_anim_f{i}.png");

        // (prefix, name, hp, gold, coins, speed, detRange, atkRange, atkDmg, atkCd, exp)
        var enemies = new (string prefix, string name, float hp, int gold, int coins, float speed, float det, float atkR, float atkDmg, float atkCd, int exp)[]
        {
            ("tiny_zombie",  "좀비 새끼",   20f,   5, 1, 1.5f, 5f, 1.0f,  5f, 1.5f,  10),
            ("goblin",       "고블린",       30f,   8, 1, 3.0f, 6f, 1.0f,  7f, 1.2f,  12),
            ("imp",          "임프",         35f,   8, 1, 3.5f, 6f, 1.0f,  8f, 1.2f,  14),
            ("skelet",       "스켈레톤",     50f,  10, 1, 2.5f, 6f, 1.2f, 10f, 1.5f,  18),
            ("chort",        "초트",         55f,  12, 1, 3.5f, 7f, 1.0f, 12f, 1.2f,  20),
            ("wogol",        "워골",         60f,  12, 1, 2.5f, 6f, 1.3f, 12f, 1.5f,  20),
            ("doc",          "닥터",         65f,  12, 1, 2.0f, 6f, 1.2f, 11f, 1.5f,  20),
            ("pumpkin_dude", "호박 괴물",    70f,  15, 1, 2.0f, 6f, 1.3f, 13f, 1.5f,  22),
            ("masked_orc",   "복면 오크",    90f,  18, 2, 2.5f, 6f, 1.3f, 18f, 1.3f,  30),
            ("orc_warrior",  "오크 전사",   100f,  20, 2, 2.5f, 6f, 1.4f, 20f, 1.3f,  35),
            ("orc_shaman",   "오크 샤먼",    80f,  20, 2, 2.0f, 7f, 1.2f, 15f, 1.5f,  32),
            ("big_zombie",   "거대 좀비",   150f,  30, 2, 1.5f, 5f, 1.5f, 25f, 2.0f,  50),
            ("angel",        "타락 천사",   120f,  25, 2, 3.5f, 7f, 1.2f, 20f, 1.2f,  42),
            ("ogre",         "오거",        200f,  40, 3, 2.0f, 6f, 1.5f, 35f, 2.0f,  70),
            ("big_demon",    "대악마",      400f,  80, 3, 2.5f, 8f, 1.6f, 50f, 1.5f, 120),
        };

        int count = 0;
        foreach (var e in enemies)
        {
            string path = $"Assets/Prefabs/Enemies/{e.prefix}.prefab";

            var go = new GameObject(e.prefix);
            go.tag = "Enemy";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/Art/Sprites/Enemy/{e.prefix}_idle_anim_f0.png");
            sr.sortingOrder = 1;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 2f;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.7f, 1.0f);

            var baseComp = go.AddComponent<EnemyBase>();
            var soBase = new SerializedObject(baseComp);
            soBase.FindProperty("maxHp").floatValue  = e.hp;
            soBase.FindProperty("goldDrop").intValue  = e.gold;
            soBase.FindProperty("coinCount").intValue = e.coins;
            soBase.FindProperty("expReward").intValue = e.exp;
            var coinProp = soBase.FindProperty("coinFrames");
            coinProp.arraySize = coinSprites.Length;
            for (int i = 0; i < coinSprites.Length; i++)
                coinProp.GetArrayElementAtIndex(i).objectReferenceValue = coinSprites[i];
            soBase.ApplyModifiedProperties();

            var ai = go.AddComponent<EnemyAI>();
            var soAI = new SerializedObject(ai);
            soAI.FindProperty("moveSpeed").floatValue      = e.speed;
            soAI.FindProperty("detectionRange").floatValue = e.det;
            soAI.FindProperty("attackRange").floatValue    = e.atkR;
            soAI.FindProperty("attackDamage").floatValue   = e.atkDmg;
            soAI.FindProperty("attackCooldown").floatValue = e.atkCd;
            soAI.FindProperty("groundLayer").intValue      = LayerMask.GetMask("Ground");
            soAI.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[EnemyPrefabGenerator] {count}개 적 프리팹 생성 완료 → Assets/Prefabs/Enemies/");
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
    }
}
