using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EntryLinkHandler : MonoBehaviour, IPointerClickHandler
{
    public EntryWindow entryWindowPrefab;   // 词条弹窗 Prefab（下面的 EntryWindow.cs）
    private EntryWindow currentWindow;

    private TMP_Text textComponent;
    private Canvas rootCanvas;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        rootCanvas = textComponent.canvas.rootCanvas; // 用根 Canvas
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 先让 TMP 生成最新的 mesh/link 信息
        textComponent.ForceMeshUpdate();

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, eventData.pressEventCamera);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();

            string definition = DictionaryManager.Instance.GetDefinition(linkID);
            if (string.IsNullOrEmpty(definition)) return;

            // 已有弹窗 → 先关掉
            if (currentWindow != null)
            {
                Destroy(currentWindow.gameObject);
                currentWindow = null;
            }

            // 计算“链接矩形”的屏幕中心点（更稳）
            Vector2 screenPos = GetLinkScreenCenter(textComponent, linkInfo, eventData.pressEventCamera);

            // 在根 Canvas 下生成弹窗
            currentWindow = Instantiate(entryWindowPrefab, rootCanvas.transform);

            currentWindow.Show(
                definition,
                eventData.position,   // 屏幕坐标
                rootCanvas,
                eventData.pressEventCamera
            );
        }
        else
        {
            // 点到空白 → 关闭弹窗
            if (currentWindow != null)
            {
                Destroy(currentWindow.gameObject);
                currentWindow = null;
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
