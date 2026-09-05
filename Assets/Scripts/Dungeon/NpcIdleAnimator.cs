using UnityEngine;

public class NpcIdleAnimator : MonoBehaviour
{
    public Sprite[] frames;
    [SerializeField] float fps = 6f;

    SpriteRenderer _sr;
    int _frame;
    float _timer;

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        _timer += Time.deltaTime;
        if (_timer >= 1f / fps)
        {
            _timer -= 1f / fps;
            _frame = (_frame + 1) % frames.Length;
            _sr.sprite = frames[_frame];
        }
    }
}
