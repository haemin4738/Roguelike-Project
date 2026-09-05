using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] HeartDisplay heartDisplay;
    [SerializeField] TMP_Text goldText;
    [SerializeField] TMP_Text levelText;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.eKey.wasPressedThisFrame) CharacterInfoUI.Instance?.Toggle();
        if (kb.iKey.wasPressedThisFrame) InventoryUI.Instance?.Toggle();
    }

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
        var stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
            heartDisplay?.SetHearts(stats.CurrentHp, stats.MaxHp);
        UpdateGold(GoldWallet.Gold);
        if (PlayerLevel.Instance != null)
            UpdateLevel(PlayerLevel.Instance.Level);
    }

    void OnHpChanged(PlayerHpChangedEvent e) => heartDisplay?.SetHearts(e.Current, e.Max);

    void OnGoldChanged(GoldChangedEvent e) => UpdateGold(e.Total);
    void OnLevelUp(LevelUpEvent e) => UpdateLevel(e.NewLevel);

    void UpdateGold(int amount) { if (goldText != null) goldText.text = $"{amount}G"; }
    void UpdateLevel(int level) { if (levelText != null) levelText.text = $"Lv.{level}"; }
}
