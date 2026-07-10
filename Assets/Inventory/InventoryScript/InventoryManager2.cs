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
        if (item == null || IsItemDisplayed(item)) return;

        // 统一通过 Inventory 添加
        myBag?.AddItem(item);

        // 创建 UI
        CreateNewItem(item);
    }

    /// <summary>
    /// 创建 UI slot
    /// </summary>
    private void CreateNewItem(Item item)
    {
        if (item == null || instance == null) return;

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

        // 获取当前背包实际存在的物品
        HashSet<int> currentItems = new HashSet<int>();
        foreach (var item in myBag.GetPickedItems())
            currentItems.Add(item.itemNum);

        // 1️⃣ 移除已经不在背包的 UI
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

        // 2️⃣ 添加新增的 UI
        foreach (var item in myBag.GetPickedItems())
        {
            if (!displayedItemIds.Contains(item.itemNum))
                CreateNewItem(item);
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
