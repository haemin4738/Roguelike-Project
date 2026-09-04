using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    const string BaseUrl = "https://roguelike-project-server-production.up.railway.app";
    const string TokenKey = "jwt_token";

    public string Token => PlayerPrefs.GetString(TokenKey, null);
    public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(string username, string password, Action<bool, string> callback)
    {
        StartCoroutine(PostJson($"{BaseUrl}/auth/register",
            $"{{\"username\":\"{username}\",\"password\":\"{password}\"}}",
            (ok, body) => { if (ok) SaveToken(body); callback(ok, body); }));
    }

    public void Login(string username, string password, Action<bool, string> callback)
    {
        StartCoroutine(PostForm($"{BaseUrl}/auth/login", username, password,
            (ok, body) => { if (ok) SaveToken(body); callback(ok, body); }));
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.Save();
    }

    void SaveToken(string responseBody)
    {
        var wrapper = JsonUtility.FromJson<TokenResponse>(responseBody);
        if (wrapper != null && !string.IsNullOrEmpty(wrapper.access_token))
        {
            PlayerPrefs.SetString(TokenKey, wrapper.access_token);
            PlayerPrefs.Save();
        }
    }

    IEnumerator PostJson(string url, string json, Action<bool, string> callback)
    {
        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        callback(req.result == UnityWebRequest.Result.Success, req.downloadHandler.text);
    }

    IEnumerator PostForm(string url, string username, string password, Action<bool, string> callback)
    {
        var form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        using var req = UnityWebRequest.Post(url, form);
        yield return req.SendWebRequest();
        callback(req.result == UnityWebRequest.Result.Success, req.downloadHandler.text);
    }

    [Serializable] class TokenResponse { public string access_token; }
}
