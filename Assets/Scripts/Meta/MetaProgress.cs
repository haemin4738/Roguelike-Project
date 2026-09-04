using System.Collections.Generic;
using UnityEngine;

public class MetaProgress : MonoBehaviour
{
    public static MetaProgress Instance { get; private set; }

    public int Ap { get; private set; }
    public int Gold { get; private set; }
    public int TotalRuns { get; private set; }
    public int BestFloor { get; private set; }
    public int TotalKills { get; private set; }
    public int TotalDeaths { get; private set; }

    public Dictionary<string, int> Abilities { get; private set; } = new();

    public int RunKills { get; private set; }
    public int RunFloor { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
    void OnDisable() => EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
    void OnEnemyKilled(EnemyKilledEvent _) => RunKills++;

    public void LoadFromServer(PlayerDataResponse data, List<AbilityLevelEntry> abilities)
    {
        Ap = data.ap; Gold = data.gold;
        TotalRuns = data.total_runs; BestFloor = data.best_floor;
        TotalKills = data.total_kills; TotalDeaths = data.total_deaths;
        Abilities.Clear();
        foreach (var e in abilities) Abilities[e.ability_id] = e.level;
    }

    public void SpendAp(int amount) => Ap = Mathf.Max(0, Ap - amount);
    public void AddAp(int amount) => Ap += amount;

    public int GetAbilityLevel(string id) => Abilities.TryGetValue(id, out int lv) ? lv : 0;

    public bool UpgradeAbility(string id, int maxLevel)
    {
        int current = GetAbilityLevel(id);
        if (current >= maxLevel) return false;
        Abilities[id] = current + 1;
        return true;
    }

    public void RecordKill() => RunKills++;
    public void SetFloor(int floor) => RunFloor = floor;

    public void OnRunEnd(bool victory)
    {
        TotalRuns++;
        TotalKills += RunKills;
        if (victory) BestFloor = Mathf.Max(BestFloor, RunFloor);
        else TotalDeaths++;
        RunKills = 0;
        RunFloor = 0;
    }
}

[System.Serializable]
public class PlayerDataResponse
{
    public int ap, gold, total_runs, best_floor, total_kills, total_deaths;
}

[System.Serializable]
public class AbilityLevelEntry
{
    public string ability_id;
    public int level;
}
