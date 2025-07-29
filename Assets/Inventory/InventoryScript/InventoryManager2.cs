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

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;
    }
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
