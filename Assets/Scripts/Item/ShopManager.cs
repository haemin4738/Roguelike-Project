using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] ShopItemData[] allItems;
    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] int stockCount = 5;

    ShopItemData[] _stock;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        RefreshStock();
    }

    public void RefreshStock()
    {
        if (allItems == null || allItems.Length == 0) { _stock = new ShopItemData[0]; return; }
        int n = Mathf.Min(stockCount, allItems.Length);
        var pool = new List<ShopItemData>(allItems);
        _stock = new ShopItemData[n];
        for (int i = 0; i < n; i++)
        {
            int idx = Random.Range(0, pool.Count);
            _stock[i] = pool[idx];
            pool.RemoveAt(idx);
        }
    }

    public bool TryBuy(int index)
    {
        if (index < 0 || index >= _stock.Length) return false;
        var item = _stock[index];
        float discount = MetaProgress.Instance?.ShopDiscount ?? 0f;
        int finalPrice = Mathf.RoundToInt(item.price * (1f - discount));
        if (!GoldWallet.Spend(finalPrice)) return false;
        playerCombat?.EquipWeapon(item.weaponData);
        return true;
    }

    public ShopItemData GetItem(int index) =>
        index >= 0 && index < _stock?.Length ? _stock[index] : null;

    public int ItemCount => _stock?.Length ?? 0;
}
