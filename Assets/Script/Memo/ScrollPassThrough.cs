using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollPassThrough : MonoBehaviour, IScrollHandler
{
    private ScrollRect parentScrollRect;

    private void Awake()
    {
        // 自动向上寻找外层的 ScrollRect
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnScroll(PointerEventData eventData)
    {
        // 将吃到的滚轮事件直接交给父级 ScrollRect 响应
        if (parentScrollRect != null)
        {
            parentScrollRect.OnScroll(eventData);
        }
    }
}