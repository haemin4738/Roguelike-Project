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
