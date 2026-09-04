using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartDisplay : MonoBehaviour
{
    [SerializeField] Sprite fullHeart;
    [SerializeField] Sprite halfHeart;
    [SerializeField] Sprite emptyHeart;
    [SerializeField] Vector2 heartSize = new(32, 32);
    [SerializeField] int hpPerHeart = 20;

    readonly List<Image> _hearts = new();

    public void SetHearts(float current, float max)
    {
        int count = Mathf.Max(1, Mathf.CeilToInt(max / hpPerHeart));
        SyncCount(count);
        for (int i = 0; i < count; i++)
        {
            float segment = current - i * hpPerHeart;
            if (segment >= hpPerHeart)
                _hearts[i].sprite = fullHeart;
            else if (segment > 0)
                _hearts[i].sprite = halfHeart;
            else
                _hearts[i].sprite = emptyHeart;
        }
    }

    void SyncCount(int count)
    {
        while (_hearts.Count < count)
        {
            var go = new GameObject("Heart", typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = emptyHeart;
            go.GetComponent<RectTransform>().sizeDelta = heartSize;
            _hearts.Add(img);
        }
        while (_hearts.Count > count)
        {
            Destroy(_hearts[^1].gameObject);
            _hearts.RemoveAt(_hearts.Count - 1);
        }
    }
}
