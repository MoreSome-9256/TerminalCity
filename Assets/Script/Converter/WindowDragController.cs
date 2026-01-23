using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private RectTransform validArea;
    private RectTransform windowTransform;
    private Canvas canvas;
    private float scaleFactor;
    public Window window;

    // 缓存 Window 组件引用
    private Window myWindow;

    public void Init(RectTransform area)
    {
        windowTransform = GetComponent<RectTransform>();
        myWindow = GetComponent<Window>(); // 获取 Window 组件

        // 向上查找 Canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null && area != null)
        {
            canvas = area.GetComponentInParent<Canvas>();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (windowTransform == null || canvas == null) return;

        // 1. 移动父节点自身
        // 使用 scaleFactor 处理不同分辨率下的拖拽速度
        windowTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // 2. 【关键】立刻通知 Window 更新子节点位置
        // 这样每一帧都在重新计算，看起来就是完全同步的
        if (myWindow != null)
        {
            myWindow.UpdateChildrenLayout();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (validArea == null || canvas == null) return;

        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
            validArea,
            eventData.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera
        );

        if (!isInside)
        {
            windowTransform.anchoredPosition = Vector2.zero;
            // 复位后也要更新子节点位置，否则子节点会留在原地
            if (myWindow != null)
            {
                myWindow.UpdateChildrenLayout();
            }
        }
    }
}