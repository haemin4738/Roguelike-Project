using UnityEngine;
using UnityEditor;

public class ProjectilePrefabGenerator
{
    [MenuItem("Tools/Generate Projectile Prefabs")]
    static void Generate()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Projectiles");

        // (prefabName, spritePath, useCircle, colliderSize, rotOffset, scale)
        var projectiles = new (string name, string sprite, bool circle, float cw, float ch, float scale)[]
        {
            ("Proj_Arrow",       "Assets/Art/Sprites/Items/weapon_arrow.png",        false, 0.8f, 0.2f, 1.0f),
            ("Proj_ThrowingAxe", "Assets/Art/Sprites/Items/weapon_throwing_axe.png", false, 0.6f, 0.4f, 0.8f),
            ("Proj_GreenOrb",    "Assets/Art/Sprites/Items/flask_green.png",          true,  0.3f, 0.3f, 0.5f),
            ("Proj_RedOrb",      "Assets/Art/Sprites/Items/flask_red.png",            true,  0.3f, 0.3f, 0.5f),
        };

        var prefabMap = new System.Collections.Generic.Dictionary<string, GameObject>();

        foreach (var p in projectiles)
        {
            string path = $"Assets/Prefabs/Projectiles/{p.name}.prefab";

            var go = new GameObject(p.name);
            go.transform.localScale = Vector3.one * p.scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p.sprite);
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (p.circle)
                go.AddComponent<CircleCollider2D>().radius = p.cw;
            else
                go.AddComponent<BoxCollider2D>().size = new Vector2(p.cw, p.ch);

            go.AddComponent<ProjectileBase>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            prefabMap[p.name] = prefab;
            Debug.Log($"[ProjectileGen] {p.name} 생성");
        }

        LinkToWeapons(prefabMap);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ProjectileGen] 완료 — WeaponData projectilePrefab 연결 완료");
    }

    static void LinkToWeapons(System.Collections.Generic.Dictionary<string, GameObject> map)
    {
        var links = new (string assetPath, string projName)[]
        {
            ("Assets/Data/Weapons/weapon_bow.asset",              "Proj_Arrow"),
            ("Assets/Data/Weapons/weapon_bow_2.asset",            "Proj_Arrow"),
            ("Assets/Data/Weapons/weapon_throwing_axe.asset",     "Proj_ThrowingAxe"),
            ("Assets/Data/Weapons/weapon_green_magic_staff.asset", "Proj_GreenOrb"),
            ("Assets/Data/Weapons/weapon_red_magic_staff.asset",   "Proj_RedOrb"),
        };

        foreach (var (assetPath, projName) in links)
        {
            var wd = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
            if (wd == null) { Debug.LogWarning($"[ProjectileGen] 없음: {assetPath} — 먼저 Generate Weapon Assets 실행"); continue; }
            if (!map.TryGetValue(projName, out var prefab)) continue;
            wd.projectilePrefab = prefab;
            EditorUtility.SetDirty(wd);
        }
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
    }
}
