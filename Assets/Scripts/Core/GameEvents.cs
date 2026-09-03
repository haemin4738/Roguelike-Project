using UnityEngine;

public struct DamageEvent
{
    public GameObject Target;
    public float Amount;
}

public struct EnemyKilledEvent
{
    public GameObject Enemy;
}

public struct RoomClearedEvent { }

public struct PlayerDiedEvent { }

public struct PlayerHpChangedEvent { public float Current; public float Max; }

public struct GoldChangedEvent { public int Total; }

public struct ItemPickedEvent { public string ItemName; }
