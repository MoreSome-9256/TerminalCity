using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerminalAreaExplorationUI : MonoBehaviour
{
    [Header("背包数据引用")]
    [SerializeField] private Inventory playerInventory;

    [Header("方块容器 (直接拖入 BlocksGrid 即可自动读取子节点)")]
    [SerializeField] private Transform blocksGrid;

    // 缓存每个方块的内部遮罩（Block 下的 Image）
    private List<GameObject> blockInnerMasks = new List<GameObject>();
    private List<Level1Data> currentAreaL1Items = new List<Level1Data>();
    private int lastPlaceNumber = -999;

    private void Awake()
    {
        CacheBlockUnits();
    }

    private void CacheBlockUnits()
    {
        blockInnerMasks.Clear();
        if (blocksGrid == null) return;

        // 遍历 BlocksGrid 下的所有 Block0 ~ Block12
        for (int i = 0; i < blocksGrid.childCount; i++)
        {
            Transform blockTrans = blocksGrid.GetChild(i);

            // 查找内部作为深色遮罩的 Image 子物体
            Transform innerImage = blockTrans.Find("Image");
            if (innerImage != null)
            {
                blockInnerMasks.Add(innerImage.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += RefreshExplorationDisplay;
        }
        InitAreaAndRefresh();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshExplorationDisplay;
        }
    }

    private void Start()
    {
        InitAreaAndRefresh();
    }

    private void Update()
    {
        // 区域编号变更时重新统计
        if (RoomManager.Instance != null && RoomManager.Instance.placeNumber != lastPlaceNumber)
        {
            InitAreaAndRefresh();
        }
    }

    /// <summary>
    /// 初始化区域资料并刷新
    /// </summary>
    public void InitAreaAndRefresh()
    {
        if (RoomManager.Instance == null) return;

        lastPlaceNumber = RoomManager.Instance.placeNumber;
        CollectCurrentAreaItems();
        RefreshExplorationDisplay();
    }

    /// <summary>
    /// 统计属于当前区域的一级资料
    /// </summary>
    private void CollectCurrentAreaItems()
    {
        currentAreaL1Items.Clear();

        if (RoomManager.Instance == null) return;

        List<int> currentRooms = RoomManager.Instance.roomIDsInArea;

        Level1Data[] allL1 = Resources.LoadAll<Level1Data>("Items/Level1");
        if (allL1 == null || allL1.Length == 0)
        {
            allL1 = Resources.LoadAll<Level1Data>("Item/Level1");
        }

        // 仅保留 room 属于本区域房间的资料
        if (allL1 != null && currentRooms != null)
        {
            foreach (var l1 in allL1)
            {
                if (l1 != null && currentRooms.Contains(l1.room))
                {
                    currentAreaL1Items.Add(l1);
                }
            }
        }
    }

    /// <summary>
    /// 根据比例刷新格子点亮状态
    /// </summary>
    public void RefreshExplorationDisplay()
    {
        if (blockInnerMasks.Count == 0)
        {
            CacheBlockUnits();
        }

        int totalCountInArea = currentAreaL1Items.Count;
        int collectedCountInArea = 0;

        if (playerInventory != null)
        {
            foreach (var ownedItem in playerInventory.level1List)
            {
                if (ownedItem != null && currentAreaL1Items.Contains(ownedItem))
                {
                    collectedCountInArea++;
                }
            }
        }

        // 1. 计算收集比例 (0.0 ~ 1.0)
        float ratio = totalCountInArea > 0 ? (float)collectedCountInArea / totalCountInArea : 0f;

        // 2. 根据比例换算应该点亮多少格
        int totalBlocks = blockInnerMasks.Count;
        int activeBlocksCount = (collectedCountInArea >= totalCountInArea && totalCountInArea > 0)
            ? totalBlocks
            : Mathf.RoundToInt(ratio * totalBlocks);

        // 3. 逐格控制内部深色遮罩的显隐
        for (int i = 0; i < totalBlocks; i++)
        {
            if (blockInnerMasks[i] == null) continue;

            bool isLit = i < activeBlocksCount;

            // 点亮时不显示内部遮罩（露出完整实心大矩形）；未点亮时显示遮罩（呈现线框）
            blockInnerMasks[i].SetActive(!isLit);
        }
    }
}