using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicTimelineView : MonoBehaviour
{
    [Header("UI 容器与组件")]
    [SerializeField] private RectTransform contentRect;       // ScrollView 的 Content
    [SerializeField] private RectTransform mainAxisLine;      // 主竖线 Image
    [SerializeField] private GameObject nodePrefab;           // TimelineNode 预制体（挂了 TimelineNodeUI）

    [Header("布局间距参数 (像素)")]
    [SerializeField] private float topPadding = 60f;
    [SerializeField] private float bottomPadding = 60f;
    [SerializeField] private float minItemSpacing = 70f;
    [SerializeField] private float maxItemSpacing = 220f;
    [SerializeField] private float timeElasticFactor = 35f;

    /// <summary>
    /// 清空所有动态生成的节点（保留主竖线）
    /// </summary>
    public void ClearGeneratedNodes()
    {
        if (contentRect == null) return;

        // 从后往前遍历删除，避免索引变化漏删
        for (int i = contentRect.childCount - 1; i >= 0; i--)
        {
            Transform child = contentRect.GetChild(i);
            if (mainAxisLine != null && child == mainAxisLine.transform) continue;

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 支持传入已解锁条目列表与未读 ID 集合
    /// </summary>
    public void BuildTimeline(List<TimelineEventData> unlockedEntries, HashSet<string> unreadEventIds)
    {
        if (contentRect == null) return;

        // 1️⃣ 第一步：无条件把所有的旧节点清理干净！
        ClearGeneratedNodes();

        // 2️⃣ 若无已解锁项，直接重置高度并隐藏主轴
        if (unlockedEntries == null || unlockedEntries.Count == 0)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 0);
            if (mainAxisLine != null) mainAxisLine.gameObject.SetActive(false);
            return;
        }

        // 3️⃣ 严格按时间从小到大排序
        var sortedList = unlockedEntries.OrderBy(e => e.exactYear).ToList();

        // 4️⃣ 计算每个节点的 Y 坐标
        List<float> yPositions = new List<float>();
        float currentY = -topPadding;
        yPositions.Add(currentY);

        for (int i = 0; i < sortedList.Count - 1; i++)
        {
            float deltaYears = Mathf.Max(0f, sortedList[i + 1].exactYear - sortedList[i].exactYear);
            float dynamicSpacing = minItemSpacing + Mathf.Log(1f + deltaYears) * timeElasticFactor;
            dynamicSpacing = Mathf.Clamp(dynamicSpacing, minItemSpacing, maxItemSpacing);

            currentY -= dynamicSpacing;
            yPositions.Add(currentY);
        }

        // 5️⃣ 更新 Content 总高度
        float totalHeight = Mathf.Abs(currentY) + bottomPadding;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

        // 获取主竖线的 Pos X，让每个节点以竖线中心为原点对齐
        float axisX = mainAxisLine != null ? mainAxisLine.anchoredPosition.x : 0f;

        // 6️⃣ 实例化并配置节点
        for (int i = 0; i < sortedList.Count; i++)
        {
            TimelineEventData entry = sortedList[i];
            GameObject nodeObj = Instantiate(nodePrefab, contentRect);
            RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();

            // 纵向定位与横向对齐主轴
            nodeRect.anchoredPosition = new Vector2(axisX, yPositions[i]);

            // 判断是否显示 NewPrompt
            bool isNew = unreadEventIds != null && unreadEventIds.Contains(entry.id);

            TimelineNodeUI nodeUI = nodeObj.GetComponent<TimelineNodeUI>();
            if (nodeUI != null)
            {
                nodeUI.Setup(entry, isNew);
            }
        }

        // 7️⃣ 调整主轴竖线位置和长度
        if (mainAxisLine != null)
        {
            mainAxisLine.gameObject.SetActive(true);
            mainAxisLine.SetAsFirstSibling();

            float startY = yPositions[0];
            float endY = yPositions[yPositions.Count - 1];
            float lineHeight = Mathf.Abs(endY - startY);

            if (sortedList.Count == 1)
            {
                lineHeight = 60f;
                mainAxisLine.sizeDelta = new Vector2(mainAxisLine.sizeDelta.x, lineHeight);
                mainAxisLine.anchoredPosition = new Vector2(mainAxisLine.anchoredPosition.x, startY + 30f);
            }
            else
            {
                mainAxisLine.sizeDelta = new Vector2(mainAxisLine.sizeDelta.x, lineHeight + 30f);
                mainAxisLine.anchoredPosition = new Vector2(mainAxisLine.anchoredPosition.x, startY + 15f);
            }
        }
    }
}