using UnityEngine;

public static class DamageSystem
{
    public static void Damage(GameObject target, float amount)
    {
        if (target == null) return;
        if (target.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(amount);
        EventBus.Publish(new DamageEvent { Target = target, Amount = amount });
    }
}
