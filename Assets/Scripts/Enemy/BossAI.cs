using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyBase))]
public class BossAI : MonoBehaviour
{
    [Header("Phase 1 - 근접")]
    [SerializeField] float p1Speed = 2.5f;
    [SerializeField] float meleeDamage = 20f;
    [SerializeField] float meleeRange = 1.5f;
    [SerializeField] float meleeCooldown = 1.2f;

    [Header("Phase 2 - 원거리 (HP 50% 이하)")]
    [SerializeField] float p2Speed = 3.5f;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed = 8f;
    [SerializeField] float projectileDamage = 15f;
    [SerializeField] float rangedCooldown = 1.8f;
    [SerializeField] float rangedRange = 8f;

    EnemyBase _base;
    Rigidbody2D _rb;
    SpriteRenderer _sr;
    Transform _player;
    float _attackTimer;
    bool _phase2;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _base = GetComponent<EnemyBase>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    void Update()
    {
        if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;
        if (!_phase2 && _base.HpFraction <= 0.5f) _phase2 = true;
    }

    void FixedUpdate()
    {
        if (_player == null) return;
        float dist = Vector2.Distance(transform.position, _player.position);
        float speed = _phase2 ? p2Speed : p1Speed;

        if (dist <= meleeRange)
            Melee();
        else if (_phase2 && dist <= rangedRange)
            Ranged();
        else
            MoveToward(speed);
    }

    void MoveToward(float speed)
    {
        float dir = _player.position.x > transform.position.x ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dir * speed, _rb.linearVelocity.y);
        if (_sr != null) _sr.flipX = dir < 0f;
    }

    void Melee()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        if (_attackTimer > 0f) return;
        _attackTimer = meleeCooldown;
        DamageSystem.Damage(_player.gameObject, meleeDamage);
    }

    void Ranged()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        if (_attackTimer > 0f || projectilePrefab == null) return;
        _attackTimer = rangedCooldown;
        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        go.GetComponent<ProjectileBase>().Init(dir, projectileSpeed, projectileDamage, "Player");
    }
}
