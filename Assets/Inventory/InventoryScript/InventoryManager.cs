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

    // 用来记录当前 UI 已显示的 itemId，防止重复创建 UI
    private HashSet<int> displayedItemIds = new HashSet<int>();

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
    // 检查物品是否已经在 UI 中显示
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
    public static void CreateNewItem(Item item)
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

            // 清空旧图片
            foreach (Transform child in imageContent)
                Destroy(child.gameObject);

            // 强制刷新布局
            Canvas.ForceUpdateCanvases();
            float parentWidth = ((RectTransform)imageContent).rect.width;

            foreach (var sprite in slot.slotItem.itemImages)
            {
                Image img = Instantiate(imagePrefab, imageContent);
                img.sprite = sprite;

                // 关闭 preserveAspect，手动控制尺寸
                img.preserveAspect = false;

                RectTransform rt = img.GetComponent<RectTransform>();
                float aspect = sprite.rect.height / sprite.rect.width;

                // 设置宽度填满父容器，高度按比例
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentWidth * aspect);

                // 添加 LayoutElement，禁用 Flexible Width/Height，防止 LayoutGroup 覆盖
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
