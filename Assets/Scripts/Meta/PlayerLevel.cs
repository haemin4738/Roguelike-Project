using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public static PlayerLevel Instance { get; private set; }

    static readonly int[] ExpTable = {
        0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700,
        3250, 3850, 4500, 5200, 5950, 6750, 7600, 8500, 9450, 10450,
        11500, 12600, 13750, 14950, 16200, 17500, 18850, 20250, 21700, 23200
    };

    [SerializeField] int debugStartLevel = 1;

    public int Level { get; private set; } = 1;
    public int Exp { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            if (debugStartLevel > 1)
            {
                int target = Mathf.Clamp(debugStartLevel, 1, 30);
                if (Instance.Level < target)
                {
                    MetaProgress.Instance?.AddAp(target - Instance.Level);
                    Instance.Level = target;
                    Instance.Exp = 0;
                }
            }
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (debugStartLevel <= 1) return;
        int target = Mathf.Clamp(debugStartLevel, 1, 30);
        int apToGive = target - Level;
        Level = target;
        MetaProgress.Instance?.AddAp(apToGive);
        EventBus.Publish(new LevelUpEvent { NewLevel = Level, ApGained = apToGive });
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
