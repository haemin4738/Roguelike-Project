using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashDisplay : MonoBehaviour
{
    [SerializeField] Color fullColor = new Color(0.3f, 0.8f, 1f);
    [SerializeField] Color emptyColor = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] Color borderColor = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] Vector2 segmentSize = new(48, 28);
    [SerializeField] float dividerWidth = 2f;
    [SerializeField] float padding = 2f;

    readonly List<Image> _segs = new();

    void Awake()
    {
        var img = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        img.color = borderColor;
    }

    void OnEnable() => EventBus.Subscribe<DashChangedEvent>(Refresh);
    void OnDisable() => EventBus.Unsubscribe<DashChangedEvent>(Refresh);

    void Refresh(DashChangedEvent e)
    {
        SyncCount(e.Max);
        for (int i = 0; i < e.Max; i++)
            _segs[i].color = i < e.Current ? fullColor : emptyColor;
    }

    void SyncCount(int count)
    {
        while (_segs.Count < count)
        {
            int idx = _segs.Count;
            var go = new GameObject("DashSeg", typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.color = emptyColor;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = segmentSize;
            rt.anchoredPosition = new Vector2(padding + idx * (segmentSize.x + dividerWidth), 0f);
            _segs.Add(img);
        }
        while (_segs.Count > count)
        {
            Destroy(_segs[^1].gameObject);
            _segs.RemoveAt(_segs.Count - 1);
        }

        float totalWidth = padding * 2 + count * segmentSize.x + Mathf.Max(0, count - 1) * dividerWidth;
        GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth, segmentSize.y + padding * 2);
    }
}
