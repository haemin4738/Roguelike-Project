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

            var img = slot.GetComponentInChildren<Image>();
            if (img != null && c.previewSprite != null) img.sprite = c.previewSprite;

            slot.GetComponentInChildren<Button>()?.onClick.AddListener(() =>
            {
                CharacterManager.Select(captured);
                Close();
            });
        }
    }
}
