using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Button slotButton1;      // 자식에 Image + TMP_Text 포함
    [SerializeField] Button slotButton2;      // 자식에 Image + TMP_Text 포함
    [SerializeField] Transform inventoryContainer;
    [SerializeField] GameObject inventorySlotPrefab;
    [SerializeField] PlayerCombat playerCombat;

    int _selectedSlot = 0;

    static readonly Color SelectedColor = new(0.3f, 0.7f, 0.3f);
    static readonly Color DefaultColor  = Color.white;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
        if (slotButton1 != null) slotButton1.onClick.AddListener(() => SelectSlot(0));
        if (slotButton2 != null) slotButton2.onClick.AddListener(() => SelectSlot(1));
    }

    void Update()
    {
        if (panel.activeSelf && Keyboard.current?.escapeKey.wasPressedThisFrame == true) Close();
    }

    void SelectSlot(int slot) { _selectedSlot = slot; Refresh(); }
    public void Toggle() { if (panel.activeSelf) Close(); else Open(); }
    public void Open()  { Refresh(); panel.SetActive(true); }
    public void Close() => panel.SetActive(false);

    void Refresh()
    {
        var w1 = playerCombat?.GetWeapon(0);
        var w2 = playerCombat?.GetWeapon(1);
        if (slotButton1 != null) RefreshSlotButton(slotButton1, w1, _selectedSlot == 0);
        if (slotButton2 != null) RefreshSlotButton(slotButton2, w2, _selectedSlot == 1);

        foreach (Transform t in inventoryContainer) Destroy(t.gameObject);
        var inv = PlayerInventory.Instance;
        if (inv == null || playerCombat == null) return;

        foreach (var weapon in inv.Weapons)
        {
            var slot = Instantiate(inventorySlotPrefab, inventoryContainer);
            var captured = weapon;

            Image img = null;
            foreach (var c in slot.GetComponentsInChildren<Image>())
            {
                if (c.gameObject != slot) { img = c; break; }
            }
            if (img != null && weapon.sprite != null)
            {
                img.sprite = weapon.sprite;
                img.transform.localScale = Vector3.one * 0.5f;
            }

            var texts = slot.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = weapon.weaponName;

            var btn = slot.GetComponentInChildren<Button>();
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    var current = playerCombat.GetWeapon(_selectedSlot);
                    playerCombat.EquipWeapon(captured, _selectedSlot);
                    if (current != null)
                        PlayerInventory.Instance.Swap(captured, current);
                    else
                        PlayerInventory.Instance.Remove(captured);
                    Refresh();
                });
        }
    }

    static void RefreshSlotButton(Button btn, WeaponData w, bool selected)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = selected ? SelectedColor : DefaultColor;

        var weaponImg = btn.GetComponentInChildren<Image>();
        if (weaponImg != null) weaponImg.sprite = w?.sprite;

        var texts = btn.GetComponentsInChildren<TMP_Text>();
        if (texts.Length > 0) texts[0].text = w?.weaponName ?? "비어있음";
        if (texts.Length > 1) texts[1].text = w != null ? BuildWeaponStatText(w) : "";
    }

    static string BuildWeaponStatText(WeaponData w)
    {
        string type = w.weaponType == WeaponType.Melee ? "근접" : "원거리";
        return $"{type}  공격력 {w.damage}  공격속도 {w.attackSpeed}/s  사거리 {w.attackRange}";
    }
}
