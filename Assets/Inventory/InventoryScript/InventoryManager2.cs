using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager2 : MonoBehaviour
{
    public static InventoryManager2 instance;

    [Header("背包数据")]
    public Inventory myBag;

    [Header("UI References")]
    public GameObject slotGrid;
    public Slot2 slotPrefab;
    [SerializeField] private ReverseGridLayout gridLayout; // 自定义布局组件

    // 当前显示的 UI
    public List<GameObject> inventoryItems = new List<GameObject>();
    private HashSet<int> displayedItemIds = new HashSet<int>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        // 初始化 UI
        RefreshInventoryUI();
    }

    void OnEnable()
    {
        if (myBag != null)
            myBag.OnInventoryChanged += RefreshInventoryUI;
    }

    void OnDisable()
    {
        if (myBag != null)
            myBag.OnInventoryChanged -= RefreshInventoryUI;
    }

    /// <summary>
    /// 判断某物品是否已经显示在 UI
    /// </summary>
    public bool IsItemDisplayed(Item item)
    {
        if (item == null) return false;
        return displayedItemIds.Contains(item.itemNum);
    }

    /// <summary>
    /// 往背包里添加物品
    /// </summary>
    public void AddItemToInventory(Item item)
    {
        if (item == null) return;

        // 统一通过 Inventory 添加到数据层，确保任务和全局系统能查到
        myBag?.AddItem(item);

        // 如果是特殊非合成物品，不生成合成背包的 UI
        if (ShouldHideFromInventory(item)) return;

        // 如果已经显示在 UI 上，不重复创建
        if (IsItemDisplayed(item)) return;

        // 创建 UI
        CreateNewItem(item);
    }

    /// <summary>
    /// 判断物品是否应当在此合成背包中隐藏
    /// （如果该物品是 Level1Data 且 itemType == 10，则不显示）
    /// </summary>
    private bool ShouldHideFromInventory(Item item)
    {
        if (item is Level1Data l1Data && l1Data.itemType == 10)
        {
            return true;
        }
        return false;
    }

    private void CreateNewItem(Item item)
    {
        // 关键改动：如果为空，或者属于 itemType == 10 的特殊物品，直接拦截
        if (item == null || instance == null || ShouldHideFromInventory(item)) return;

        Slot2 newSlot = Instantiate(slotPrefab, slotGrid.transform);
        newSlot.transform.SetParent(slotGrid.transform, false);
        newSlot.slotItem = item;

        // 设置图标
        Transform iconTransform = newSlot.transform.Find("Icon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            iconImage.sprite = item.itemImage;
            iconImage.gameObject.SetActive(item.itemImage != null);
        }

        newSlot.slotName.text = item.LocalizedItemName;

        // 记录状态
        inventoryItems.Add(newSlot.gameObject);
        displayedItemIds.Add(item.itemNum);

        // 放到格子顶部
        gridLayout?.AddItemToTop(newSlot.gameObject);
    }

    /// <summary>
    /// 刷新背包 UI（响应 Inventory.OnInventoryChanged）
    /// </summary>
    public void RefreshInventoryUI()
    {
        if (myBag == null) return;

        // 获取当前背包实际存在的【非 10 类型】物品
        HashSet<int> currentItems = new HashSet<int>();
        foreach (var item in myBag.GetPickedItems())
        {
            if (item != null && !ShouldHideFromInventory(item))
            {
                currentItems.Add(item.itemNum);
            }
        }

        // 1️⃣ 移除已经不在背包（或者变成了隐藏类型）的 UI
        for (int i = inventoryItems.Count - 1; i >= 0; i--)
        {
            Slot2 slot = inventoryItems[i].GetComponent<Slot2>();
            if (slot == null || slot.slotItem == null || !currentItems.Contains(slot.slotItem.itemNum))
            {
                displayedItemIds.Remove(slot.slotItem.itemNum);
                Destroy(inventoryItems[i]);
                inventoryItems.RemoveAt(i);
            }
        }

        // 2️⃣ 添加新增的 UI（跳过隐藏物品）
        foreach (var item in myBag.GetPickedItems())
        {
            if (item != null && !ShouldHideFromInventory(item))
            {
                if (!displayedItemIds.Contains(item.itemNum))
                    CreateNewItem(item);
            }
        }
    }

    /// <summary>
    /// 移除物品接口（会触发 Inventory.OnInventoryChanged 自动刷新 UI）
    /// </summary>
    public void RemoveItem(Item item)
    {
        if (item == null) return;
        myBag?.RemoveItem(item);
    }
}
