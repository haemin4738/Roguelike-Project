using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int amount = 10;
    public Sprite[] coinFrames;

    SpriteRenderer _sr;
    float _animTimer;
    int _frame;

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    void Update()
    {
        if (coinFrames == null || coinFrames.Length == 0) return;
        _animTimer += Time.deltaTime;
        if (_animTimer >= 0.1f)
        {
            _animTimer = 0f;
            _frame = (_frame + 1) % coinFrames.Length;
            _sr.sprite = coinFrames[_frame];
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GoldWallet.Add(amount);
        EventBus.Publish(new ItemPickedEvent { ItemName = $"Gold x{amount}" });
        Destroy(gameObject);
    }
}
