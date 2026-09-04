using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] ShopItemData[] items;
    [SerializeField] PlayerCombat playerCombat;

    // UI에서 인덱스로 호출 (feat/ui에서 연결 예정)
    public bool TryBuy(int index)
    {
        if (index < 0 || index >= items.Length) return false;
        var item = items[index];
        if (!GoldWallet.Spend(item.price)) return false;
        playerCombat?.EquipWeapon(item.weaponData);
        return true;
    }

    public ShopItemData GetItem(int index) =>
        index >= 0 && index < items.Length ? items[index] : null;

    public int ItemCount => items?.Length ?? 0;
}
