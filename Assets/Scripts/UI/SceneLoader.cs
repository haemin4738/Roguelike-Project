using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    [SerializeField] Image fadePanel;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName) => StartCoroutine(FadeAndLoad(sceneName));

    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneName);
        yield return Fade(1f, 0f);
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadePanel == null) yield break;
        fadePanel.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            fadePanel.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t));
            yield return null;
        }
        if (to == 0f) fadePanel.gameObject.SetActive(false);
    }
}
