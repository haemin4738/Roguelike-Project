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
}
