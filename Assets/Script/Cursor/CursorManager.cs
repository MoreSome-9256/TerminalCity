using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour
{
    [Header("光标数据 (可在 Inspector 中拖动 hotspot)")]
    public CursorData uiCursor;   // 全局 UI 光标
    public CursorData mapCursor;  // 地图光标

    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults;

    private CursorData currentCursor;

    void Start()
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        raycastResults = new List<RaycastResult>();

        // 默认设置地图光标
        SetCursor(mapCursor);
    }

    void Update()
    {
        pointerEventData.position = Input.mousePosition;
        raycastResults.Clear();

        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        if (raycastResults.Count > 0)
        {
            // 取最上层 UI
            RaycastResult topResult = raycastResults[0];

            Canvas canvas = topResult.gameObject.GetComponentInParent<Canvas>();

            if (canvas != null && canvas.sortingOrder > 10) // 假设全局 UI sortingOrder >= 10
            {
                SetCursor(uiCursor);
                return;
            }
        }

        // 默认用地图光标
        SetCursor(mapCursor);
    }

    void SetCursor(CursorData data)
    {
        if (data == null || data.texture == null) return;

        // 避免每帧重复调用
        if (currentCursor == data) return;

        Cursor.SetCursor(data.texture, data.hotspot, CursorMode.Auto);
        currentCursor = data;
    }
}
