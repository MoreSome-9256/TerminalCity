using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

// 主背包-二级资料部分
public class InventoryManager3 : MonoBehaviour
{
    static InventoryManager3 instance;

    public Inventory myBag;
    public GameObject slotGrid;
    public Slot slotPrefab;
    [Header("UI References")]
    [SerializeField] private ReverseGridLayout gridLayout; // 自定义布局组件
    public List<GameObject> inventoryItems;

    // 新增：记录已显示的 itemNum
    private HashSet<int> displayedItemIds = new HashSet<int>();

    public ScrollRect scrollRect;
    public GameObject textDisplayPanel;
    public TMP_Text contentText;
    public Button closeButton;
    [SerializeField] private RectTransform contentRect;

    [System.Serializable]
    public class ItemDialogPair
    {
        public int itemID;
        public UnityEvent dialogEvent;
    }

    //[Header("Dialogue Trigger After Viewing Items")]
    //[SerializeField] private List<ItemDialogPair> itemDialogMappings = new List<ItemDialogPair>();

    private Dictionary<int, UnityEvent> itemDialogMap = new Dictionary<int, UnityEvent>();
    private UnityEvent pendingDialogEvent = null;

    [SerializeField] private DialogTriggerAfterInventory dialogTriggerAfterInventory;
    [SerializeField] private SideScreenMove bagController;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;
        if (scrollRect != null && contentRect != null)
        {
            scrollRect.content = contentRect;
        }

        // 将 List 映射成 Dictionary，方便查找
        /*foreach (var pair in itemDialogMappings)
        {
            if (!itemDialogMap.ContainsKey(pair.itemID))
            {
                itemDialogMap[pair.itemID] = pair.dialogEvent;
            }
        }*/
    }
    public static void CreateNewItem(Item item)
    {
        Slot newItem = Instantiate(instance.slotPrefab, instance.slotGrid.transform.position, Quaternion.identity);

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

        newItem.slotSynopsis.text = item.itemInfo;
        instance.gridLayout.AddItemToTop(newItem.gameObject);
        // 记录
        instance.displayedItemIds.Add(item.itemNum);
        //instance.inventoryItems.Add(newItem.gameObject);
    }
    public void OnItemClicked(GameObject itemGameObject)
    {
        //Debug.Log("onItemClicked");
        Slot slot = itemGameObject.GetComponent<Slot>();
        if (slot != null && slot.slotItem != null && slot.slotItem.textFile != null)
        {
            slot.slotItem.readTime++;
            textDisplayPanel.SetActive(true);
            contentText.text = slot.slotItem.textFile.text;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentText.rectTransform);
            float newHeight = contentText.preferredHeight;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, newHeight);

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }

            /*int itemID = slot.slotItem.itemNum;
            if (itemDialogMap.TryGetValue(itemID, out UnityEvent dialogEvent))
            {
                pendingDialogEvent = dialogEvent;
            }
            else
            {
                pendingDialogEvent = null;
            }*/
            dialogTriggerAfterInventory.RequestTriggerByItemID(slot.slotItem.itemNum, bagController);

        }
    }
    void Start()
    {
        RebuildFromBag();
    }

    void OnEnable()
    {
        //RebuildFromBag();
    }

    private void RebuildFromBag()
    {
        if (myBag == null) return;

        foreach (var item in myBag.level2List)
        {
            if (item == null) continue;

            if (!displayedItemIds.Contains(item.itemNum))
            {
                CreateNewItem(item);
                displayedItemIds.Add(item.itemNum);
            }
        }
    }

}
