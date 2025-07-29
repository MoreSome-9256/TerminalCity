using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GraphZoomController : MonoBehaviour
{
    public RectTransform graphRoot;  // 控制缩放的根节点（比如 PanelGraphRoot）
    public float zoomSpeed = 0.1f;
    public float panSpeed = 1.0f;
    public float minZoom = 0.5f;
    public float maxZoom = 2.0f;

    private float currentZoom = 1f;
    private bool isPanning = false;
    private Vector2 lastMousePosition;

    public float maxDragDistance = 200f;
    public RectTransform viewport;
    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom = Mathf.Clamp(currentZoom + scroll * zoomSpeed, minZoom, maxZoom);
            graphRoot.localScale = Vector3.one * currentZoom;
        }
    }

    void HandlePan()
    {
        // 鼠标左键按下时
        if (Input.GetMouseButtonDown(0))
        {
            // 只在没点中节点时开始拖拽
            if (!IsClickingCharacterNode())
            {
                isPanning = true;
                lastMousePosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(0) && isPanning)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            Vector2 newPos = graphRoot.anchoredPosition + delta * panSpeed;
            graphRoot.anchoredPosition = ClampToBounds(newPos);
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
        }
    }
    bool IsClickingCharacterNode()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<CharacterNode>() != null)
                return true;
        }

        return false;
    }
    Vector2 ClampToBounds(Vector2 targetPos)
    {
        if (viewport == null || graphRoot == null) return targetPos;

        Vector2 viewportSize = viewport.rect.size;
        Vector2 scaledGraphSize = graphRoot.rect.size * currentZoom;

        // 假设你希望图不要被拖出 viewport 边缘太多，就做如下限制
        float limitX = Mathf.Max(0, (scaledGraphSize.x - viewportSize.x) / 2f + maxDragDistance);
        float limitY = Mathf.Max(0, (scaledGraphSize.y - viewportSize.y) / 2f + maxDragDistance);

        targetPos.x = Mathf.Clamp(targetPos.x, -limitX, limitX);
        targetPos.y = Mathf.Clamp(targetPos.y, -limitY, limitY);

        return targetPos;
    }
}
