using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class SynthesisManager : MonoBehaviour
{
    public static SynthesisManager Instance;

    [Header("Runtime State")]
    public string currentEvent = null;   // 当前锁定的事件
    public TMP_Text centerEventText;
    private HashSet<Item> occupied = new HashSet<Item>();

    private List<Window> level2Windows = new List<Window>();
    private List<Window2> level1Windows = new List<Window2>();

    // 【新增】记录 窗口 -> 连线 的映射，方便我们在 Update 里修改线的偏移
    private Dictionary<RectTransform, UIConnectionLine> centerConnections
    = new Dictionary<RectTransform, UIConnectionLine>();


    [Header("Prefabs & Root")]
    public Transform graphRoot;
    public GameObject window2Prefab;   // 一级资料窗口（Window2）

    public RectTransform windowsRoot; // 拖入三级合成面板下的 WindowsRoot

    [Header("Line System")]
    public GameObject linePrefab;     // 拖入新的 UIConnection Prefab
    public RectTransform centerPoint;
    public Transform linesContainer;  // 必须赋值！建议在 GraphRoot 下建一个全屏的 Panel
    public float linePortSpacing = 10f; // 线在父节点出口处的垂直间距

    [Header("Ghost Line")]
    public GameObject ghostLinePrefab;   // 拖入新的虚线曲线 prefab


    void Awake()
    {
        Instance = this;
    }
    void LateUpdate()
    {
        // 左右两侧都重建一次
        RebuildCenterLinesOnSide(true);
        RebuildCenterLinesOnSide(false);
    }

    // =========================
    // 判定：是否允许拖入
    // =========================
    /*public bool CanAccept(Item item)
    {
        // 已经被占用（例如被二级展开）
        if (occupied.Contains(item))
            return false;

        // 如果这个 item 是 Level1Data，并且它已经属于某个被拖入的二级资料
        if (item is Level1Data l1)
        {
            // 检查它是否已经被拖入过
            bool alreadyDragged = level1Windows.Any(w2 => w2.windowItem == l1 && !w2.isProxy);
            if (alreadyDragged)
                return false;

            // 检查它是否属于已拖入的二级资料
            bool partOfDraggedLevel2 = level2Windows
                .Any(w2 => w2.windowItem is Level2Data l2 && l2.requiredLevel1.Contains(l1));

            if (partOfDraggedLevel2)
                return false;
        }
        // 尚未锁定事件：任何物品都可以作为起点
        if (string.IsNullOrEmpty(currentEvent))
            return true;

        // 已锁定事件：
        // - 允许同事件
        // - 允许无事件
        string itemEvent = GetItemEvent(item);

        if (string.IsNullOrEmpty(itemEvent))
            return true;

        if (itemEvent == currentEvent)
            return true;

        return false;
    }*/

    // =========================
    // 注册二级窗口（Window）
    // =========================
    public void RegisterLevel2(Window w)
    {
        if (w == null || w.windowItem == null) return;

        level2Windows.Add(w);
        occupied.Add(w.windowItem);

        string itemEvent = GetItemEvent(w.windowItem);

        // 若这是第一个含事件的资料，则锁定事件
        if (string.IsNullOrEmpty(currentEvent) && !string.IsNullOrEmpty(itemEvent))
        {
            currentEvent = itemEvent;
            UpdateCenterEventUI();
        }

        // 若是二级资料，自动展开一级资料
        if (w.windowItem is Level2Data l2)
        {
            ExpandLevel2(w, l2);
        }
        // 创建中央连线
        CreateCenterLine(w.GetComponent<RectTransform>());
    }

    // =========================
    // 注册一级窗口（Window2）
    // =========================
    public void RegisterLevel1(Window2 w2)
    {
        if (w2 == null || w2.windowItem == null) return;

        level1Windows.Add(w2);
        if (!w2.isProxy) occupied.Add(w2.windowItem);
        string itemEvent = GetItemEvent(w2.windowItem);

        if (string.IsNullOrEmpty(currentEvent) && !string.IsNullOrEmpty(itemEvent))
        {
            currentEvent = itemEvent;
            UpdateCenterEventUI();
        }

        // =========================
        // 关键逻辑：只有 Manual 的 Window2 才与中央节点连线
        // =========================
        if (w2.sourceType == Level1SourceType.Manual)
        {
            CreateCenterLine(w2.GetComponent<RectTransform>());
        }

    }

    // =========================
    // 注销二级窗口（Window）
    // =========================
    public void UnregisterLevel2(Window w)
    {
        if (w == null) return;

        level2Windows.Remove(w);
        occupied.Remove(w.windowItem);

        // 若是二级资料，回收其自动展开的一级资料
        if (w.windowItem is Level2Data)
        {
            var children = level1Windows
                .Where(x => x.parent == w && x.sourceType == Level1SourceType.AutoExpanded)
                .ToList();

            foreach (var c in children)
            {
                level1Windows.Remove(c);
                occupied.Remove(c.windowItem);
                Destroy(c.gameObject);
            }
        }

        ReevaluateEventLock();
    }

    // =========================
    // 注销一级窗口（Window2）
    // =========================
    public void UnregisterWindow2(Window2 w2)
    {
        if (w2 == null) return;

        level1Windows.Remove(w2);
        occupied.Remove(w2.windowItem);

        ReevaluateEventLock();
    }

    // =========================
    // 二级展开逻辑
    // =========================
    void ExpandLevel2(Window parent, Level2Data data)
    {
        if (data.requiredLevel1 == null || data.requiredLevel1.Count == 0) return;

        for (int i = 0; i < data.requiredLevel1.Count; i++)
        {
            Level1Data l1 = data.requiredLevel1[i];

            bool alreadyExists = occupied.Contains(l1);

            GameObject go = Instantiate(window2Prefab, graphRoot);
            Window2 w2 = go.GetComponent<Window2>();
            w2.Init(l1, Level1SourceType.AutoExpanded, parent);

            // === 新增：代理逻辑 ===
            if (alreadyExists)
            {
                w2.isProxy = true;
                w2.original = FindFirstWindow2Of(l1);
                w2.ApplyStyle();
            }
            else
            {
                occupied.Add(l1);
            }

            parent.children.Add(w2);
            RegisterLevel1(w2);

            float centerOffset = (data.requiredLevel1.Count - 1) / 2f;
            float lineOffsetY = (centerOffset - i) * linePortSpacing;
            CreateLine(parent.GetComponent<RectTransform>(), w2.GetComponent<RectTransform>(), lineOffsetY);

            // 若是代理节点，创建“虚连接”
            if (w2.isProxy && w2.original != null)
            {
                CreateGhostLine(
                    w2.original.GetComponent<RectTransform>(),
                    w2.GetComponent<RectTransform>()
                );
            }
        }

        parent.UpdateChildrenLayout();
    }

    Window2 FindFirstWindow2Of(Level1Data data)
    {
        return level1Windows.FirstOrDefault(w => w.windowItem == data && !w.isProxy);
    }
    void CreateGhostLine(RectTransform from, RectTransform to)
    {
        if (ghostLinePrefab == null || from == null || to == null) return;

        Transform container = linesContainer != null ? linesContainer : graphRoot;
        GameObject lineObj = Instantiate(ghostLinePrefab, container);
        lineObj.transform.localScale = Vector3.one;
        lineObj.transform.localPosition = Vector3.zero;

        // 【新增】关键修复：将虚线放到层级最上方（即渲染在最底层/背景）
        // 如果没有这一句，线会盖在节点上，导致节点无法点击
        lineObj.transform.SetAsFirstSibling();

        var lineScript = lineObj.GetComponent<UIGhostCurveLine>();
        if (lineScript != null)
        {
            lineScript.Init(from, to);
        }
    }

    // =========================
    // 重新评估事件锁定
    // =========================
    void ReevaluateEventLock()
    {
        string newEvent = null;

        foreach (var w in level2Windows)
        {
            string e = GetItemEvent(w.windowItem);
            if (!string.IsNullOrEmpty(e))
            {
                newEvent = e;
                break;
            }
        }

        if (newEvent == null)
        {
            foreach (var w in level1Windows)
            {
                string e = GetItemEvent(w.windowItem);
                if (!string.IsNullOrEmpty(e))
                {
                    newEvent = e;
                    break;
                }
            }
        }

        currentEvent = newEvent;
        UpdateCenterEventUI();
    }

    // =========================
    // 工具：统一获取事件
    // =========================
    string GetItemEvent(Item item)
    {
        if (item is Level1Data l1)
            return l1.Event;
        if (item is Level2Data l2)
            return l2.Event;
        return null;
    }
    void CreateLine(RectTransform start, RectTransform end, float startOffsetY = 0f)
    {
        if (linePrefab == null || start == null || end == null) return;

        Transform container = linesContainer != null ? linesContainer : graphRoot;
        GameObject lineObj = Instantiate(linePrefab, container);
        lineObj.transform.localScale = Vector3.one;
        lineObj.transform.SetAsFirstSibling();

        UIConnectionLine lineScript = lineObj.GetComponent<UIConnectionLine>();
        if (lineScript != null)
        {
            // 传入偏移量
            lineScript.Init(start, end, startOffsetY);
        }
    }
    void CreateCenterLine(RectTransform target)
    {
        bool isRight = target.transform.localPosition.x > centerPoint.transform.localPosition.x;

        Transform container = linesContainer != null ? linesContainer : graphRoot;
        GameObject lineObj = Instantiate(linePrefab, container);
        lineObj.transform.localScale = Vector3.one;
        lineObj.transform.SetAsFirstSibling();

        var line = lineObj.GetComponent<UIConnectionLine>();
        if (line == null) return;

        centerConnections[target] = line;

        RebuildCenterLinesOnSide(isRight);
    }

    void RebuildCenterLinesOnSide(bool isRight)
    {
        var sameSide = centerConnections
    .Where(kv =>
    {
        var rt = kv.Key;
        return rt != null &&
               (rt.transform.localPosition.x > centerPoint.transform.localPosition.x) == isRight;
    })
    .Select(kv => kv.Key)
    .OrderByDescending(rt => rt.transform.localPosition.y) // 上到下
    .ToList();

        int count = sameSide.Count;
        if (count == 0) return;

        float centerOffset = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            var target = sameSide[i];
            float offsetY = (centerOffset - i) * linePortSpacing;

            var line = centerConnections[target];
            line.Init(centerPoint, target, offsetY);
        }
    }
    void UpdateCenterEventUI()
    {
        if (centerEventText == null) return;

        if (string.IsNullOrEmpty(currentEvent))
            centerEventText.text = ""; // 或者 "—"
        else
            centerEventText.text = currentEvent;
    }

}
