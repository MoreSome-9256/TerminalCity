using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ItemDialogPair
{
    public int itemID;
    public UnityEvent dialogEvent;
}

public class DialogTriggerAfterInventory : MonoBehaviour
{
    private SideScreenMove bagController;
    private bool shouldTriggerDialogAfterClose = false;

    private UnityEvent pendingDialogEvent = null; // ← 当前等待触发的对话事件

    [Header("ItemID → 对话事件映射")]
    [SerializeField] private List<ItemDialogPair> itemDialogMappings = new List<ItemDialogPair>();

    private Dictionary<int, UnityEvent> itemDialogDict = new Dictionary<int, UnityEvent>();

    void Awake()
    {
        foreach (var pair in itemDialogMappings)
        {
            if (!itemDialogDict.ContainsKey(pair.itemID))
            {
                itemDialogDict.Add(pair.itemID, pair.dialogEvent);
            }
        }
    }

    /// <summary>
    /// 【通用入口】请求在背包关闭后触发某个对话
    /// </summary>
    public void RequestTriggerAfterInventoryClosed(SideScreenMove bagController, UnityEvent eventToTrigger)
    {
        this.bagController = bagController;
        if (!bagController.isMoved)
        {
            Debug.Log("背包已关闭，立即触发对话");
            eventToTrigger?.Invoke();
        }
        else
        {
            Debug.Log("背包未关闭，等待关闭后触发对话");
            pendingDialogEvent = eventToTrigger;
            shouldTriggerDialogAfterClose = true;
            bagController.OnBagClosed.AddListener(CheckAndTrigger);
        }
    }

    /// <summary>
    /// 【背包物品调用】请求根据 itemID 查找并触发对应对话
    /// </summary>
    public void RequestTriggerByItemID(int itemID, SideScreenMove bagController)
    {
        Debug.Log("RequestTriggerByItemID");
        if (itemDialogDict.TryGetValue(itemID, out UnityEvent dialogEvent))
        {
            RequestTriggerAfterInventoryClosed(bagController, dialogEvent);
        }
        else
        {
            Debug.LogWarning($"未在 dialogMappings 中找到 itemID = {itemID} 的对话事件");
        }
    }

    private void CheckAndTrigger()
    {
        if (shouldTriggerDialogAfterClose && pendingDialogEvent != null)
        {
            shouldTriggerDialogAfterClose = false;
            bagController.OnBagClosed.RemoveListener(CheckAndTrigger);
            pendingDialogEvent?.Invoke();
            pendingDialogEvent = null;
        }
    }
}
