using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using UnityEngine.Events;

public class Exchange : MonoBehaviour, IPointerClickHandler
{
    public Inventory playerInventory;
    public GameObject secondLevel;
    public GameObject secondLevel2;

    [SerializeField] private CraftingProgress craftingProgress;

    Level2Data loadedItem = null;

    // 修改后的合成点击逻辑
    public void OnPointerClick(PointerEventData eventData)
    {
        // 防止重复点击
        if (craftingProgress.IsRunning) return;

        CraftingSystem crafting = FindObjectOfType<CraftingSystem>();

        // 先检查是否能合成，避免不必要的进度条显示
        if (!crafting.CanCraft(out int resultItemID))
        {
            HandleCraftFailure();
            return;
        }
        if (IsItemAlreadyInInventory(resultItemID))
        {
            RepetitiveCrafting();
            //craftingProgress.OnProgressFailed?.Invoke(); // 添加失败事件
            return;
        }

        // 启动进度条
        craftingProgress.ShowProgress(() =>
        {
            /*if (IsItemAlreadyInInventory(resultItemID))
            {
                RepetitiveCrafting();
                craftingProgress.OnProgressFailed?.Invoke(); // 添加失败事件
                return;
            }*/
            StartCoroutine(CompleteCraftRoutine(resultItemID));
        });
    }
    private IEnumerator CompleteCraftRoutine(int resultItemID)
    {
        bool isLoading = true;

        // 启动异步加载
        StartCoroutine(LoadItemAsync(resultItemID, (item) => {
            loadedItem = item;
            isLoading = false;
        }));

        // 等待加载完成（同时保持进度条可见）
        while (isLoading)
        {
            yield return null;
        }

        if (loadedItem == null) yield break;

        // 执行物品添加逻辑
        secondLevel.SetActive(true);
        secondLevel2.SetActive(true);
        playerInventory.level2List.Add(loadedItem);
        InventoryManager3.CreateNewItem(loadedItem);
        InventoryManager4.CreateNewItem(loadedItem);
        secondLevel.SetActive(false);
        secondLevel2.SetActive(false);
        craftingProgress.OnProgressComplete1?.Invoke();
    }

    private IEnumerator LoadItemAsync(int itemNum, System.Action<Level2Data> callback)
    {
        // 改用同步加载但分帧处理
        Level2Data[] allItems = Resources.LoadAll<Level2Data>("Items/");

        // 分帧查找避免卡顿
        foreach (Level2Data item in allItems)
        {
            if (item.itemNum == itemNum)
            {
                callback?.Invoke(item);
                yield break;
            }
            yield return null; // 每检查一个物品释放一帧
        }

        // 未找到时的处理
        Debug.LogError($"物品{itemNum}加载失败，已加载{allItems.Length}个物品");
        callback?.Invoke(null);
    }
    private bool IsItemAlreadyInInventory(int itemID)
    {
        // 使用Linq优化遍历查询
        return playerInventory.level2List.Any(item => item.itemNum == itemID);
    }
    private void HandleCraftFailure()
    {
        Debug.Log("合成失败");
        loadedItem = null; // 清理加载的物品
        craftingProgress.OnProgressFailed?.Invoke();
        craftingProgress.HideImmediate(); // 确保立即隐藏进度条
    }
    public void HandleCraftSuccess()
    {
        NewData.ShowInformation(loadedItem);
    }
    private void RepetitiveCrafting()
    {
        Debug.Log("重复合成已存在物品");
        loadedItem = null; // 清理加载的物品
        craftingProgress.OnProgressRepete?.Invoke();
        craftingProgress.HideImmediate();
    }
}