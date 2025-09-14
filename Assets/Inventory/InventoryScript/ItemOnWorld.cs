using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ItemOnWorld : MonoBehaviour, IPointerClickHandler
{
    private RelationshipGraph relationshipGraph;

    public Level1Data item;
    public Inventory playerInventory;
    public UnityEvent onClick;

    public AudioClip clickSound;
    private AudioSource audioSource;

    private GameObject previewArea;
    private GameObject firstLevel;

    private void Awake()
    {
        relationshipGraph = FindObjectOfType<RelationshipGraph>();

        if (UIManager.Instance != null)
        {
            previewArea = UIManager.Instance.previewArea;
            firstLevel = UIManager.Instance.firstLevel;
        }
        else
        {
            Debug.LogError("UIManager 未找到，请确保场景中有 UIManager");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (previewArea == null) Debug.LogError("未找到 PreView 面板！");
        if (firstLevel == null) Debug.LogError("未找到 FirstLevel 面板！");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        ShowPreView();
        AddNewItem();
        AddGraph();
        AddDictionaryEntries();

        Destroy(gameObject);
        onClick?.Invoke();
    }

    private void AddNewItem()
    {
        if (firstLevel != null)
        {
            firstLevel.SetActive(true); // 激活背包 UI
        }

        if (playerInventory != null)
        {
            playerInventory.level1List.Add(item);
        }

        InventoryManager.CreateNewItem(item);
        InventoryManager2.CreateNewItem(item);

        if (firstLevel != null)
        {
            firstLevel.SetActive(false); // 再隐藏
        }
    }

    private void ShowPreView()
    {
        if (previewArea == null) return;

        // 通过 SidePanelManager 保证同时只显示一个 Panel
        SidePanelManager.Instance.ShowPanel(previewArea);

        // 刷新面板内容
        PreView.ShowInformation(item);
    }
    private void AddGraph()
    {
        if (item != null && relationshipGraph != null)
        {
            relationshipGraph.UnlockCharacterByLevel1Data(item);
        }
    }
    private void AddDictionaryEntries()
    {
        if (item != null && item.materialData != null && DictionaryManager.Instance != null)
        {
            Debug.Log("AddDictionaryEntries");

            foreach (var entry in item.materialData.entries)
            {
                // 添加正式词条 + 解释
                DictionaryManager.Instance.AddTerm(entry.term);
                if (!string.IsNullOrEmpty(entry.definition))
                    DictionaryManager.Instance.AddDefinition(entry.term, entry.definition);

                // 将资料里的别名都加入正式词条的 aliases
                if (entry.aliases != null)
                {
                    foreach (var alias in entry.aliases)
                    {
                        DictionaryManager.Instance.AddAliasToTerm(entry.term, alias.Trim());
                    }
                }
                if (item.materialData.aliases != null)
                {
                    foreach (var alias in item.materialData.aliases)
                    {
                        DictionaryManager.Instance.AddAliasToTerm(entry.term, alias.Trim());
                    }
                }
            }

            DictionaryManager.Instance.RefreshUI();
            Debug.Log($"[Dictionary] 已更新 {item.itemName} 的资料，共 {item.materialData.entries.Count} 个词条");
        }
    }
}
