using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected float maxHp = 50f;

    protected float _hp;

    public float HpFraction => Mathf.Clamp01(_hp / maxHp);

    protected virtual void OnEnable() => _hp = maxHp;

    public virtual void TakeDamage(float amount)
    {
        _hp -= amount;
        if (_hp <= 0f) Die();
    }

    protected virtual void Die()
    {
        EventBus.Publish(new EnemyKilledEvent { Enemy = gameObject });
        Destroy(gameObject);
    }
}
