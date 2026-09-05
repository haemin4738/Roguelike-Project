using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text killsText;
    [SerializeField] TMP_Text levelText;

    [SerializeField] TMP_Text titleText;

    void OnEnable()
    {
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Subscribe<RunEndedEvent>(OnRunEnded);
    }
    void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Unsubscribe<RunEndedEvent>(OnRunEnded);
    }

    void Start() => panel?.SetActive(false);

    void OnPlayerDied(PlayerDiedEvent _) => Show("게임 오버", MetaProgress.Instance?.RunKills ?? 0);

    void OnRunEnded(RunEndedEvent e) { if (e.Victory) Show("클리어!", e.Kills); }

    void Show(string title, int kills)
    {
        if (titleText != null) titleText.text = title;
        panel?.SetActive(true);
        Time.timeScale = 0f;
        if (killsText != null) killsText.text = $"처치: {kills}";
        if (levelText != null && PlayerLevel.Instance != null)
            levelText.text = $"Lv.{PlayerLevel.Instance.Level}";
    }

    public void OnRetry() { Time.timeScale = 1f; SceneLoader.Instance?.LoadScene("Game"); }
    public void OnMainMenu() { Time.timeScale = 1f; SceneLoader.Instance?.LoadScene("MainMenu"); }
}
