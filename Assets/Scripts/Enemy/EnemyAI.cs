using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyBase))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float detectionRange = 6f;

    [Header("Attack")]
    [SerializeField] float attackRange = 1.2f;
    [SerializeField] float attackDamage = 10f;
    [SerializeField] float attackCooldown = 1.5f;

    [Header("Patrol")]
    [SerializeField] LayerMask groundLayer;

    enum State { Patrol, Chase, Attack }
    State _state = State.Patrol;

    Rigidbody2D _rb;
    SpriteRenderer _sr;
    Transform _player;
    float _patrolDir = 1f;
    float _attackTimer;
    float _flipCooldown;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    void Update()
    {
        if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;
        if (_player == null) return;

        float dist = Vector2.Distance(transform.position, _player.position);
        _state = dist <= attackRange ? State.Attack
               : dist <= detectionRange ? State.Chase
               : State.Patrol;
    }

    void FixedUpdate()
    {
        if (_player == null) return;
        switch (_state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase:  DoChase();  break;
            case State.Attack: DoAttack(); break;
        }
    }

    void DoPatrol()
    {
        if (_flipCooldown > 0f)
        {
            _flipCooldown -= Time.fixedDeltaTime;
        }
        else
        {
            bool wallAhead = Physics2D.Raycast(transform.position, Vector2.right * _patrolDir, 0.6f, groundLayer);
            bool groundAhead = Physics2D.Raycast(
                (Vector2)transform.position + Vector2.right * _patrolDir * 0.5f,
                Vector2.down, 1.5f, groundLayer);

            if (wallAhead || !groundAhead)
            {
                _patrolDir *= -1f;
                _flipCooldown = 0.5f;
            }
        }

        _rb.linearVelocity = new Vector2(_patrolDir * moveSpeed, _rb.linearVelocity.y);
        if (_sr != null) _sr.flipX = _patrolDir < 0f;
    }

    void DoChase()
    {
        float dir = _player.position.x > transform.position.x ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dir * moveSpeed, _rb.linearVelocity.y);
        if (_sr != null) _sr.flipX = dir < 0f;
    }

    void DoAttack()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        if (_attackTimer > 0f) return;
        _attackTimer = attackCooldown;
        DamageSystem.Damage(_player.gameObject, attackDamage);
    }
}
