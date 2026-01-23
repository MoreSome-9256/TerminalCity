using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Level1SourceType
{
    AutoExpanded,   // 来自二级资料自动展开
    Manual          // 玩家手动拖入
}

public class Window2 : MonoBehaviour
{
    [Header("Data")]
    public Item windowItem;
    public Window parent; // 若由二级展开，则记录父Window（Level2）

    public Level1SourceType sourceType;

    // 相对父节点偏移量
    public Vector2 offset;

    [Header("UI")]
    public Image background;
    //public Image icon;
    public TMP_Text titleText;
    public Button closeButton;

    [Header("Style")]
    public Sprite manualSprite;     // 手动加入的一级资料样式
    public Sprite autoSprite;       // 自动展开的一级资料样式
    public Sprite proxySprite; // 可选：代理节点样式

    public bool isProxy = false;
    public Window2 original = null; // 指向第一个出现的窗口

    /// <summary>
    /// 初始化窗口
    /// </summary>
    public void Init(Item item, Level1SourceType type, Window parentWindow = null)
    {
        windowItem = item;
        sourceType = type;
        parent = parentWindow;

        // 基础显示
        //if (icon != null && item != null)
        //    icon.sprite = item.itemImage;

        if (titleText != null && item != null)
            titleText.text = item.itemName;

        ApplyStyle();
        SetupCloseButton();
    }

    public void ApplyStyle()
    {
        if (background == null) return;

        if (isProxy && proxySprite != null)
        {
            background.sprite = proxySprite;
            SetAlpha(0.6f);
            return;
        }

        if (sourceType == Level1SourceType.AutoExpanded)
        {
            if (autoSprite != null)
                background.sprite = autoSprite;
            SetAlpha(0.75f);
        }
        else
        {
            if (manualSprite != null)
                background.sprite = manualSprite;
            SetAlpha(1f);
        }
    }

    void SetupCloseButton()
    {
        if (closeButton == null) return;

        closeButton.onClick.RemoveAllListeners();

        if (sourceType == Level1SourceType.AutoExpanded)
        {
            // 自动展开的一级资料不可关闭
            closeButton.gameObject.SetActive(false);
        }
        else
        {
            closeButton.gameObject.SetActive(true);
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    void OnCloseClicked()
    {
        // 只有 Manual 才会走到这里
        if (sourceType != Level1SourceType.Manual)
            return;

        if (SynthesisManager.Instance != null)
        {
            SynthesisManager.Instance.UnregisterWindow2(this);
        }

        Destroy(gameObject);
    }

    void SetAlpha(float a)
    {
        if (background != null)
        {
            var c = background.color;
            c.a = a;
            background.color = c;
        }

        /*if (icon != null)
        {
            var c = icon.color;
            c.a = a;
            icon.color = c;
        }*/

        if (titleText != null)
        {
            var c = titleText.color;
            c.a = a;
            titleText.color = c;
        }
    }
}
