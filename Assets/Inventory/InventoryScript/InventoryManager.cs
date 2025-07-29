using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public Inventory myBag;
    public GameObject slotGrid;
    public Slot slotPrefab;

    [Header("UI References")]
    [SerializeField] private ReverseGridLayout gridLayout;

    public List<GameObject> inventoryItems;
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

    [Header("Dialogue Trigger After Viewing Items")]
    [SerializeField] private List<ItemDialogPair> itemDialogMappings = new List<ItemDialogPair>();

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
        foreach (var pair in itemDialogMappings)
        {
            if (!itemDialogMap.ContainsKey(pair.itemID))
            {
                itemDialogMap[pair.itemID] = pair.dialogEvent;
            }
        }
    }

    public static void CreateNewItem(Item item)
    {
        Slot newItem = Instantiate(instance.slotPrefab, instance.slotGrid.transform.position, Quaternion.identity);
        newItem.transform.SetParent(instance.slotGrid.transform, false);

        newItem.slotItem = item;

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

        newItem.slotName.text = item.itemName;

        if (item is Level1Data level1Data)
        {
            string traits = "";
            if (!string.IsNullOrEmpty(level1Data.Name)) traits += "人物 ";
            if (!string.IsNullOrEmpty(level1Data.Time)) traits += "时间 ";
            if (!string.IsNullOrEmpty(level1Data.Event)) traits += "事件 ";
            if (string.IsNullOrEmpty(traits)) traits = "无";

            newItem.slotTrait.text = traits.Trim();
        }

        newItem.slotSynopsis.text = item.itemInfo;
        instance.gridLayout.AddItemToTop(newItem.gameObject);
    }

    public void OnItemClicked(GameObject itemGameObject)
    {
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

            int itemID = slot.slotItem.itemNum;
            if (itemDialogMap.TryGetValue(itemID, out UnityEvent dialogEvent))
            {
                pendingDialogEvent = dialogEvent;
            }
            else
            {
                pendingDialogEvent = null;
            }
        }
    }

    public void OnInventoryClosed()
    {
        if (pendingDialogEvent != null)
        {
            dialogTriggerAfterInventory.RequestTriggerAfterInventoryClosed(bagController, pendingDialogEvent);
            pendingDialogEvent = null;
        }
    }
}
