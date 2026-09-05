using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerSync : MonoBehaviour
{
    const string BaseUrl = "https://roguelike-project-server-production.up.railway.app";

    void Start()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
            StartCoroutine(LoadAll());
    }

    void OnEnable()
    {
        EventBus.Subscribe<PlayerDiedEvent>(OnRunEnd);
        EventBus.Subscribe<RunEndedEvent>(OnVictory);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(OnRunEnd);
        EventBus.Unsubscribe<RunEndedEvent>(OnVictory);
    }

    IEnumerator LoadAll()
    {
        string dataJson = null, abilitiesJson = null;
        yield return Get($"{BaseUrl}/player/data", r => dataJson = r);
        yield return Get($"{BaseUrl}/player/abilities", r => abilitiesJson = r);
        if (dataJson == null || abilitiesJson == null) yield break;

        var data = JsonUtility.FromJson<PlayerDataResponse>(dataJson);
        var wrapper = JsonUtility.FromJson<AbilityListWrapper>("{\"items\":" + abilitiesJson + "}");
        if (data != null && wrapper != null)
            MetaProgress.Instance?.LoadFromServer(data, wrapper.items);
    }

    IEnumerator Get(string url, System.Action<string> onSuccess)
    {
        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {AuthManager.Instance.Token}");
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            onSuccess(req.downloadHandler.text);
        else
            Debug.LogWarning($"[ServerSync] GET {url} failed: {req.downloadHandler.text}");
    }

    [System.Serializable]
    class AbilityListWrapper { public List<AbilityLevelEntry> items; }

    void OnRunEnd(PlayerDiedEvent _) => SyncAll(false);
    void OnVictory(RunEndedEvent e) { if (e.Victory) SyncAll(true); }

    void SyncAll(bool victory)
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn) return;
        MetaProgress.Instance?.OnRunEnd(victory);
        StartCoroutine(PutPlayerData());
        StartCoroutine(PutAbilities());
    }

    IEnumerator PutPlayerData()
    {
        var meta = MetaProgress.Instance;
        if (meta == null) yield break;

        string json = JsonUtility.ToJson(new PlayerDataRequest
        {
            ap = meta.Ap, gold = meta.Gold,
            total_runs = meta.TotalRuns, best_floor = meta.BestFloor,
            total_kills = meta.TotalKills, total_deaths = meta.TotalDeaths
        });
        yield return Put($"{BaseUrl}/player/data", json);
    }

    IEnumerator PutAbilities()
    {
        var meta = MetaProgress.Instance;
        if (meta == null) yield break;

        var sb = new StringBuilder("[");
        bool first = true;
        foreach (var kv in meta.Abilities)
        {
            if (!first) sb.Append(",");
            sb.Append(JsonUtility.ToJson(new AbilityLevelEntry { ability_id = kv.Key, level = kv.Value }));
            first = false;
        }
        sb.Append("]");
        yield return Put($"{BaseUrl}/player/abilities", sb.ToString());
    }

    IEnumerator Put(string url, string json)
    {
        var req = new UnityWebRequest(url, "PUT");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {AuthManager.Instance.Token}");
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"[ServerSync] PUT {url} failed: {req.downloadHandler.text}");
    }

    [System.Serializable]
    class PlayerDataRequest
    {
        public int ap, gold, total_runs, best_floor, total_kills, total_deaths;
    }
}
