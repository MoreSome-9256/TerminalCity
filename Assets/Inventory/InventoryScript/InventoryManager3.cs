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
    [Header("Text Display")]
    [SerializeField] private GameObject scrollViewText;
    [SerializeField] private TMP_Text contentText;

    [Header("Image Display")]
    [SerializeField] private GameObject scrollViewImage;
    [SerializeField] private RectTransform imageContent;
    [SerializeField] private Image imagePrefab;
    public Button closeButton;
    [SerializeField] private RectTransform contentRect;

    [Header("Chat System Reference")]
    public ChatManager chatManager;

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
        Slot slot = itemGameObject.GetComponent<Slot>();
        if (slot == null || slot.slotItem == null) return;

        slot.slotItem.readTime++; // 统一增加阅读次数

        // --- 新增：聊天功能判断 ---
        // 尝试将物品转换为Level2Data以访问type属性
        Level2Data level2Item = slot.slotItem as Level2Data;

        // 如果是聊天类型 (type 1)，并且有ChatManager，则进入此分支
        if (level2Item != null && level2Item.type == 1 && chatManager != null && level2Item.textFile != null)
        {
            textDisplayPanel.SetActive(false); // 确保旧的显示面板是关闭的
            chatManager.StartConversation(level2Item.textFile); // 把任务交给ChatManager

            // 可以在这里保留你旧的对话事件逻辑，如果需要的话
            // int itemID = slot.slotItem.itemNum; ...

            return; // **重要**：处理完聊天逻辑后，直接退出方法，不再执行下面的旧逻辑
        }

        // --- 原有逻辑 (仅调整了一行代码的位置) ---

        // 如果代码执行到这里，说明它不是一个聊天物品，可以安全地打开常规显示面板
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
            float chaos = PlayerChaos.Instance.chaos;
            string corrupedText = ChaosTextProcessor.ApplyChaosMixed(text, chaos);
            contentText.text = corrupedText;
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
            foreach (Transform child in imageContent)
                Destroy(child.gameObject);

            Canvas.ForceUpdateCanvases();
            float parentWidth = ((RectTransform)imageContent).rect.width;
            foreach (var sprite in slot.slotItem.itemImages)
            {
                Image img = Instantiate(imagePrefab, imageContent);
                img.sprite = sprite;
                img.preserveAspect = false;
                RectTransform rt = img.GetComponent<RectTransform>();
                float aspect = sprite.rect.height / sprite.rect.width;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentWidth * aspect);
                LayoutElement le = img.GetComponent<LayoutElement>();
                if (le == null) le = img.gameObject.AddComponent<LayoutElement>();
                le.flexibleWidth = 0;
                le.flexibleHeight = 0;
                le.preferredWidth = parentWidth;
                le.preferredHeight = parentWidth * aspect;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(imageContent);
            var sr = scrollViewImage.GetComponent<ScrollRect>();
            sr.verticalNormalizedPosition = 1f;
        }

        // --- 对话逻辑 (你原有的UnityEvent系统) ---
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
    private void DisableTextDisplayRaycast()
    {
        var graphics = textDisplayPanel.GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
        {
            g.raycastTarget = false;
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
