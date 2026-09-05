using UnityEngine;
using UnityEditor;

public class WeaponAssetGenerator
{
    [MenuItem("Tools/Generate Weapon Assets")]
    static void Generate()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/Weapons");
        EnsureFolder("Assets/Data/ShopItems");

        // (file, name, type, dmg, spd, range, rotOffset, price)
        var weapons = new (string file, string name, WeaponType type, float dmg, float spd, float range, float rot, int price)[]
        {
            ("weapon_rusty_sword",       "녹슨 검",      WeaponType.Melee,   8f,  2.0f, 1.2f, -90f,  50),
            ("weapon_knife",             "단검",         WeaponType.Melee,  10f,  3.0f, 1.0f, -90f,  80),
            ("weapon_regular_sword",     "일반 검",      WeaponType.Melee,  12f,  2.0f, 1.3f, -90f, 100),
            ("weapon_machete",           "마체테",       WeaponType.Melee,  14f,  2.5f, 1.2f, -90f, 120),
            ("weapon_baton_with_spikes", "스파이크 봉",  WeaponType.Melee,  15f,  2.0f, 1.3f, -90f, 130),
            ("weapon_duel_sword",        "듀얼 검",      WeaponType.Melee,  15f,  2.5f, 1.3f, -90f, 140),
            ("weapon_katana",            "카타나",       WeaponType.Melee,  16f,  3.0f, 1.3f, -90f, 150),
            ("weapon_anime_sword",       "애니메 검",    WeaponType.Melee,  18f,  3.0f, 1.4f, -90f, 160),
            ("weapon_axe",               "도끼",         WeaponType.Melee,  20f,  1.8f, 1.4f, -90f, 180),
            ("weapon_spear",             "창",           WeaponType.Melee,  20f,  2.0f, 2.0f, -90f, 200),
            ("weapon_knight_sword",      "기사 검",      WeaponType.Melee,  20f,  2.0f, 1.5f, -90f, 200),
            ("weapon_cleaver",           "중검",         WeaponType.Melee,  22f,  1.5f, 1.4f, -90f, 220),
            ("weapon_saw_sword",         "톱날 검",      WeaponType.Melee,  22f,  1.8f, 1.4f, -90f, 220),
            ("weapon_mace",              "철퇴",         WeaponType.Melee,  24f,  1.5f, 1.4f, -90f, 240),
            ("weapon_hammer",            "망치",         WeaponType.Melee,  26f,  1.2f, 1.3f, -90f, 260),
            ("weapon_waraxe",            "전투 도끼",    WeaponType.Melee,  28f,  1.5f, 1.5f, -90f, 280),
            ("weapon_red_gem_sword",     "홍보석 검",    WeaponType.Melee,  28f,  2.0f, 1.5f, -90f, 300),
            ("weapon_golden_sword",      "황금 검",      WeaponType.Melee,  30f,  2.0f, 1.5f, -90f, 330),
            ("weapon_double_axe",        "쌍 도끼",      WeaponType.Melee,  32f,  1.3f, 1.5f, -90f, 350),
            ("weapon_lavish_sword",      "호화 검",      WeaponType.Melee,  32f,  2.0f, 1.6f, -90f, 350),
            ("weapon_big_hammer",        "대형 망치",    WeaponType.Melee,  45f,  0.8f, 1.5f, -90f, 420),
            ("weapon_bow",               "활",           WeaponType.Ranged, 14f,  2.0f, 0.0f,   0f, 150),
            ("weapon_throwing_axe",      "투척 도끼",    WeaponType.Ranged, 18f,  2.5f, 0.0f,   0f, 200),
            ("weapon_bow_2",             "강화 활",      WeaponType.Ranged, 22f,  2.0f, 0.0f,   0f, 260),
            ("weapon_green_magic_staff", "초록 마법봉",  WeaponType.Ranged, 20f,  2.0f, 0.0f,   0f, 240),
            ("weapon_red_magic_staff",   "붉은 마법봉",  WeaponType.Ranged, 30f,  1.8f, 0.0f,   0f, 320),
        };

        int count = 0;
        foreach (var w in weapons)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Sprites/Items/{w.file}.png");

            var wd = GetOrCreate<WeaponData>($"Assets/Data/Weapons/{w.file}.asset");
            wd.weaponName          = w.name;
            wd.weaponType          = w.type;
            wd.sprite              = sprite;
            wd.damage              = w.dmg;
            wd.attackSpeed         = w.spd;
            wd.attackRange         = w.range;
            wd.spriteRotationOffset = w.rot;
            EditorUtility.SetDirty(wd);

            var sd = GetOrCreate<ShopItemData>($"Assets/Data/ShopItems/{w.file}_shop.asset");
            sd.displayName = w.name;
            sd.weaponData  = wd;
            sd.icon        = sprite;
            sd.price       = w.price;
            EditorUtility.SetDirty(sd);

            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[WeaponAssetGenerator] {count}개 무기 에셋 생성/업데이트 완료");
    }

    static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
    }
}
