using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public static PlayerLevel Instance { get; private set; }

    static readonly int[] ExpTable = {
        0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700,
        3250, 3850, 4500, 5200, 5950, 6750, 7600, 8500, 9450, 10450,
        11500, 12600, 13750, 14950, 16200, 17500, 18850, 20250, 21700, 23200
    };

    public int Level { get; private set; } = 1;
    public int Exp { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        EventBus.Subscribe<PlayerDiedEvent>(OnRunEnd);
        EventBus.Subscribe<RunEndedEvent>(OnVictory);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        EventBus.Unsubscribe<PlayerDiedEvent>(OnRunEnd);
        EventBus.Unsubscribe<RunEndedEvent>(OnVictory);
    }

    void OnEnemyKilled(EnemyKilledEvent e) => AddExp(e.ExpReward);
    void OnRunEnd(PlayerDiedEvent _) => Settle();
    void OnVictory(RunEndedEvent _) => Settle();

    void AddExp(int amount)
    {
        if (Level >= 30) return;
        Exp += amount;
        while (Level < 30 && Exp >= ExpTable[Level])
        {
            Exp -= ExpTable[Level];
            Level++;
            int apGain = 1;
            MetaProgress.Instance?.AddAp(apGain);
            EventBus.Publish(new LevelUpEvent { NewLevel = Level, ApGained = apGain });
        }
    }

    void Settle() => Exp = 0;
}
