using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Window : MonoBehaviour, IPointerClickHandler
{
    // 基础组件
    public Item windowItem;
    public Image windowImage;
    public TMP_Text windowName;
    public Image background;

    // 状态相关
    public Sprite selectedSprite;
    public Sprite deselectedSprite;
    private static Window currentlySelected;

    // 三级资料合成时才用到
    [HideInInspector] public List<Window2> children = new List<Window2>();
    [HideInInspector] public bool IsLeftSide => transform.localPosition.x < 0;
    [Header("布局设置")]
    public float horizontalDist = 500f; // 父子之间的水平固定距离
    public float verticalSpacing = 500f; // 子节点之间的垂直间距

    private bool isDirty = false; // 标记是否需要更新布局
    private bool enableSelection = true;


    public int ItemNum => windowItem != null ? windowItem.itemNum : 0;

    // 数据展示组件（通过代码获取或拖拽赋值）
    [SerializeField] private InformationDecode dataDisplay;
    public static Window CurrentSelected
    {
        get => currentlySelected != null &&
               currentlySelected.gameObject.activeInHierarchy ?
               currentlySelected : null;
        private set
        {
            // 确保旧窗口取消选中
            if (currentlySelected != null && currentlySelected != value)
            {
                currentlySelected.SetSelectionVisual(false);
            }
            currentlySelected = value;
        }
    }

    public static Level1Data CurrentSelectedData  // 保留数据访问器
    {
        get
        {
            if (CurrentSelected?.windowItem is Level1Data)
            {
                return CurrentSelected.windowItem as Level1Data;
            }
            return null;
        }
    }
    void Start()
    {
        InitializeWindow();
        // 在三级合成模式下，禁用选中效果
        if (SynthesizerUIManager.Instance != null &&
            SynthesizerUIManager.Instance.currentMode == SynthesizerUIManager.SynthesisMode.Level3)
        {
            enableSelection = false;

            // 强制使用“未选中”外观
            if (background != null && deselectedSprite != null)
                background.sprite = deselectedSprite;

            return; // 不再自动选中
        }
        AutoSelectOnCreation();
    }
    private void InitializeWindow()
    {
        if (windowItem != null)
        {
            windowName.text = windowItem.itemName;
        }
    }

    private void AutoSelectOnCreation()
    {
        if (!enableSelection) return;

        // 强制取消前一个窗口的选中状态
        if (CurrentSelected != null)
        {
            CurrentSelected.SetSelectionVisual(false);
        }

        // 设置新窗口为选中
        SetSelectionVisual(true);
        CurrentSelected = this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!enableSelection) return;

        // 直接通过属性设置，自动触发旧窗口取消选中
        CurrentSelected = this;
        SetSelectionVisual(true);
    }
    private void SetSelectionVisual(bool isSelected)
    {
        if (!enableSelection) return;

        background.sprite = isSelected ? selectedSprite : deselectedSprite;
    }

    // 新增数据访问方法
    public Level1Data GetLevel1Data()
    {
        return windowItem is Level1Data ? (Level1Data)windowItem : null;
    }

    public void DeselectWindow()
    {
        if (!enableSelection) return;

        background.sprite = deselectedSprite;
    }
    /// <summary>
    /// Unity 生命周期函数：每一帧的最后执行
    /// 解决“刚拖拽出来时不跟随”的问题
    /// </summary>
    void LateUpdate()
    {
        // 只要有子节点，每一帧都强制检查位置
        // 这样做性能消耗很小（只是几个向量加法），但能保证绝对的视觉同步
        // 无论是由 DragController 控制，还是由上层 Inventory 系统控制位置，都能生效
        if (children.Count > 0)
        {
            UpdateChildrenLayout();
        }
    }
    /// <summary>
    /// 【核心方法】强制更新所有子节点的位置
    /// 这个方法会被 DragController 每帧调用
    /// </summary>
    public void UpdateChildrenLayout()
    {
        if (children.Count == 0) return;

        // 1. 获取父节点当前的局部坐标
        Vector3 myPos = transform.localPosition;

        // 2. 实时判断：父节点现在是在中心点的左侧还是右侧？
        // 这样拖拽过中线时，子节点会自动“甩”到另一边
        bool isCurrentLeft = myPos.x < 0;

        // 3. 计算起始 Y 坐标 (垂直居中)
        // 比如有3个子节点，总高度跨度是 2 * spacing
        // StartY 应该是 Top 位置
        float totalSpan = (children.Count - 1) * verticalSpacing;
        float startY = myPos.y + (totalSpan / 2f);

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (child == null) continue;

            // 4. 计算目标位置
            float offsetX = isCurrentLeft ? -horizontalDist : horizontalDist;
            float targetY = startY - (i * verticalSpacing);

            Vector3 targetPos = new Vector3(
                myPos.x + offsetX,
                targetY,
                child.transform.localPosition.z
            );

            // 5. 直接赋值
            child.transform.localPosition = targetPos;
        }
    }
}