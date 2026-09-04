using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/ShopItemData")]
public class ShopItemData : ScriptableObject
{
    public string displayName;
    public WeaponData weaponData;
    public int price;
    public Sprite icon;
}
