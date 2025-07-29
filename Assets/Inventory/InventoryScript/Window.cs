using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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
        // 直接通过属性设置，自动触发旧窗口取消选中
        CurrentSelected = this;
        SetSelectionVisual(true);
    }
    private void SetSelectionVisual(bool isSelected)
    {
        background.sprite = isSelected ? selectedSprite : deselectedSprite;
    }

    // 新增数据访问方法
    public Level1Data GetLevel1Data()
    {
        return windowItem is Level1Data ? (Level1Data)windowItem : null;
    }

    public void DeselectWindow()
    {
        background.sprite = deselectedSprite;
    }
}