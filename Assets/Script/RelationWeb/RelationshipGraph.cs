using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RelationshipGraph : MonoBehaviour
{
    private LineManager lineManager;

    [Header("全局数据")]
    public GameData gameData;

    [Header("节点父物体")]
    public Transform nodesParent;

    [Header("节点Prefab")]
    public GameObject nodePrefab;

    private Dictionary<string, CharacterNode> nodes = new Dictionary<string, CharacterNode>();
    public static RelationshipGraph Instance { get; private set; }

    public List<CharacterNode> selectedNodes = new List<CharacterNode>();
    private HashSet<string> connectedPairs = new HashSet<string>();
    private HashSet<string> unlockedCharacters = new HashSet<string>();

    public IEnumerable<string> GetUnlockedCharacters() => unlockedCharacters;
    public IEnumerable<string> GetConnectedPairs() => connectedPairs;

    private const string SAVE_KEY_CHARS = "TC_RelGraph_UnlockedChars";
    private const string SAVE_KEY_PAIRS = "TC_RelGraph_ConnectedPairs";

    private void Awake()
    {
        lineManager = FindObjectOfType<LineManager>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 尝试从存档恢复之前解锁的角色与连线
        LoadGraphData();
    }

    /// <summary>
    /// 根据玩家拾取到的 Level1Data 解锁角色节点
    /// </summary>
    public void UnlockCharacterByLevel1Data(Level1Data data)
    {
        if (data == null || string.IsNullOrEmpty(data.linkedCharacterName))
        {
            return;
        }

        UnlockCharacterDirectly(data.linkedCharacterName);
    }

    /// <summary>
    /// 直接根据角色名字解锁生成节点（支持读档与外部调用）
    /// </summary>
    public void UnlockCharacterDirectly(string characterName)
    {
        if (string.IsNullOrEmpty(characterName) || nodes.ContainsKey(characterName)) return;

        if (gameData == null || gameData.AllCharacters == null)
        {
            Debug.LogWarning("[RelationshipGraph] GameData 未配置或为空！");
            return;
        }

        CharacterInfo info = gameData.AllCharacters.Find(c => c.Name == characterName);
        if (info == null)
        {
            Debug.LogWarning($"未在 GameData 中找到角色【{characterName}】，请检查 GameData 设置！");
            return;
        }

        // 记录已解锁
        unlockedCharacters.Add(info.Name);
        SaveGraphData();

        // 实例化节点
        if (nodePrefab != null && nodesParent != null)
        {
            GameObject obj = Instantiate(nodePrefab, nodesParent);
            var node = obj.GetComponent<CharacterNode>();
            if (node != null)
            {
                node.Init(info);
            }
            nodes.Add(info.Name, node);
        }

        // 通知小图
        var miniGraph = FindObjectOfType<MiniRelationshipGraph>();
        if (miniGraph != null && miniGraph.gameObject.activeInHierarchy)
        {
            miniGraph.CreateMiniNodeIfNotExist(info.Name);
        }
    }

    public void OnNodeClicked(CharacterNode node, bool isSelected)
    {
        if (isSelected)
        {
            if (!selectedNodes.Contains(node))
            {
                selectedNodes.Add(node);
            }
        }
        else
        {
            selectedNodes.Remove(node);
        }
    }

    private string GetPairKey(string a, string b)
    {
        return string.Compare(a, b) < 0 ? $"{a}|{b}" : $"{b}|{a}";
    }

    public void TryConnectSelectedNodes()
    {
        if (selectedNodes.Count != 2) return;

        var nodeA = selectedNodes[0];
        var nodeB = selectedNodes[1];
        string nameA = nodeA.CharacterName;
        string nameB = nodeB.CharacterName;
        string key = GetPairKey(nameA, nameB);

        if (connectedPairs.Contains(key))
        {
            Debug.Log("这对角色已经连接过了！");
            return;
        }

        if (gameData != null && gameData.HasRelationship(nameA, nameB))
        {
            RectTransform rtA = nodeA.GetComponent<RectTransform>();
            RectTransform rtB = nodeB.GetComponent<RectTransform>();

            if (lineManager != null)
            {
                lineManager.DrawLineBetween(rtA, rtB);
            }

            connectedPairs.Add(key);
            SaveGraphData();
            Debug.Log("连接成功！");

            var miniGraph = FindObjectOfType<MiniRelationshipGraph>();
            if (miniGraph != null && miniGraph.gameObject.activeInHierarchy)
            {
                miniGraph.CreateMiniLineIfNotExist(key);
            }
        }
        else
        {
            Debug.Log("连接错误！触发惩罚");
        }

        foreach (var node in selectedNodes)
        {
            node.SetSelectedVisual(false);
            node.SetIsSelected(false);
        }
        selectedNodes.Clear();
    }

    #region 存档与读档

    private void SaveGraphData()
    {
        PlayerPrefs.SetString(SAVE_KEY_CHARS, string.Join(",", unlockedCharacters));
        PlayerPrefs.SetString(SAVE_KEY_PAIRS, string.Join(",", connectedPairs));
        PlayerPrefs.Save();
    }

    private void LoadGraphData()
    {
        string charsStr = PlayerPrefs.GetString(SAVE_KEY_CHARS, "");
        if (!string.IsNullOrEmpty(charsStr))
        {
            string[] charArray = charsStr.Split(',');
            foreach (var charName in charArray)
            {
                if (!string.IsNullOrEmpty(charName))
                {
                    UnlockCharacterDirectly(charName);
                }
            }
        }

        string pairsStr = PlayerPrefs.GetString(SAVE_KEY_PAIRS, "");
        if (!string.IsNullOrEmpty(pairsStr))
        {
            string[] pairArray = pairsStr.Split(',');
            foreach (var pair in pairArray)
            {
                if (!string.IsNullOrEmpty(pair) && !connectedPairs.Contains(pair))
                {
                    connectedPairs.Add(pair);
                    string[] parts = pair.Split('|');
                    if (parts.Length == 2 && nodes.ContainsKey(parts[0]) && nodes.ContainsKey(parts[1]))
                    {
                        if (lineManager != null)
                        {
                            lineManager.DrawLineBetween(
                                nodes[parts[0]].GetComponent<RectTransform>(),
                                nodes[parts[1]].GetComponent<RectTransform>()
                            );
                        }
                    }
                }
            }
        }
    }

    [ContextMenu("🗑️ 清空人物关系存档")]
    public void ClearGraphSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY_CHARS);
        PlayerPrefs.DeleteKey(SAVE_KEY_PAIRS);
        PlayerPrefs.Save();
        unlockedCharacters.Clear();
        connectedPairs.Clear();
        Debug.Log("人物关系存档已重置！");
    }

    #endregion
}