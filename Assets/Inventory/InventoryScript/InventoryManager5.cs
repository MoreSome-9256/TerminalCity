using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager5 : MonoBehaviour
{
    public static InventoryManager5 instance;

    public Inventory myBag;
    public GameObject slotGrid;
    public Slot3 slotPrefab;
    [Header("UI References")]
    [SerializeField] private ReverseGridLayout gridLayout; // 自定义布局组件
    public List<GameObject> inventoryItems; // 背包中的物品列表

    // 用来记录当前 UI 已显示的 itemId，防止重复创建 UI
    private HashSet<int> displayedItemIds = new HashSet<int>();

    private List<Slot3> selectedSlots = new List<Slot3>();

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;
    }
    void Start()
    {
        // 在游戏开始时把已标记为 isPicked 的物品加载到背包（使用统一入口）
        if (myBag != null)
        {
            foreach (var item in myBag.GetPickedItems())
            {
                AddItemToInventory(item);
            }
        }
    }
    public bool IsItemDisplayed(Item item)
    {
        if (item == null) return false;
        return displayedItemIds.Contains(item.itemNum);
    }

    // 统一的“添加到背包”接口，用于外部调用
    public void AddItemToInventory(Item item)
    {
        if (item == null) return;
        // 过滤掉 isImportant=true 的物品，不显示在背包
        if (item is Level1Data level1 && level1.isImportant)
        {
            Debug.Log($"跳过重要物品: {level1.itemName}");
            return;
        }
        if (IsItemDisplayed(item)) return;

        if (item is Level1Data level1Data)
        {
            if (myBag != null && !myBag.level1List.Contains(level1Data))
            {
                myBag.level1List.Add(level1Data);
            }
            level1Data.isPicked = true;
        }

        CreateNewItem(item);
    }
    public static void CreateNewItem(Item item)
    {
        Slot3 newItem = Instantiate(instance.slotPrefab, instance.slotGrid.transform.position, Quaternion.identity);

        // 设置父级（建议使用SetParent的规范写法）
        newItem.transform.SetParent(instance.slotGrid.transform, false);

        // 设置数据
        newItem.slotItem = item;

        Transform iconTransform = newItem.transform.Find("Icon"); // 直接通过名称查找
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            iconImage.sprite = item.itemImage;
            iconImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("找不到Icon子物体！");
        }
        newItem.slotName.text = item.LocalizedItemName;
        instance.gridLayout.AddItemToTop(newItem.gameObject);
    }
    public void ToggleSelect(Slot3 slot)
    {
        if (slot.isSelected)
        {
            if (!selectedSlots.Contains(slot))
                selectedSlots.Add(slot);

            // 限制最多 1 件（初期）
            if (selectedSlots.Count > 1)
            {
                // 超出后取消最新的
                slot.Deselect();
                selectedSlots.Remove(slot);
            }
        }
        else
        {
            selectedSlots.Remove(slot);
        }
    }
    public void OnDiscardButtonClicked()
    {
        if (selectedSlots.Count == 0) return;

        foreach (Slot3 slot in selectedSlots)
        {
            if (slot != null && slot.slotItem != null)
            {
                Item item = slot.slotItem;

                // 从 Inventory ScriptableObject 中移除
                myBag.RemoveItem(item);

                // 删除 UI 格子
                Destroy(slot.gameObject);
            }
        }

        selectedSlots.Clear();

        // 降低混乱度（当前减 50%）
        PlayerChaos.Instance.ReduceChaos(0.5f);
    }
}
