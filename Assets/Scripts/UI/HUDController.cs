using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] Image hpBar;
    [SerializeField] TMP_Text goldText;
    [SerializeField] TMP_Text levelText;

    void OnEnable()
    {
        EventBus.Subscribe<PlayerHpChangedEvent>(OnHpChanged);
        EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
        EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<PlayerHpChangedEvent>(OnHpChanged);
        EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
        EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
    }

    void Start()
    {
        UpdateGold(GoldWallet.Gold);
        if (PlayerLevel.Instance != null)
            UpdateLevel(PlayerLevel.Instance.Level);
    }

    void OnHpChanged(PlayerHpChangedEvent e)
    {
        if (hpBar != null)
            hpBar.fillAmount = e.Max > 0f ? e.Current / e.Max : 0f;
    }

    void OnGoldChanged(GoldChangedEvent e) => UpdateGold(e.Total);
    void OnLevelUp(LevelUpEvent e) => UpdateLevel(e.NewLevel);

    void UpdateGold(int amount) { if (goldText != null) goldText.text = $"{amount}G"; }
    void UpdateLevel(int level) { if (levelText != null) levelText.text = $"Lv.{level}"; }
}
