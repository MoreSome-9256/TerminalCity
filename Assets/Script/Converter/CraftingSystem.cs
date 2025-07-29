using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Linq;
using System;

public class CraftingSystem : MonoBehaviour
{
    // 所有合成规则的存储池
    private List<CraftingRule> rules = new List<CraftingRule>();

    void Start()
    {
        LoadCraftingRules(); // 游戏启动时加载规则
    }

    // 核心方法1：加载CSV规则
    void LoadCraftingRules()
    {
        rules = new List<CraftingRule>();
        TextAsset csv = Resources.Load<TextAsset>("ExchangeRules");

        // 诊断原始内容
        Debug.Log($"CSV原始内容:\n{csv.text}");

        string[] lines = csv.text.Split('\n');
        foreach (string line in lines.Skip(1))
        { // 跳过标题行
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 3)
            {
                Debug.LogWarning($"跳过无效行: {line}");
                continue;
            }

            // 解析空格分隔的物品ID
            string[] itemIDs = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> requiredItems = new List<int>();

            foreach (string idStr in itemIDs)
            {
                if (int.TryParse(idStr, out int id))
                {
                    requiredItems.Add(id);
                }
                else
                {
                    Debug.LogError($"无法解析物品ID: {idStr}");
                }
            }

            if (requiredItems.Count > 0 && int.TryParse(parts[2], out int resultItem))
            {
                rules.Add(new CraftingRule
                {
                    requiredItems = requiredItems,
                    resultItem = resultItem
                });
            }
        }
    }
    // 核心方法2：判断能否合成
    public bool CanCraft(out int resultItemID)
    {
        resultItemID = -1;

        // 获取所有非空Window的物品ID
        var currentItems = FindObjectsOfType<Window>()
            .Where(w => w.windowItem != null && w.ItemNum != 0)
            .Select(w => w.ItemNum)
            .ToList();

        if (currentItems.Count == 0) return false;

        // 诊断当前物品
        Debug.Log($"当前物品栏: [{string.Join(" ", currentItems)}]");
        Debug.Log(rules.Count);

        foreach (CraftingRule rule in rules)
        {
            // 诊断规则检查
            Debug.Log($"正在检查规则: [{string.Join(" ", rule.requiredItems)}] -> {rule.resultItem}");

            // 匹配逻辑（顺序无关+数量完全匹配）
            if (currentItems.Count == rule.requiredItems.Count &&
                currentItems.All(rule.requiredItems.Contains) &&
                rule.requiredItems.All(currentItems.Contains))
            {
                resultItemID = rule.resultItem;
                return true;
            }
        }
        return false;
    }
}