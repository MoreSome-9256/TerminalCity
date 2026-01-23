using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class DragItemController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject windowPrefab; // Window预制体
    private GameObject currentWindow;
    public RectTransform dropArea; // 指定放置区域

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
        DetermineDropArea();

        // 创建新窗口
        CreateNewWindow(originalSlot, eventData.position);
    }
    private void DetermineDropArea()
    {
        // 如果 Inspector 已经指定了 dropArea，则优先使用
        if (dropArea != null) return;

        if (SynthesizerUIManager.Instance != null)
        {
            switch (SynthesizerUIManager.Instance.currentMode)
            {
                case SynthesizerUIManager.SynthesisMode.Level2:
                    dropArea = SynthesizerUIManager.Instance.windowsRootLevel2;
                    break;
                case SynthesizerUIManager.SynthesisMode.Level3:
                    dropArea = SynthesizerUIManager.Instance.windowsRootLevel3;
                    break;
            }
        }

        // fallback 老逻辑
        if (dropArea == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            dropArea = canvas.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(rt => rt.name == "WindowArea");
        }
    }
    private RectTransform GetTargetParent()
    {
        if (SynthesizerUIManager.Instance != null)
        {
            switch (SynthesizerUIManager.Instance.currentMode)
            {
                case SynthesizerUIManager.SynthesisMode.Level2:
                    return SynthesizerUIManager.Instance.windowsRootLevel2;
                case SynthesizerUIManager.SynthesisMode.Level3:
                    return SynthesizerUIManager.Instance.windowsRootLevel3;
            }
        }

        // fallback
        return dropArea;
    }


    // 检查物品是否已存在窗口
    private bool IsItemAlreadyInWindows(Item targetItem)
    {
        return FindObjectsOfType<Window>()
            .Any(window => window.windowItem == targetItem);
    }

    private void CreateNewWindow(Slot2 originalSlot, Vector2 position)
    {
        if (originalSlot == null || originalSlot.slotItem == null) return;

        GameObject prefabToUse;
        RectTransform targetParent = dropArea;

        // 1️⃣ 根据当前界面选择 prefab 和父对象
        if (SynthesizerUIManager.Instance != null)
        {
            switch (SynthesizerUIManager.Instance.currentMode)
            {
                case SynthesizerUIManager.SynthesisMode.Level2:
                    targetParent = SynthesizerUIManager.Instance.windowsRootLevel2;

                    // 在二级合成界面，所有窗口都用 Window prefab
                    prefabToUse = windowPrefab;
                    break;

                case SynthesizerUIManager.SynthesisMode.Level3:
                    targetParent = SynthesizerUIManager.Instance.windowsRootLevel3;

                    // 在三级合成界面，区分一级/二级
                    if (originalSlot.slotItem is Level2Data)
                        prefabToUse = windowPrefab;
                    else // Level1Data
                        prefabToUse = SynthesisManager.Instance.window2Prefab;
                    break;

                default:
                    prefabToUse = windowPrefab;
                    break;
            }
        }
        else
        {
            prefabToUse = windowPrefab; // fallback
        }

        // 2️⃣ 创建窗口
        currentWindow = Instantiate(prefabToUse, targetParent);
        currentWindow.transform.position = position;

        // 3️⃣ 初始化窗口组件
        if (originalSlot.slotItem is Level2Data)
        {
            Window windowComponent = currentWindow.GetComponent<Window>();
            windowComponent.windowItem = originalSlot.slotItem;
            windowComponent.windowImage.sprite = originalSlot.slotImage.sprite;
            windowComponent.windowName.text = originalSlot.slotName.text;

            // 注册 SynthesisManager
            if (SynthesisManager.Instance != null)
                SynthesisManager.Instance.RegisterLevel2(windowComponent);
        }
        else if (originalSlot.slotItem is Level1Data)
        {
            // 区分界面：Level2界面 → Window，Level3界面 → Window2
            if (SynthesizerUIManager.Instance.currentMode == SynthesizerUIManager.SynthesisMode.Level2)
            {
                // 二级合成界面用 Window prefab
                Window windowComponent = currentWindow.GetComponent<Window>();
                windowComponent.windowItem = originalSlot.slotItem;
                windowComponent.windowImage.sprite = originalSlot.slotImage.sprite;
                windowComponent.windowName.text = originalSlot.slotName.text;

                if (SynthesisManager.Instance != null)
                    SynthesisManager.Instance.RegisterLevel2(windowComponent);
            }
            else
            {
                // 三级合成界面用 Window2 prefab
                Window2 window2Component = currentWindow.GetComponent<Window2>();
                window2Component.Init((Level1Data)originalSlot.slotItem, Level1SourceType.Manual, null);

                if (SynthesisManager.Instance != null)
                    SynthesisManager.Instance.RegisterLevel1(window2Component);
            }
        }

        // 4️⃣ 设置关闭按钮和拖拽
        SetupCloseButton(currentWindow);
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