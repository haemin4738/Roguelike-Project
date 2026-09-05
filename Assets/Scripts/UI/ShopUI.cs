using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Transform itemContainer;
    [SerializeField] GameObject itemSlotPrefab;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        if (panel.activeSelf && Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            Close();
    }

    public void Open()
    {
        Refresh();
        panel.SetActive(true);
    }

    public void Close() => panel.SetActive(false);

    void Refresh()
    {
        foreach (Transform t in itemContainer) Destroy(t.gameObject);
        if (ShopManager.Instance == null) return;

        float discount = MetaProgress.Instance?.ShopDiscount ?? 0f;

        for (int i = 0; i < ShopManager.Instance.ItemCount; i++)
        {
            var item = ShopManager.Instance.GetItem(i);
            if (item == null) continue;

            var slot = Instantiate(itemSlotPrefab, itemContainer);
            int idx = i;
            int finalPrice = Mathf.RoundToInt(item.price * (1f - discount));

            Image img = null;
            foreach (var candidate in slot.GetComponentsInChildren<Image>())
            {
                if (candidate.gameObject != slot) { img = candidate; break; }
            }
            if (img != null && item.icon != null)
            {
                img.sprite = item.icon;
                img.transform.localScale = Vector3.one * 0.5f;
            }

            var texts = slot.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = item.displayName;
            if (texts.Length > 1) texts[1].text = discount > 0 ? $"<s>{item.price}G</s>  {finalPrice}G" : $"{item.price}G";

            slot.GetComponentInChildren<Button>()?.onClick.AddListener(() =>
            {
                ShopManager.Instance.TryBuy(idx);
                Refresh();
            });
        }
    }
}
