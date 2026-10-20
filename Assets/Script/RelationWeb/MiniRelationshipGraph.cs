using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniRelationshipGraph : MonoBehaviour
{
    [Header("数据与引用")]
    public GameData gameData;
    public Transform nodesParent;           // 小窗口的节点容器 Content
    public GameObject miniNodePrefab;       // 小角色预制体（MiniCharacterNode）
    public LineManager miniLineManager;     // 小窗口专属的 LineManager（负责小窗口的连线）

    [Header("缩放适配参数")]
    [Tooltip("大图坐标换算到小窗口的缩放系数，例如 0.25f ~ 0.35f")]
    public float positionScale = 0.3f;

    // 记录小窗口已生成的节点实例
    private Dictionary<string, RectTransform> miniNodes = new Dictionary<string, RectTransform>();
    private HashSet<string> miniConnectedPairs = new HashSet<string>();

    private void OnEnable()
    {
        // 每次打开合成界面小窗口时，拉取最新的状态并同步
        SyncFromMainGraph();
    }

    /// <summary>
    /// 从主关系图拉取所有已解锁角色和已连线关系
    /// </summary>
    public void SyncFromMainGraph()
    {
        if (RelationshipGraph.Instance == null || gameData == null) return;

        // 1. 同步已解锁的角色节点
        foreach (var charName in RelationshipGraph.Instance.GetUnlockedCharacters())
        {
            CreateMiniNodeIfNotExist(charName);
        }

        // 2. 同步已连接的关系连线
        foreach (var pairKey in RelationshipGraph.Instance.GetConnectedPairs())
        {
            CreateMiniLineIfNotExist(pairKey);
        }
    }

    /// <summary>
    /// 生成单个迷你节点
    /// </summary>
    public void CreateMiniNodeIfNotExist(string characterName)
    {
        if (miniNodes.ContainsKey(characterName)) return;

        CharacterInfo info = gameData.AllCharacters.Find(c => c.Name == characterName);
        if (info == null) return;

        GameObject obj = Instantiate(miniNodePrefab, nodesParent);
        RectTransform rt = obj.GetComponent<RectTransform>();

        // 核心：把大图的 DefaultPosition 等比缩小映射到小窗口
        rt.anchoredPosition = info.DefaultPosition * positionScale;

        // 初始化小头像和名字（如果有）
        CharacterNode nodeUI = obj.GetComponent<CharacterNode>();
        if (nodeUI != null)
        {
            if (nodeUI.PortraitImage != null) nodeUI.PortraitImage.sprite = info.Portrait;
            if (nodeUI.NameText != null) nodeUI.NameText.text = info.Name;
        }

        miniNodes.Add(characterName, rt);
    }

    /// <summary>
    /// 生成迷你连线
    /// </summary>
    public void CreateMiniLineIfNotExist(string pairKey)
    {
        if (miniConnectedPairs.Contains(pairKey)) return;

        string[] parts = pairKey.Split('|');
        if (parts.Length != 2) return;

        string nameA = parts[0];
        string nameB = parts[1];

        if (miniNodes.TryGetValue(nameA, out RectTransform rtA) &&
            miniNodes.TryGetValue(nameB, out RectTransform rtB))
        {
            if (miniLineManager != null)
            {
                miniLineManager.DrawLineBetween(rtA, rtB);
                miniConnectedPairs.Add(pairKey);
            }
        }
    }
}