using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileBase : MonoBehaviour
{
    float _damage;

    public void Init(Vector2 direction, float speed, float damage)
    {
        _damage = damage;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // TODO: DamageSystem 연결 (feat/enemy 브랜치)
            Destroy(gameObject);
        }

        if (other.CompareTag("Ground"))
            Destroy(gameObject);
    }
}
