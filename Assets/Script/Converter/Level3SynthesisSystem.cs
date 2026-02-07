using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level3SynthesisSystem : MonoBehaviour
{
    private List<Level3Rule> rules = new List<Level3Rule>();

    void Start()
    {
        LoadLevel3Rules();
    }

    void LoadLevel3Rules()
    {
        rules.Clear();

        TextAsset csv = Resources.Load<TextAsset>("ExchangeRules2");
        if (csv == null)
        {
            Debug.LogError("未找到 ExchangeRules2.csv");
            return;
        }

        Debug.Log($"Level3 CSV 原始内容:\n{csv.text}");

        string[] lines = csv.text.Split('\n');

        foreach (string rawLine in lines.Skip(1)) // 跳过表头
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 4)
            {
                Debug.LogWarning($"跳过无效行: {line}");
                continue;
            }

            // parts[1] -> Items
            // parts[2] -> Weights
            // parts[3] -> Result

            string[] itemStrs = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] weightStrs = parts[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (itemStrs.Length != weightStrs.Length)
            {
                Debug.LogError($"规则物品数与权重数不一致，已跳过: {line}");
                continue;
            }

            List<int> items = new List<int>();
            List<float> weights = new List<float>();

            bool parseError = false;

            for (int i = 0; i < itemStrs.Length; i++)
            {
                if (!int.TryParse(itemStrs[i], out int itemID))
                {
                    Debug.LogError($"无法解析物品ID: {itemStrs[i]}");
                    parseError = true;
                    break;
                }

                if (!float.TryParse(weightStrs[i], out float w))
                {
                    Debug.LogError($"无法解析权重: {weightStrs[i]}");
                    parseError = true;
                    break;
                }

                items.Add(itemID);
                weights.Add(w);
            }

            if (parseError) continue;

            if (!int.TryParse(parts[3], out int resultID))
            {
                Debug.LogError($"无法解析结果ID: {parts[3]}");
                continue;
            }

            Level3Rule rule = new Level3Rule
            {
                items = items,
                weights = weights,
                resultItem = resultID
            };

            rules.Add(rule);

            Debug.Log(
                $"加载三级规则: [{string.Join(" ", items)}] " +
                $"权重[{string.Join(" ", weights)}] -> {resultID}"
            );
        }

        Debug.Log($"三级规则加载完成，总数: {rules.Count}");
    }
    public bool TrySynthesize(
    List<int> playerItems,
    out int resultID,
    out float similarityPercent,
    float threshold = 0.7f,
    float extraPenalty = 0.2f   // ⭐ 每个多余物品扣多少权重（可调）
)
    {
        resultID = -1;
        similarityPercent = 0f;

        if (playerItems == null || playerItems.Count == 0)
            return false;

        float bestScore = 0f;
        Level3Rule bestRule = null;

        foreach (var rule in rules)
        {
            float matchedWeight = 0f;

            // 计算命中权重
            for (int i = 0; i < rule.items.Count; i++)
            {
                int ruleItem = rule.items[i];
                float weight = rule.weights[i];

                if (playerItems.Contains(ruleItem))
                {
                    matchedWeight += weight;
                }
            }

            // 计算多余物品数量
            int extraCount = playerItems.Count(id => !rule.items.Contains(id));

            // 计算惩罚
            float penalty = extraCount * extraPenalty;

            // 最终得分
            float finalWeight = Mathf.Max(0, matchedWeight - penalty);
            float similarity = finalWeight / rule.TotalWeight;

            Debug.Log(
                $"规则[{string.Join(" ", rule.items)}] " +
                $"命中权重={matchedWeight:F2}, 多余={extraCount}, 扣分={penalty:F2}, " +
                $"最终相似度={similarity * 100f:F1}%"
            );

            if (similarity > bestScore)
            {
                bestScore = similarity;
                bestRule = rule;
            }
        }

        similarityPercent = bestScore * 100f;

        if (bestRule != null && bestScore >= threshold)
        {
            resultID = bestRule.resultItem;
            return true;
        }

        return false;
    }

}
