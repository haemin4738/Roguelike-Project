using UnityEngine;
using UnityEditor;

public class CharacterAssetGenerator
{
    [MenuItem("Tools/Generate Character Assets")]
    static void Generate()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/Characters");

        // (prefix, id, name, desc, bonusHp, bonusSpeed, bonusDmg, bonusAtkSpd, bonusDash)
        var chars = new (string prefix, string id, string name, string desc,
                         float hp, float spd, float dmg, float atkSpd, int dash)[]
        {
            ("knight_m", "knight", "기사",
             "든든한 체력과 강력한 공격력. 느리지만 한 방이 강하다.",
             30f, 0f, 3f, 0f, 0),

            ("wizzard_m", "wizard", "마법사",
             "원거리 공격 특화. 체력은 낮지만 마법 위력이 강하다.",
             -20f, 0f, 8f, 0.3f, 0),

            ("elf_m", "elf", "엘프",
             "빠른 이동속도와 추가 대시. 전장을 자유롭게 누빈다.",
             0f, 1.5f, 0f, 0f, 1),

            ("lizard_m", "lizard", "도마뱀",
             "균형 잡힌 스탯. 적응력이 뛰어난 만능 캐릭터.",
             20f, 0.5f, 0f, 0f, 0),

            ("dwarf_m", "dwarf", "드워프",
             "압도적인 체력과 공격력. 느리지만 쉽게 쓰러지지 않는다.",
             50f, -0.5f, 5f, 0f, 0),
        };

        int count = 0;
        foreach (var c in chars)
        {
            string path = $"Assets/Data/Characters/{c.id}.asset";
            var cd = GetOrCreate<CharacterData>(path);
            cd.characterId      = c.id;
            cd.displayName      = c.name;
            cd.description      = c.desc;
            cd.previewSprite    = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/Art/Sprites/Player/{c.prefix}_idle_anim_f0.png");
            cd.bonusMaxHp       = c.hp;
            cd.bonusMoveSpeed   = c.spd;
            cd.bonusDamage      = c.dmg;
            cd.bonusAttackSpeed = c.atkSpd;
            cd.bonusDashCount   = c.dash;
            EditorUtility.SetDirty(cd);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CharacterAssetGenerator] {count}개 캐릭터 에셋 생성/업데이트 → Assets/Data/Characters/");
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
