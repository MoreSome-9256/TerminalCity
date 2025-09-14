using TMPro;
using UnityEngine;

public class EntryWindow : MonoBehaviour
{
    public TextMeshProUGUI definitionText;
    public System.Action onClosed;

    public void Show(string definition, Vector2 screenPos, Canvas rootCanvas, Camera eventCam)
    {
        definitionText.text = definition;

        var rect = transform as RectTransform;
        var rootRect = rootCanvas.transform as RectTransform;

        // 统一锚点/枢轴，避免偏移
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.0f, 0.0f); // 让弹窗“左上角”对准点，更自然

        // Render Mode 对应的相机
        Camera cam = null;
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera || rootCanvas.renderMode == RenderMode.WorldSpace)
            cam = eventCam != null ? eventCam : rootCanvas.worldCamera;

        // 屏幕 → 根 Canvas 局部
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, screenPos, cam, out var localPos);
        rect.anchoredPosition = localPos;

        // 可选：把弹窗限制在屏幕内
        ClampToCanvas(rect, rootRect);
    }
    public void Close()
    {
        onClosed?.Invoke();
        Destroy(gameObject);
    }
    private void ClampToCanvas(RectTransform window, RectTransform root)
    {
        var size = window.rect.size;
        Vector2 pos = window.anchoredPosition;
        var rootHalf = root.rect.size * 0.5f;

        float left = -rootHalf.x;
        float right = rootHalf.x - size.x;
        float bottom = -rootHalf.y;
        float top = rootHalf.y - size.y;

        pos.x = Mathf.Clamp(pos.x, left, right);
        pos.y = Mathf.Clamp(pos.y, bottom, top);

        window.anchoredPosition = pos;
    }
}
