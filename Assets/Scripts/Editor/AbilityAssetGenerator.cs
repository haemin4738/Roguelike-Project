using UnityEngine;
using UnityEditor;

public class AbilityAssetGenerator
{
    [MenuItem("Tools/Generate Ability Assets")]
    static void Generate()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/Abilities");

        // 던그리드 어빌리티 6종 (기본 5 + 집중)
        // perLevel: (dmg, spd, def, hp, crit, critDmg, atkSpd, dodge)
        var abilities = new[]
        {
            new AbilityEntry
            {
                id = "fury", name = "분노", maxLv = 20,
                dmg = 2f,
                m5desc  = "점프 시 주변 피해 8",
                m10desc = "적 처치 시 15초간 위력 +10",
                m20desc = "체력 60% 미만 최대데미지·방어관통, 대시 +1",
                m20dash = 1,
            },
            new AbilityEntry
            {
                id = "swift", name = "신속", maxLv = 20,
                spd = 0.05f, atkSpd = 0.025f,
                m5desc  = "이단 점프, 대시 +1",
                m10desc = "체력 80% 이상 공격속도 +10%, 대시 충전 35% 가속",
                m20desc = "대시 시 0.2초 무적, 대시 +1",
                m5jump = true, m5dash = 1, m20dash = 1,
            },
            new AbilityEntry
            {
                id = "endure", name = "인내", maxLv = 20,
                def = 1.5f, hp = 1f,
                m5desc  = "마법 방패 획득",
                m10desc = "죽음의 피해 시 4초 무적 (던전당 1회)",
                m20desc = "저체력 시 방어력 증가 및 지속 회복, 대시 +1",
                m20dash = 1,
            },
            new AbilityEntry
            {
                id = "mystic", name = "신비", maxLv = 20,
                crit = 0.5f, dodge = 0.5f,
                m5desc  = "상점 가격 40% 할인",
                m10desc = "아이템 스킬 쿨타임 20% 감소",
                m20desc = "사망 시 아이템 1개 보관, 대시 +1",
                m5shopDiscount = 0.4f, m20dash = 1,
            },
            new AbilityEntry
            {
                id = "greed", name = "탐욕", maxLv = 20,
                hp = 2f,
                m5desc  = "골드 드랍율 20% 증가",
                m10desc = "최대 포만감 +25",
                m20desc = "액세서리 슬롯 +1, 대시 +1",
                m20dash = 1,
            },
            new AbilityEntry
            {
                id = "focus", name = "집중", maxLv = 20,
                critDmg = 2.5f,
                m5desc  = "원거리 무기 장착 시 체력 +20, 방어력 +10",
                m10desc = "재장전 속도 +15%, 방 클리어 시 체력 +2 회복",
                m20desc = "12초마다 재장전 도구 획득, 대시 +1",
                m20dash = 1,
            },
        };

        int count = 0;
        foreach (var a in abilities)
        {
            string path = $"Assets/Data/Abilities/{a.id}.asset";
            var ad = GetOrCreate<AbilityData>(path);
            ad.abilityId              = a.id;
            ad.displayName            = a.name;
            ad.maxLevel               = a.maxLv;
            ad.requiresUnlock         = false;
            ad.damagePerLevel         = a.dmg;
            ad.speedPerLevel          = a.spd;
            ad.defensePerLevel        = a.def;
            ad.maxHpPerLevel          = a.hp;
            ad.critChancePerLevel     = a.crit;
            ad.critDamagePerLevel     = a.critDmg;
            ad.attackSpeedPerLevel    = a.atkSpd;
            ad.dodgeChancePerLevel    = a.dodge;
            ad.milestone5Desc         = a.m5desc;
            ad.milestone10Desc        = a.m10desc;
            ad.milestone20Desc        = a.m20desc;
            ad.milestone5DoubleJump   = a.m5jump;
            ad.milestone5DashBonus    = a.m5dash;
            ad.milestone20DashBonus   = a.m20dash;
            ad.milestone5ShopDiscount = a.m5shopDiscount;
            EditorUtility.SetDirty(ad);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AbilityAssetGenerator] {count}개 어빌리티 에셋 생성/업데이트 → Assets/Data/Abilities/");
    }

    class AbilityEntry
    {
        public string id, name;
        public int maxLv = 20;
        public float dmg, spd, def, hp, crit, critDmg, atkSpd, dodge;
        public string m5desc = "", m10desc = "", m20desc = "";
        public bool m5jump;
        public int m5dash, m20dash;
        public float m5shopDiscount;
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
