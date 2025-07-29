using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private RectTransform validArea;
    private RectTransform windowTransform;
    private Canvas canvas;
    private float scaleFactor;

    public void Init(RectTransform area)
    {
        validArea = area;
        windowTransform = GetComponent<RectTransform>();

        // 直接通过validArea获取父级Canvas（适用于ScreenSpaceOverlay）
        canvas = validArea.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("找不到父级Canvas！请确保validArea在Canvas层级下");
            return;
        }
        scaleFactor = canvas.scaleFactor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (windowTransform == null || canvas == null) return;
        windowTransform.anchoredPosition += eventData.delta / scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (validArea == null || canvas == null) return;

        // 强制使用ScreenSpaceOverlay参数（第三个参数传null）
        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
            validArea,
            eventData.position,
            null // 显式指定摄像机为null
        );

        if (!isInside)
        {
            windowTransform.anchoredPosition = Vector2.zero;
        }
    }
}