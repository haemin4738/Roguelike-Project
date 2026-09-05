using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterInfoUI : MonoBehaviour
{
    public static CharacterInfoUI Instance { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Image characterImage;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text statText;
    [SerializeField] CharacterData defaultCharacter;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }

    void Update()
    {
        if (panel.activeSelf && Keyboard.current?.escapeKey.wasPressedThisFrame == true) Close();
    }

    public void Toggle() { if (panel.activeSelf) Close(); else Open(); }

    public void Open() { Refresh(); panel.SetActive(true); }
    public void Close() => panel.SetActive(false);

    void Refresh()
    {
        var c = CharacterManager.Selected ?? defaultCharacter;
        if (c == null) return;
        if (characterImage != null) characterImage.sprite = c.previewSprite;
        if (nameText != null) nameText.text = c.displayName;
        if (statText != null) statText.text = BuildStatText(c);
    }

    static string BuildStatText(CharacterData c)
    {
        var parts = new List<string>();
        if (c.bonusMaxHp != 0)       parts.Add($"HP {(c.bonusMaxHp > 0 ? "+" : "")}{c.bonusMaxHp}");
        if (c.bonusDamage != 0)      parts.Add($"공격력 {(c.bonusDamage > 0 ? "+" : "")}{c.bonusDamage}");
        if (c.bonusMoveSpeed != 0)   parts.Add($"이동속도 {(c.bonusMoveSpeed > 0 ? "+" : "")}{c.bonusMoveSpeed}");
        if (c.bonusAttackSpeed != 0) parts.Add($"공격속도 {(c.bonusAttackSpeed > 0 ? "+" : "")}{c.bonusAttackSpeed}");
        if (c.bonusDashCount != 0)   parts.Add($"대시 +{c.bonusDashCount}");
        return parts.Count > 0 ? string.Join("\n", parts) : "기본 캐릭터";
    }
}
