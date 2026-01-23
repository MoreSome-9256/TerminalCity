using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager6 : MonoBehaviour
{
    public static InventoryManager6 Instance;

    [Header("背包数据（用于三级合成）")]
    public Inventory myBag;

    [Header("UI References")]
    public GameObject slotGrid;
    public Slot2 slotPrefab;
    [SerializeField] private ReverseGridLayout gridLayout;

    // 当前显示的 UI
    private readonly List<GameObject> inventoryItems = new List<GameObject>();
    private readonly HashSet<int> displayedItemIds = new HashSet<int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
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

    public bool IsItemDisplayed(Item item)
    {
        if (item == null) return false;
        return displayedItemIds.Contains(item.itemNum);
    }

    public void AddItemToInventory(Item item)
    {
        if (item == null || IsItemDisplayed(item)) return;

        myBag?.AddItem(item);
        CreateNewItem(item);
    }

    private void CreateNewItem(Item item)
    {
        if (item == null) return;

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

        newSlot.slotName.text = item.itemName;

        inventoryItems.Add(newSlot.gameObject);
        displayedItemIds.Add(item.itemNum);

        gridLayout?.AddItemToTop(newSlot.gameObject);
    }

    public void RefreshInventoryUI()
    {
        if (myBag == null) return;

        HashSet<int> currentItems = new HashSet<int>();
        foreach (var item in myBag.GetAllItems())
            currentItems.Add(item.itemNum);

        // 移除不存在的
        for (int i = inventoryItems.Count - 1; i >= 0; i--)
        {
            Slot2 slot = inventoryItems[i].GetComponent<Slot2>();
            if (slot == null || slot.slotItem == null || !currentItems.Contains(slot.slotItem.itemNum))
            {
                if (slot != null && slot.slotItem != null)
                    displayedItemIds.Remove(slot.slotItem.itemNum);

                Destroy(inventoryItems[i]);
                inventoryItems.RemoveAt(i);
            }
        }

        // 添加新增的
        foreach (var item in myBag.GetAllItems())
        {
            if (!displayedItemIds.Contains(item.itemNum))
                CreateNewItem(item);
        }
    }

    public void RemoveItem(Item item)
    {
        if (item == null) return;
        myBag?.RemoveItem(item);
    }
}
