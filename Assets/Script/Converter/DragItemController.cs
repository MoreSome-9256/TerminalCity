using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class DragItemController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject windowPrefab; // Window预制体
    private GameObject currentWindow;
    private RectTransform dropArea; // 指定放置区域

    private void Start()
    {
        //dropArea = GameObject.Find("WindowArea").GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DeselectAllWindows();

        // 获取物品数据
        Slot2 originalSlot = GetComponent<Slot2>();
        if (originalSlot == null || originalSlot.slotItem == null) return;

        // 检查重复性（关键修改）
        if (IsItemAlreadyInWindows(originalSlot.slotItem))
        {
            // 立即终止拖拽流程
            currentWindow = null; 
            eventData.pointerDrag = null; // 阻止后续拖拽事件
            Debug.Log($"已存在 [{originalSlot.slotItem.itemName}] 的窗口");
            return;
        }

        // 初始化拖拽区域
        if (dropArea == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            dropArea = canvas.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(rt => rt.name == "WindowArea");
        }

        // 创建新窗口
        CreateNewWindow(originalSlot, eventData.position);
    }

    // 检查物品是否已存在窗口
    private bool IsItemAlreadyInWindows(Item targetItem)
    {
        return FindObjectsOfType<Window>()
            .Any(window => window.windowItem == targetItem);
    }

    private void CreateNewWindow(Slot2 originalSlot, Vector2 position)
    {
        currentWindow = Instantiate(windowPrefab, dropArea);
        currentWindow.transform.position = position;

        // 初始化窗口组件
        Window windowComponent = currentWindow.GetComponent<Window>();
        windowComponent.windowItem = originalSlot.slotItem;
        windowComponent.windowImage.sprite = originalSlot.slotImage.sprite;
        windowComponent.windowName.text = originalSlot.slotName.text;

        // 确保关闭按钮功能
        SetupCloseButton(currentWindow);

        // 确保新窗口可拖动
        AddWindowDragComponent(currentWindow);
    }

    private void SetupCloseButton(GameObject window)
    {
        // 获取关闭按钮组件
        WindowCloseButton closeComponent = window.GetComponent<WindowCloseButton>();

        // 安全检查
        if (closeComponent == null)
        {
            Debug.LogError("Window prefab is missing WindowCloseButton component!", window);
            return;
        }

        // 如果按钮需要特殊初始化可以在这里添加
    }
    private void AddWindowDragComponent(GameObject window)
    {
        var dragComp = window.AddComponent<WindowDragController>();
        dragComp.Init(dropArea); // 初始化拖拽区域限制
    }
    void DeselectAllWindows()
    {
        foreach (Window w in FindObjectsOfType<Window>())
        {
            w.DeselectWindow();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentWindow == null) return;

        RectTransform windowTransform = currentWindow.GetComponent<RectTransform>();
        windowTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentWindow == null) return;

        // 检查是否在指定区域
        if (RectTransformUtility.RectangleContainsScreenPoint(dropArea, eventData.position))
        {
            // 这里可以添加放置后的处理逻辑
        }
        else
        {
            Destroy(currentWindow);
        }
    }
}