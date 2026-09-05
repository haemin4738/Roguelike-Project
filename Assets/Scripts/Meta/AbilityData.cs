using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/AbilityData")]
public class AbilityData : ScriptableObject
{
    public string abilityId;
    public string displayName;
    public int maxLevel = 20;
    public bool requiresUnlock;

    [Header("Per-Level Bonuses")]
    public float damagePerLevel;
    public float speedPerLevel;
    public float defensePerLevel;
    public float maxHpPerLevel;
    public float critChancePerLevel;
    public float critDamagePerLevel;
    public float attackSpeedPerLevel;
    public float dodgeChancePerLevel;

    [Header("Milestone Descriptions (UI)")]
    public string milestone5Desc;
    public string milestone10Desc;
    public string milestone20Desc;

    [Header("Milestone Effects (구현)")]
    public bool milestone5DoubleJump;
    public int milestone5DashBonus;
    public int milestone20DashBonus;
    public float milestone5ShopDiscount;
}
