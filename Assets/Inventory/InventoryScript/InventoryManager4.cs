using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager4 : MonoBehaviour
{
    static InventoryManager4 instance;

    public DoorController doorController;
    private DoorController activeDoor;
    public Inventory myBag;
    public GameObject slotGrid;
    public Slot slotPrefab;
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
    public static void CreateNewItem(Item item, bool canOpen = false)
    {
        Slot newItem = Instantiate(
            instance.slotPrefab,
            instance.slotGrid.transform.position,
            Quaternion.identity
        );

        newItem.transform.SetParent(instance.slotGrid.transform, false);

        newItem.slotItem = item;
        newItem.canOpen = canOpen;

        Transform iconTransform = newItem.transform.Find("Icon");
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
    public void SetActiveDoor(DoorController door)
    {
        activeDoor = door;
    }
    public void OnToolItemClicked(int itemNum)
    {
        if (activeDoor != null)
        {
            activeDoor.TryUseItem(itemNum);
        }
        else
        {
            Debug.LogWarning("没有正在解锁的门！");
        }
    }
}
