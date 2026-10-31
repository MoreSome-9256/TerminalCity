using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntryLinkHandler : MonoBehaviour, IPointerClickHandler
{
    public EntryWindow entryWindowPrefab;   // 词条弹窗 Prefab（下面的 EntryWindow.cs）
    private EntryWindow currentWindow;

    private TMP_Text textComponent;
    private Canvas rootCanvas;

    [SerializeField] private Button exitButton;  // Inspector 里的退出按钮

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        rootCanvas = textComponent.canvas.rootCanvas; // 用根 Canvas
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("<color=yellow>[EntryLinkHandler] 1. 成功接收到点击事件！</color>");

        textComponent.ForceMeshUpdate();

        // 🌟 核心修复：如果 Canvas 是 ScreenSpaceOverlay，相机必须传 null，否则 TMP 会判定点击失效
        Camera cam = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : eventData.pressEventCamera;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, cam);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();
            Debug.Log($"<color=green>[EntryLinkHandler] 2. 成功点中链接，解析到的 linkID: '{linkID}'</color>");

            string definition = DictionaryManager.Instance.GetDefinition(linkID);
            if (string.IsNullOrEmpty(definition))
            {
                Debug.LogError($"<color=red>[EntryLinkHandler] 3. 拦截！在字典中查不到 linkID: '{linkID}' 的解释。请检查字典数据是否正确加载，或 linkID 是否有多余空格。</color>");
                return;
            }

            Debug.Log("<color=cyan>[EntryLinkHandler] 4. 获取解释成功，准备生成并显示弹窗...</color>");

            if (currentWindow != null)
            {
                Destroy(currentWindow.gameObject);
                currentWindow = null;
            }

            // 获取中心点并生成弹窗
            Vector2 screenPos = GetLinkScreenCenter(textComponent, linkInfo, cam);
            currentWindow = Instantiate(entryWindowPrefab, rootCanvas.transform);

            currentWindow.Show(
                definition,
                eventData.position,
                rootCanvas,
                cam // 同样传入兼容的 cam
            );

            if (exitButton != null) exitButton.interactable = false;
        }
        else
        {
            Debug.Log("[EntryLinkHandler] 点击位置未命中任何有效链接 (-1)，可能是点在了文字的空白处。");
            if (currentWindow != null)
            {
                Destroy(currentWindow.gameObject);
                currentWindow = null;
                if (exitButton != null) exitButton.interactable = true;
            }
        }
    }

    /// <summary>
    /// 取当前链接在屏幕上的矩形中心（把链接内所有字符的包围盒合并）
    /// </summary>
    private Vector2 GetLinkScreenCenter(TMP_Text text, TMP_LinkInfo linkInfo, Camera eventCam)
    {
        var charInfos = text.textInfo.characterInfo;
        int first = linkInfo.linkTextfirstCharacterIndex;              // 注意：大小写与版本一致
        int last = first + linkInfo.linkTextLength - 1;

        // 合并包围盒
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

        for (int i = first; i <= last; i++)
        {
            if (i < 0 || i >= charInfos.Length) continue;
            var c = charInfos[i];
            // 字符四个顶点是本地空间 → 转世界 → 再转屏幕
            var bl = text.transform.TransformPoint(c.bottomLeft);
            var tl = text.transform.TransformPoint(c.topLeft);
            var br = text.transform.TransformPoint(c.bottomRight);
            var tr = text.transform.TransformPoint(c.topRight);

            Vector3[] pts = { bl, tl, br, tr };
            foreach (var p in pts)
            {
                var sp = RectTransformUtility.WorldToScreenPoint(eventCam, p);
                if (sp.x < min.x) min.x = sp.x;
                if (sp.y < min.y) min.y = sp.y;
                if (sp.x > max.x) max.x = sp.x;
                if (sp.y > max.y) max.y = sp.y;
            }
        }

        return (Vector2)((min + max) * 0.5f);
    }
}
