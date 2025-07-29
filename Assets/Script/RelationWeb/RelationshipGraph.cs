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

    // 存所有已生成节点，避免重复生成
    private Dictionary<string, CharacterNode> nodes = new Dictionary<string, CharacterNode>();
    private static RelationshipGraph instance;

    public List<CharacterNode> selectedNodes = new List<CharacterNode>();
    private HashSet<string> connectedPairs = new HashSet<string>();


    private void Awake()
    {
        lineManager = FindObjectOfType<LineManager>();
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 根据玩家拾取到的 Level1Data 解锁角色节点
    /// </summary>
    /// <param name="data">玩家拾取到的资料</param>
    public void UnlockCharacterByLevel1Data(Level1Data data)
    {
        if (string.IsNullOrEmpty(data.linkedCharacterName))
        {
            Debug.Log($"资料【{data.Name}】不关联任何角色，无需生成节点。");
            return;
        }

        if (nodes.ContainsKey(data.linkedCharacterName))
        {
            Debug.Log($"角色【{data.linkedCharacterName}】节点已经生成，无需重复生成。");
            return;
        }

        // 从 GameData 里查找角色信息
        CharacterInfo info = gameData.AllCharacters.Find(c => c.Name == data.linkedCharacterName);
        if (info == null)
        {
            Debug.LogWarning($"未在 GameData 中找到角色【{data.linkedCharacterName}】，请检查 GameData 设置！");
            return;
        }

        // 实例化节点
        GameObject obj = Instantiate(nodePrefab, nodesParent);
        var node = obj.GetComponent<CharacterNode>();

        // 初始化节点信息
        node.Init(info);

        // 把新节点加入已生成字典
        nodes.Add(info.Name, node);

        //Debug.Log($"成功生成角色节点：【{info.Name}】");
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

        //Debug.Log($"当前选中的节点数：{selectedNodes.Count}");
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

        if (gameData.HasRelationship(nameA, nameB))
        {
            // 获取节点实际的RectTransform实例
            RectTransform rtA = nodeA.GetComponent<RectTransform>();
            RectTransform rtB = nodeB.GetComponent<RectTransform>();

            lineManager.DrawLineBetween(rtA, rtB);
            connectedPairs.Add(key);
            Debug.Log("连接成功！");
        }
        else
        {
            Debug.Log("连接错误！触发惩罚");
        }

        // 清除选择状态...
        foreach (var node in selectedNodes)
        {
            node.SetSelectedVisual(false);
            node.SetIsSelected(false);
        }
        selectedNodes.Clear();
    }
}
