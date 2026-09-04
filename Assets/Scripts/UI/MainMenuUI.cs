using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject loginPanel;

    [Header("Login Fields")]
    [SerializeField] TMP_InputField usernameField;
    [SerializeField] TMP_InputField passwordField;
    [SerializeField] TMP_Text statusText;

    void Start()
    {
        if (passwordField != null)
            passwordField.contentType = TMP_InputField.ContentType.Password;
        ShowMain();
    }

    public void OnStartGame()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
            SceneLoader.Instance?.LoadScene("Game");
        else
            ShowLogin();
    }

    public void OnLogin()
    {
        if (AuthManager.Instance == null) return;
        SetStatus("로그인 중...");
        AuthManager.Instance.Login(usernameField.text, passwordField.text, (ok, _) =>
        {
            if (ok) { SetStatus(""); ShowMain(); }
            else SetStatus("로그인 실패");
        });
    }

    public void OnRegister()
    {
        if (AuthManager.Instance == null) return;
        SetStatus("가입 중...");
        AuthManager.Instance.Register(usernameField.text, passwordField.text, (ok, _) =>
        {
            if (ok) { SetStatus(""); ShowMain(); }
            else SetStatus("가입 실패 (이미 사용 중인 아이디)");
        });
    }

    public void OnBack() => ShowMain();

    void ShowMain() { mainPanel?.SetActive(true); loginPanel?.SetActive(false); }
    void ShowLogin() { mainPanel?.SetActive(false); loginPanel?.SetActive(true); }
    void SetStatus(string msg) { if (statusText != null) statusText.text = msg; }
}
