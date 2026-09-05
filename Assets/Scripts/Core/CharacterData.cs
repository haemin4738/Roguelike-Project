using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public string displayName;
    [TextArea] public string description;
    public Sprite previewSprite;

    [Header("Animation")]
    public CharacterAnimData animData;

    [Header("Base Stat Bonuses")]
    public float bonusMaxHp;
    public float bonusMoveSpeed;
    public float bonusDamage;
    public float bonusAttackSpeed;
    public int bonusDashCount;
}
