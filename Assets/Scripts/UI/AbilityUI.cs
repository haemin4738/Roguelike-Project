using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    public static AbilityUI Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Transform abilityContainer;
    [SerializeField] GameObject abilitySlotPrefab;
    [SerializeField] AbilityData[] abilities;
    [SerializeField] TMP_Text apText;

    const int CostPerLevel = 10;

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
        var meta = MetaProgress.Instance;
        if (apText != null && meta != null)
            apText.text = $"AP: {meta.Ap}";

        foreach (Transform t in abilityContainer) Destroy(t.gameObject);
        if (abilities == null || meta == null) return;

        foreach (var data in abilities)
        {
            int lv = meta.GetAbilityLevel(data.abilityId);
            int cost = (lv + 1) * CostPerLevel;
            bool maxed = lv >= data.maxLevel;

            var slot = Instantiate(abilitySlotPrefab, abilityContainer);
            string nextMilestone = "";
            if (!maxed)
            {
                if (lv < 5 && !string.IsNullOrEmpty(data.milestone5Desc))
                    nextMilestone = $"\nLv.5: {data.milestone5Desc}";
                else if (lv < 10 && !string.IsNullOrEmpty(data.milestone10Desc))
                    nextMilestone = $"\nLv.10: {data.milestone10Desc}";
                else if (lv < 20 && !string.IsNullOrEmpty(data.milestone20Desc))
                    nextMilestone = $"\nLv.20: {data.milestone20Desc}";
            }

            var texts = slot.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = data.displayName;
            if (texts.Length > 1) texts[1].text = maxed ? $"Lv.{lv} MAX" : $"Lv.{lv}  {cost}AP{nextMilestone}";

            var btn = slot.GetComponentInChildren<Button>();
            if (btn == null) continue;
            btn.interactable = !maxed && meta.Ap >= cost;

            var captured = data;
            btn.onClick.AddListener(() =>
            {
                var m = MetaProgress.Instance;
                if (m == null) return;
                int curLv = m.GetAbilityLevel(captured.abilityId);
                int c = (curLv + 1) * CostPerLevel;
                if (m.Ap < c) return;
                if (m.UpgradeAbility(captured.abilityId, captured.maxLevel))
                    m.SpendAp(c);
                Refresh();
            });
        }
    }
}
