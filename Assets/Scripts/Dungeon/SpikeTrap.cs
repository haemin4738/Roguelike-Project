using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public Sprite[] frames;                     // DebugDungeonGenerator에서 주입 (f0~f3)
    [SerializeField] float damage = 1f;
    [SerializeField] float damageCooldown = 0.5f;
    [SerializeField] float frameTime = 0.12f;   // 프레임 간격 (초)

    SpriteRenderer _sr;
    float _nextDamageTime;
    float _animTimer;
    int _frame;

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        _animTimer += Time.deltaTime;
        if (_animTimer >= frameTime)
        {
            _animTimer -= frameTime;
            _frame = (_frame + 1) % frames.Length;
            _sr.sprite = frames[_frame];
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < _nextDamageTime) return;
        DamageSystem.Damage(other.gameObject, damage);
        _nextDamageTime = Time.time + damageCooldown;
    }
}
