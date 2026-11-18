using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ItemDialogPair
{
    public int itemID;
    [Tooltip("可绑定任意对话触发器（SimpleDialogue / DialogForPrefab / GlobalDialogManager 等）")]
    public UnityEvent dialogEvent;
    [Tooltip("是否只触发一次（避免反复点开旧资料重复触发）")]
    public bool triggerOnce = true;

    [HideInInspector] public bool hasTriggered = false;
}

[System.Serializable]
public class MultiItemDialogGroup
{
    [Tooltip("仅用于 Inspector 面板备注区分，如：序章线索汇总")]
    public string groupName;

    [Tooltip("需要全部阅读过的物品 itemID 列表")]
    public List<int> requiredItemIDs = new List<int>();

    [Tooltip("集齐并阅读完上述所有资料后，触发的总结对话事件")]
    public UnityEvent dialogEvent;

    public bool triggerOnce = true;

    [HideInInspector] public bool hasTriggered = false;
}

public class DialogTriggerAfterInventory : MonoBehaviour
{
    private SideScreenMove bagController;
    private bool isWaitingForBagClose = false;

    // 统一存放所有待执行的对话事件（无论是单份、多份组合，还是外部 Proxy 传入）
    private readonly Queue<UnityEvent> pendingDialogEvents = new Queue<UnityEvent>();
    private Coroutine playQueueCoroutine;

    [Header("单份资料触发：ItemID → 对话事件映射")]
    [SerializeField] private List<ItemDialogPair> itemDialogMappings = new List<ItemDialogPair>();

    [Header("多份资料组合触发：集齐阅读后触发总结对话")]
    [SerializeField] private List<MultiItemDialogGroup> multiItemDialogGroups = new List<MultiItemDialogGroup>();

    private Dictionary<int, ItemDialogPair> itemDialogDict = new Dictionary<int, ItemDialogPair>();
    private HashSet<int> readItemIDs = new HashSet<int>();

    void Awake()
    {
        foreach (var pair in itemDialogMappings)
        {
            if (!itemDialogDict.ContainsKey(pair.itemID))
            {
                itemDialogDict.Add(pair.itemID, pair);
            }
        }
    }

    /// <summary>
    /// 【通用入口】兼容外部（如 InventoryDialogProxy、InventoryManager）直接传入 UnityEvent 的调用
    /// </summary>
    public void RequestTriggerAfterInventoryClosed(SideScreenMove bagController, UnityEvent eventToTrigger)
    {
        if (eventToTrigger == null) return;

        this.bagController = bagController;
        EnqueueEvent(eventToTrigger);
    }

    /// <summary>
    /// 【背包物品调用】请求根据 itemID 查找并加入待播队列
    /// </summary>
    public void RequestTriggerByItemID(int itemID, SideScreenMove bagController)
    {
        this.bagController = bagController;
        readItemIDs.Add(itemID);

        // 1. 检查单份资料映射
        if (itemDialogDict.TryGetValue(itemID, out ItemDialogPair pair))
        {
            if (!pair.triggerOnce || !pair.hasTriggered)
            {
                pair.hasTriggered = true;
                EnqueueEvent(pair.dialogEvent);
            }
        }

        // 2. 检查多份资料组合
        CheckMultiItemGroups();
    }

    private void CheckMultiItemGroups()
    {
        if (multiItemDialogGroups == null) return;

        foreach (var group in multiItemDialogGroups)
        {
            if (group.triggerOnce && group.hasTriggered) continue;
            if (group.requiredItemIDs == null || group.requiredItemIDs.Count == 0) continue;

            bool allRead = true;
            foreach (int reqId in group.requiredItemIDs)
            {
                if (!readItemIDs.Contains(reqId))
                {
                    allRead = false;
                    break;
                }
            }

            if (allRead)
            {
                group.hasTriggered = true;
                Debug.Log($"[DialogTriggerAfterInventory] 已读完组合 [{group.groupName}]，加入总结对话队列！");
                EnqueueEvent(group.dialogEvent);
            }
        }
    }

    private void EnqueueEvent(UnityEvent evt)
    {
        if (evt == null) return;

        // 防止同一个 UnityEvent 实例在同一次开背包期间重复入队
        if (!pendingDialogEvents.Contains(evt))
        {
            pendingDialogEvents.Enqueue(evt);
        }

        if (bagController != null && bagController.isMoved)
        {
            if (!isWaitingForBagClose)
            {
                isWaitingForBagClose = true;
                bagController.OnBagClosed.AddListener(OnBagClosedHandler);
            }
        }
        else
        {
            // 背包本来就是关闭状态，直接启动串行播放协程
            StartQueuePlayback();
        }
    }

    private void OnBagClosedHandler()
    {
        isWaitingForBagClose = false;
        if (bagController != null)
        {
            bagController.OnBagClosed.RemoveListener(OnBagClosedHandler);
        }

        StartQueuePlayback();
    }

    private void StartQueuePlayback()
    {
        if (playQueueCoroutine == null && pendingDialogEvents.Count > 0)
        {
            playQueueCoroutine = StartCoroutine(PlayQueueRoutine());
        }
    }

    /// <summary>
    /// 核心串行调度协程：触发一段后，等待全局对话窗口关闭再放行下一段
    /// </summary>
    private IEnumerator PlayQueueRoutine()
    {
        while (pendingDialogEvents.Count > 0)
        {
            // 如果当前正好有别的对话还在播，先等它结束
            if (UIManager.Instance != null)
            {
                yield return new WaitWhile(() => UIManager.Instance.IsAnyDialoguePlaying);
            }

            UnityEvent nextEvent = pendingDialogEvents.Dequeue();
            nextEvent?.Invoke();

            // 等待一帧，确保刚才 Invoke 的对话系统已经把 dialogueUI.SetActive(true) 执行完
            yield return null;

            // 持续等待，直到这段对话播完并关闭了 dialogueUI
            if (UIManager.Instance != null)
            {
                yield return new WaitWhile(() => UIManager.Instance.IsAnyDialoguePlaying);
            }
        }

        playQueueCoroutine = null;
    }

    /// <summary>
    /// 供测试重置工具清空所有已读与已触发记录
    /// </summary>
    public void ResetReadRecords()
    {
        readItemIDs.Clear();
        pendingDialogEvents.Clear();

        if (playQueueCoroutine != null)
        {
            StopCoroutine(playQueueCoroutine);
            playQueueCoroutine = null;
        }

        foreach (var pair in itemDialogMappings) pair.hasTriggered = false;
        foreach (var group in multiItemDialogGroups) group.hasTriggered = false;
    }
}