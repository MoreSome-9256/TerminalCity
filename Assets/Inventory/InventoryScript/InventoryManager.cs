using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System;

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
    [Header("Text Display")]
    [SerializeField] private GameObject scrollViewText;
    [SerializeField] private TMP_Text contentText;

    [Header("Image Display")]
    [SerializeField] private GameObject scrollViewImage;
    [SerializeField] private RectTransform imageContent;
    [SerializeField] private Image imagePrefab;

    [SerializeField] private Button closeButton;
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
        if (slot == null || slot.slotItem == null) return;

        slot.slotItem.readTime++;
        textDisplayPanel.SetActive(true);

        // --- 文本逻辑 ---
        if (slot.slotItem.textFile != null)
        {
            scrollViewText.SetActive(true);
            scrollViewImage.SetActive(false);

            string text = slot.slotItem.textFile.text;

            foreach (var entry in DictionaryManager.Instance.entries.Values)
            {
                if (!entry.hasTerm) continue;

                text = ReplaceWithLink(text, entry.term, entry.term);

                if (entry.aliases != null)
                {
                    foreach (var alias in entry.aliases)
                    {
                        text = ReplaceWithLink(text, alias, entry.term);
                    }
                }
            }

            contentText.text = text;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentText.rectTransform);
            float newHeight = contentText.preferredHeight;
            contentText.rectTransform.sizeDelta = new Vector2(contentText.rectTransform.sizeDelta.x, newHeight);

            var sr = scrollViewText.GetComponent<ScrollRect>();
            sr.verticalNormalizedPosition = 1f;
        }
        // --- 图片逻辑 ---
        else if (slot.slotItem.itemImages != null && slot.slotItem.itemImages.Count > 0)
        {
            scrollViewText.SetActive(false);
            scrollViewImage.SetActive(true);

            // 清空旧的图片
            foreach (Transform child in imageContent)
            {
                Destroy(child.gameObject);
            }

            // 生成新图片
            foreach (var sprite in slot.slotItem.itemImages)
            {
                Debug.Log("it's an image");
                if (sprite == null) continue;

                Image img = Instantiate(imagePrefab, imageContent);
                img.sprite = sprite;
                img.preserveAspect = true;
            }

            var sr = scrollViewImage.GetComponent<ScrollRect>();
            sr.verticalNormalizedPosition = 1f;
        }

        // --- 对话逻辑 ---
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

    private string ReplaceWithLink(string text, string keyword, string linkID = null)
    {
        if (string.IsNullOrEmpty(keyword)) return text;

        string escaped = System.Text.RegularExpressions.Regex.Escape(keyword);
        string id = string.IsNullOrEmpty(linkID) ? keyword : linkID;

        // 忽略大小写
        return System.Text.RegularExpressions.Regex.Replace(text, escaped, m =>
        {
            // 如果已经在 <link> 内部，就跳过
            int index = m.Index;
            if (index > 0 && text.Substring(Mathf.Max(0, index - 7), 7).Contains("<link="))
                return m.Value;

            return $"<link=\"{id}\"><u><color=#BBDAFF>{m.Value}</color></u></link>";
        }, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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
