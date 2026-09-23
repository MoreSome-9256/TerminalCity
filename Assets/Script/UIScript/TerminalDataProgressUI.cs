using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerminalDataProgressUI : MonoBehaviour
{
    [Header("玩家背包数据库引用")]
    [SerializeField] private Inventory playerInventory;

    [Header("UI 组件引用")]
    [SerializeField] private TMP_Text totalPercentText;   // 显示综合百分比（如 56%）
    [SerializeField] private Image innerBarL1;            // 内圈弧形条 (Level1Data)
    [SerializeField] private Image outerBarL2;            // 外侧弧形条 (Level2Data)

    [Header("弧形填充范围限制")]
    [Tooltip("弧形进度条占 360 度的最大比例。若切图是整圆但只显示 1/4 弧，可设为 0.25")]
    [Range(0f, 1f)][SerializeField] private float maxFillAngleRatio = 0.25f;

    [Header("平滑过渡速度")]
    [SerializeField] private float smoothSpeed = 6f;

    // 静态资料库总容量缓存
    private int totalAllCount = 0;
    private int totalL1Count = 0;
    private int totalL2Count = 0;

    private float currentDisplayedTotal = 0f;
    private float targetL1Ratio = 0f;
    private float targetL2Ratio = 0f;
    private float targetTotalRatio = 0f;

    private void Awake()
    {
        InitDatabaseTotals();
    }

    private void InitDatabaseTotals()
    {
        // 兼容两种可能的文件路径：Items 或 Item
        Item[] allItems = Resources.LoadAll<Item>("Items");
        if (allItems == null || allItems.Length == 0)
        {
            allItems = Resources.LoadAll<Item>("Item");
        }

        Level1Data[] l1Items = Resources.LoadAll<Level1Data>("Items/Level1");
        if (l1Items == null || l1Items.Length == 0)
        {
            l1Items = Resources.LoadAll<Level1Data>("Item/Level1");
        }

        Level2Data[] l2Items = Resources.LoadAll<Level2Data>("Items/Level2");
        if (l2Items == null || l2Items.Length == 0)
        {
            l2Items = Resources.LoadAll<Level2Data>("Item/Level2");
        }

        totalAllCount = allItems != null ? allItems.Length : 0;
        totalL1Count = l1Items != null ? l1Items.Length : 0;
        totalL2Count = l2Items != null ? l2Items.Length : 0;

        Debug.Log($"<color=#00FF00>[Terminal] 静态资料总数扫描结果 -> 总计: {totalAllCount}, L1: {totalL1Count}, L2: {totalL2Count}</color>");
    }

    private void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += CalculateProgress;
        }

        CalculateProgress();
        currentDisplayedTotal = targetTotalRatio;
        UpdateUIInstant();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= CalculateProgress;
        }
    }

    private void Start()
    {
        // 再次强制刷新一次，防止 Awake/OnEnable 阶段背包数据尚未就绪
        CalculateProgress();
        currentDisplayedTotal = targetTotalRatio;
        UpdateUIInstant();
    }

    private void Update()
    {
        currentDisplayedTotal = Mathf.Lerp(currentDisplayedTotal, targetTotalRatio, Time.deltaTime * smoothSpeed);

        if (totalPercentText != null)
        {
            int percentInt = Mathf.RoundToInt(currentDisplayedTotal * 100f);
            totalPercentText.text = $"{percentInt}%";
        }

        if (innerBarL1 != null)
        {
            float targetFill = targetL1Ratio * maxFillAngleRatio;
            innerBarL1.fillAmount = Mathf.Lerp(innerBarL1.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
        }

        if (outerBarL2 != null)
        {
            float targetFill = targetL2Ratio * maxFillAngleRatio;
            outerBarL2.fillAmount = Mathf.Lerp(outerBarL2.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
        }
    }

    [ContextMenu("Recalculate Progress (统计收集度)")]
    public void CalculateProgress()
    {
        if (totalAllCount == 0)
        {
            InitDatabaseTotals();
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("[Terminal] playerInventory 槽位未赋值！");
            return;
        }

        int currentL1 = playerInventory.level1List != null ? playerInventory.level1List.Count : 0;
        int currentL2 = playerInventory.level2List != null ? playerInventory.level2List.Count : 0;
        int currentL3 = playerInventory.level3List != null ? playerInventory.level3List.Count : 0;

        targetL1Ratio = totalL1Count > 0 ? (float)currentL1 / totalL1Count : 0f;
        targetL2Ratio = totalL2Count > 0 ? (float)currentL2 / totalL2Count : 0f;

        int totalOwned = currentL1 + currentL2 + currentL3;
        targetTotalRatio = totalAllCount > 0 ? (float)totalOwned / totalAllCount : 0f;

        Debug.Log($"[Terminal] 收集度刷新: 背包拥有 {totalOwned}/{totalAllCount} -> 计算比率: {targetTotalRatio:P1}");
    }

    private void UpdateUIInstant()
    {
        if (totalPercentText != null)
        {
            totalPercentText.text = $"{Mathf.RoundToInt(targetTotalRatio * 100f)}%";
        }
        if (innerBarL1 != null)
        {
            innerBarL1.fillAmount = targetL1Ratio * maxFillAngleRatio;
        }
        if (outerBarL2 != null)
        {
            outerBarL2.fillAmount = targetL2Ratio * maxFillAngleRatio;
        }
    }
}