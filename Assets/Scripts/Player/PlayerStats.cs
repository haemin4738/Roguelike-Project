using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHp = 100f;
    [SerializeField] float invincibleDuration = 0.5f;
    [SerializeField] SpriteRenderer bodyRenderer;

    public float MaxHp => maxHp;
    public float CurrentHp { get; private set; }

    float _invincibleTimer;

    void Awake() => CurrentHp = maxHp;

    void Update()
    {
        if (_invincibleTimer > 0f)
            _invincibleTimer -= Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        if (_invincibleTimer > 0f) return;

        CurrentHp -= amount;
        _invincibleTimer = invincibleDuration;

        EventBus.Publish(new PlayerHpChangedEvent { Current = CurrentHp, Max = maxHp });

        if (bodyRenderer != null)
            StartCoroutine(HitFlashRoutine());

        if (CurrentHp <= 0f)
        {
            CurrentHp = 0f;
            EventBus.Publish(new PlayerDiedEvent());
        }
    }

    IEnumerator HitFlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < invincibleDuration)
        {
            bodyRenderer.color = new Color(1f, 1f, 1f, 0.2f);
            yield return new WaitForSeconds(0.05f);
            bodyRenderer.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            elapsed += 0.1f;
        }
        bodyRenderer.color = Color.white;
    }

    public void Heal(float amount)
    {
        CurrentHp = Mathf.Min(CurrentHp + amount, maxHp);
        EventBus.Publish(new PlayerHpChangedEvent { Current = CurrentHp, Max = maxHp });
    }
}
