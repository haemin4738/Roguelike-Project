using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text killsText;
    [SerializeField] TMP_Text levelText;

    void OnEnable() => EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    void OnDisable() => EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);

    void Start() => panel?.SetActive(false);

    void OnPlayerDied(PlayerDiedEvent _)
    {
        panel?.SetActive(true);
        Time.timeScale = 0f;
        var meta = MetaProgress.Instance;
        if (killsText != null && meta != null) killsText.text = $"처치: {meta.RunKills}";
        if (levelText != null && PlayerLevel.Instance != null)
            levelText.text = $"Lv.{PlayerLevel.Instance.Level}";
    }

    public void OnRetry() { Time.timeScale = 1f; SceneLoader.Instance?.LoadScene("Game"); }
    public void OnMainMenu() { Time.timeScale = 1f; SceneLoader.Instance?.LoadScene("MainMenu"); }
}
