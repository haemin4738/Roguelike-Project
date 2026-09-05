using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public static CharacterSelectUI Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Transform slotContainer;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] CharacterData[] characters;

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

    static string BuildStatDesc(CharacterData c)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (c.bonusMaxHp      != 0) parts.Add($"HP {(c.bonusMaxHp > 0 ? "+" : "")}{c.bonusMaxHp}");
        if (c.bonusDamage     != 0) parts.Add($"공격력 {(c.bonusDamage > 0 ? "+" : "")}{c.bonusDamage}");
        if (c.bonusMoveSpeed  != 0) parts.Add($"이동속도 {(c.bonusMoveSpeed > 0 ? "+" : "")}{c.bonusMoveSpeed}");
        if (c.bonusAttackSpeed!= 0) parts.Add($"공격속도 {(c.bonusAttackSpeed > 0 ? "+" : "")}{c.bonusAttackSpeed}");
        if (c.bonusDashCount  != 0) parts.Add($"대시 +{c.bonusDashCount}");
        return string.Join("\n", parts);
    }

    void Refresh()
    {
        foreach (Transform t in slotContainer) Destroy(t.gameObject);
        if (characters == null) return;

        foreach (var c in characters)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            var captured = c;

            var texts = slot.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = c.displayName;
            if (texts.Length > 1) texts[1].text = c.description;
            if (texts.Length > 2) texts[2].text = BuildStatDesc(c);

            var img = slot.GetComponentInChildren<Image>();
            if (img != null && c.previewSprite != null) img.sprite = c.previewSprite;

            bool isSelected = CharacterManager.Selected == captured;

            var btn = slot.GetComponentInChildren<Button>();
            if (btn == null)
            {
                var btnGO = new GameObject("SelectBtn", typeof(RectTransform));
                btnGO.transform.SetParent(slot.transform, false);
                var rt = btnGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.02f);
                rt.anchorMax = new Vector2(0.95f, 0.22f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var btnImg = btnGO.AddComponent<Image>();
                btnImg.color = new Color(0.25f, 0.5f, 1f);
                btn = btnGO.AddComponent<Button>();
                var labelGO = new GameObject("Label", typeof(RectTransform));
                labelGO.transform.SetParent(btnGO.transform, false);
                var label = labelGO.AddComponent<TextMeshProUGUI>();
                label.text = "선택";
                label.alignment = TextAlignmentOptions.Center;
                label.fontSize = 12;
                var lrt = labelGO.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            }
            // 선택된 캐릭터는 버튼 비활성 + "선택됨" 표시
            var btnLabel = btn.GetComponentInChildren<TMP_Text>();
            if (btnLabel != null) btnLabel.text = isSelected ? "선택됨" : "선택";
            btn.interactable = !isSelected;

            btn.onClick.AddListener(() =>
            {
                CharacterManager.Select(captured);
                FindObjectOfType<MetaProgressApplicator>()?.Reapply();
                Close();
            });
        }
    }
}
