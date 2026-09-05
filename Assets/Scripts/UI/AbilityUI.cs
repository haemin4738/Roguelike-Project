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
    [SerializeField] Button resetButton;

    const int CostPerLevel = 1;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
        if (resetButton != null)
            resetButton.onClick.AddListener(() =>
            {
                MetaProgress.Instance?.ResetAbilities();
                FindObjectOfType<MetaProgressApplicator>()?.Reapply();
                Refresh();
            });
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

    static string BuildStatDesc(AbilityData d)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (d.damagePerLevel      != 0) parts.Add($"공격력 +{d.damagePerLevel}/Lv");
        if (d.speedPerLevel       != 0) parts.Add($"이동속도 +{d.speedPerLevel}/Lv");
        if (d.defensePerLevel     != 0) parts.Add($"방어력 +{d.defensePerLevel}/Lv");
        if (d.maxHpPerLevel       != 0) parts.Add($"체력 +{d.maxHpPerLevel}/Lv");
        if (d.critChancePerLevel  != 0) parts.Add($"치명타 +{d.critChancePerLevel}%/Lv");
        if (d.critDamagePerLevel  != 0) parts.Add($"치명타피해 +{d.critDamagePerLevel}%/Lv");
        if (d.attackSpeedPerLevel != 0) parts.Add($"공격속도 +{d.attackSpeedPerLevel}/Lv");
        if (d.dodgeChancePerLevel != 0) parts.Add($"회피 +{d.dodgeChancePerLevel}%/Lv");
        return string.Join(", ", parts);
    }

    void Refresh()
    {
        var meta = MetaProgress.Instance;
        if (apText != null && meta != null)
            apText.text = $"AP: {meta.Ap}";

        foreach (Transform t in abilityContainer) Destroy(t.gameObject);
        if (abilities == null || meta == null) return;

        foreach (var data in abilities)
        {
            if (data == null) continue;
            int lv = meta.GetAbilityLevel(data.abilityId);
            bool maxed = lv >= data.maxLevel;

            var slot = Instantiate(abilitySlotPrefab, abilityContainer);
            var sb = new System.Text.StringBuilder();
            if (!maxed)
            {
                if (lv < 5  && !string.IsNullOrEmpty(data.milestone5Desc))
                    sb.Append($"Lv.5: {data.milestone5Desc}\n");
                if (lv < 10 && !string.IsNullOrEmpty(data.milestone10Desc))
                    sb.Append($"Lv.10: {data.milestone10Desc}\n");
                if (lv < 20 && !string.IsNullOrEmpty(data.milestone20Desc))
                    sb.Append($"Lv.20: {data.milestone20Desc}");
            }
            string nextMilestone = sb.ToString();

            string statDesc = BuildStatDesc(data);
            string levelText = maxed ? $"Lv.{lv} MAX" : $"Lv.{lv}  1AP";
            string milestoneText = nextMilestone.TrimEnd('\n');

            var btn = slot.GetComponentInChildren<Button>();
            if (btn == null) continue;

            var allTexts = slot.GetComponentsInChildren<TMP_Text>();
            var texts = System.Array.FindAll(allTexts, t => !t.transform.IsChildOf(btn.transform));

            if (texts.Length > 0) texts[0].text = $"{data.displayName}  {levelText}";
            if (texts.Length > 1) texts[1].text = statDesc;
            if (texts.Length > 2) texts[2].text = milestoneText;

            btn.interactable = !maxed && meta.Ap >= CostPerLevel;

            var captured = data;
            btn.onClick.AddListener(() =>
            {
                var m = MetaProgress.Instance;
                if (m == null) return;
                if (m.Ap < CostPerLevel) return;
                if (m.UpgradeAbility(captured.abilityId, captured.maxLevel))
                {
                    m.SpendAp(CostPerLevel);
                    FindObjectOfType<MetaProgressApplicator>()?.Reapply();
                }
                Refresh();
            });
        }
    }
}
