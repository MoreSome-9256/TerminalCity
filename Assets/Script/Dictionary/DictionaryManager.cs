using UnityEngine;
using System.Collections.Generic;
using System;

public class DictionaryManager : MonoBehaviour
{
    public static DictionaryManager Instance;

    [Header("词典数据")]
    public Dictionary<string, DictionaryEntry> entries = new Dictionary<string, DictionaryEntry>();

    [Header("UI 相关")]
    public Transform dictionaryContentParent; // ScrollView 的 Content 节点
    public GameObject entryPrefab;            // DictionaryEntryUI prefab

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 添加词条，并可附带别名
    public void AddTerm(string term, string[] aliases = null)
    {
        if (!entries.ContainsKey(term))
        {
            entries[term] = new DictionaryEntry(term, "", true, false, aliases);
        }
        else
        {
            entries[term].hasTerm = true;

            // 更新别名，如果有新的别名就加进去
            if (aliases != null)
            {
                var existingAliases = new List<string>(entries[term].aliases);
                foreach (var a in aliases)
                {
                    if (!existingAliases.Contains(a))
                        existingAliases.Add(a);
                }
                entries[term].aliases = existingAliases.ToArray();
            }
        }
    }

    // 添加解释
    public void AddDefinition(string term, string definition)
    {
        if (!entries.ContainsKey(term))
        {
            entries[term] = new DictionaryEntry(term, definition, false, true);
        }
        else
        {
            entries[term].definition = definition;
            entries[term].hasDefinition = true;
        }
    }

    // 刷新 UI 界面
    public void RefreshUI()
    {
        //Debug.Log("RefreshUI");
        // 清理旧的 UI
        foreach (Transform child in dictionaryContentParent)
        {
            Destroy(child.gameObject);
        }

        // 创建新的 UI
        foreach (var kvp in entries)
        {
            DictionaryEntry entry = kvp.Value;

            // 只有收集到 term 的才显示
            if (entry.hasTerm)
            {
                GameObject obj = Instantiate(entryPrefab, dictionaryContentParent);
                var ui = obj.GetComponent<DictionaryEntryUI>();
                ui.Setup(entry.term, entry.definition, entry.hasDefinition);
            }
        }
    }
    public void AddAliasToTerm(string term, string alias)
    {
        if (string.IsNullOrEmpty(alias)) return;

        if (!entries.ContainsKey(term)) return; // 正式词条必须存在

        var existing = new List<string>(entries[term].aliases);
        if (!existing.Contains(alias))
            existing.Add(alias);
        entries[term].aliases = existing.ToArray();
    }
    public string GetDefinition(string term)
    {
        if (string.IsNullOrEmpty(term)) return null;

        // 先直接查 term
        if (entries.TryGetValue(term, out var entry))
            return entry.definition;

        // 再检查别名
        foreach (var kvp in entries)
        {
            if (kvp.Value.aliases != null)
            {
                foreach (var alias in kvp.Value.aliases)
                {
                    if (alias.Equals(term, StringComparison.OrdinalIgnoreCase))
                        return kvp.Value.definition;
                }
            }
        }

        return null; // 没找到
    }

}
