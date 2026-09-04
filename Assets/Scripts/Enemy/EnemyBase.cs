using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected float maxHp = 50f;

    [Header("Drops")]
    [SerializeField] int goldDrop = 10;
    [SerializeField] int coinCount = 1;
    [SerializeField] Sprite[] coinFrames;
    [SerializeField] int expReward = 20;

    protected float _hp;
    bool _dead;

    public float HpFraction => Mathf.Clamp01(_hp / maxHp);

    protected virtual void OnEnable() { _hp = maxHp; _dead = false; }

    public virtual void TakeDamage(float amount)
    {
        if (_dead) return;
        _hp -= amount;
        if (_hp <= 0f) { _dead = true; Die(); }
    }

    protected virtual void Die()
    {
        EventBus.Publish(new EnemyKilledEvent { Enemy = gameObject, ExpReward = expReward });
        SpawnDrops();
        Destroy(gameObject);
    }

    void SpawnDrops()
    {
        Vector3 pos = transform.position + Vector3.up * 0.1f;

        if (goldDrop > 0)
        {
            int count = Mathf.Max(1, coinCount);
            int perCoin = goldDrop / count;
            int remainder = goldDrop % count;
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("GoldPickup");
                go.transform.position = pos + new Vector3(Random.Range(-0.8f, 0.8f), 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 2;
                if (coinFrames != null && coinFrames.Length > 0) sr.sprite = coinFrames[0];
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
                var pickup = go.AddComponent<GoldPickup>();
                pickup.amount = perCoin + (i == 0 ? remainder : 0);
                pickup.coinFrames = coinFrames;
            }
        }
    }
}
