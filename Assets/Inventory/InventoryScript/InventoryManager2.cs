using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager2 : MonoBehaviour
{
    static InventoryManager2 instance;

    public Inventory myBag;
    public GameObject slotGrid;
    public Slot2 slotPrefab;
    [Header("UI References")]
    [SerializeField] private ReverseGridLayout gridLayout; // 自定义布局组件
    public List<GameObject> inventoryItems; // 背包中的物品列表

    // 用来记录当前 UI 已显示的 itemId，防止重复创建 UI
    private HashSet<int> displayedItemIds = new HashSet<int>();

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

        if (IsItemDisplayed(item)) return;

        if (item is Level1Data level1)
        {
            if (myBag != null && !myBag.level1List.Contains(level1))
            {
                myBag.level1List.Add(level1);
            }
            level1.isPicked = true;
        }

        CreateNewItem(item);
    }

    // 创建 UI 的方法
    /*public static void CreateNewItem(Item item)
    {
        if (instance == null || item == null) return;

        // 二次防重：如果已经显示了就不再创建
        if (instance.IsItemDisplayed(item)) return;

        // 推荐使用带 parent 的 Instantiate，这样 transform 不会跑偏
        Slot newItem = Instantiate(instance.slotPrefab, instance.slotGrid.transform);
        newItem.transform.SetParent(instance.slotGrid.transform, false);

        newItem.slotItem = item;

        Transform iconTransform = newItem.transform.Find("Icon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = item.itemImage;
                iconImage.gameObject.SetActive(item.itemImage != null);
            }
        }
        else
        {
            Debug.LogError("找不到Icon子物体！");
        }

        newItem.slotName.text = item.itemName;

        if (item is Level1Data level1Data)
        {
            string traits = "";
            if (!string.IsNullOrEmpty(level1Data.Name)) traits += "人物 ";
            if (!string.IsNullOrEmpty(level1Data.Time)) traits += "时间 ";
            if (!string.IsNullOrEmpty(level1Data.Event)) traits += "事件 ";
            if (string.IsNullOrEmpty(traits)) traits = "无";

            newItem.slotTrait.text = traits.Trim();

            // 保底把 isPicked 设 true（通常 AddItemToInventory 已设）
            level1Data.isPicked = true;
        }

        newItem.slotSynopsis.text = item.itemInfo;

        // 记录 UI 状态（用于去重与后续删除）
        instance.inventoryItems.Add(newItem.gameObject);
        instance.displayedItemIds.Add(item.itemNum);

        // 将项放在格子顶部（你已有的布局方法）
        instance.gridLayout.AddItemToTop(newItem.gameObject);
    }*/
    public static void CreateNewItem(Item item)
    {
        Slot2 newItem = Instantiate(instance.slotPrefab, instance.slotGrid.transform.position, Quaternion.identity);

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
        newItem.slotName.text = item.itemName;
        instance.gridLayout.AddItemToTop(newItem.gameObject);
    }
}
