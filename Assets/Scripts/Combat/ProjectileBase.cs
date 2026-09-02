using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileBase : MonoBehaviour
{
    [SerializeField] float rotationOffset = 0f;

    float _damage;
    string _targetTag = "Enemy";
    Rigidbody2D _rb;
    int _groundLayer;
    bool _initialized;

    public void Init(Vector2 direction, float speed, float damage, string targetTag = "Enemy")
    {
        _damage = damage;
        _targetTag = targetTag;
        _rb = GetComponent<Rigidbody2D>();
        _rb.linearVelocity = direction * speed;
        _rb.gravityScale = 0f;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        _groundLayer = LayerMask.NameToLayer("Ground");

        // 물리 충돌 없이 이동만 하도록 모든 콜라이더를 trigger로
        foreach (var col in GetComponents<Collider2D>())
            col.isTrigger = true;

        _initialized = true;
        Destroy(gameObject, 5f);
    }

    void FixedUpdate()
    {
        if (!_initialized || _rb == null) return;

        Vector2 vel = _rb.linearVelocity;
        float dist = vel.magnitude * Time.fixedDeltaTime + 0.1f;

        var hits = Physics2D.RaycastAll(transform.position, vel.normalized, dist);
        foreach (var hit in hits)
        {
            var go = hit.collider.gameObject;
            if (go == gameObject) continue;
            if (go.GetComponent<ProjectileBase>() != null) continue;

            if (go.CompareTag(_targetTag))
            {
                DamageSystem.Damage(go, _damage);
                Destroy(gameObject);
                return;
            }

            if (go.layer == _groundLayer)
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}
