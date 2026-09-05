using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] CharacterAnimData animData;

    Rigidbody2D _rb;
    Sprite[] _current;
    int _frame;
    float _timer;

    public void SetAnimData(CharacterAnimData data)
    {
        animData = data;
        _current = data?.idleFrames;
        _frame = 0;
        _timer = 0f;
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        _current = animData != null ? animData.idleFrames : null;
    }

    void Update()
    {
        if (targetRenderer == null || animData == null) return;

        bool moving = _rb != null && Mathf.Abs(_rb.linearVelocity.x) > 0.1f;
        var next = (moving && animData.runFrames != null && animData.runFrames.Length > 0)
            ? animData.runFrames : animData.idleFrames;

        if (next != _current) { _current = next; _frame = 0; _timer = 0f; }
        if (_current == null || _current.Length == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / animData.fps)
        {
            _timer -= 1f / animData.fps;
            _frame = (_frame + 1) % _current.Length;
            targetRenderer.sprite = _current[_frame];
        }
    }
}
