using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
public class UIGhostCurveLine : MaskableGraphic
{
    public RectTransform start;
    public RectTransform end;

    [Header("曲线设置")]
    public float curveOffset = 150f;
    public float dashLength = 10f;
    public float gapLength = 6f;
    public int segments = 30;

    public void Init(RectTransform from, RectTransform to)
    {
        start = from;
        end = to;

        // 【新增】双重保险：强制关闭射线检测
        // 确保这根线绝对不会阻挡鼠标点击
        this.raycastTarget = false;

        SetAllDirty();
    }

    void LateUpdate()
    {
        if (start != null && end != null)
            SetAllDirty();  // 确保曲线跟随窗口移动
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (start == null || end == null) return;

        // 1. 转局部坐标
        Vector3 p0Local = transform.InverseTransformPoint(start.position);
        Vector3 p2Local = transform.InverseTransformPoint(end.position);

        // =========================================================
        // 【核心修改】直接取【上方正中】
        // =========================================================

        // 计算起点顶部 (考虑 Pivot)
        // 如果 Pivot 是中心(0.5)，这就加一半高度
        float startTopY = start.rect.height * (1f - start.pivot.y);
        Vector3 p0 = p0Local + new Vector3(0, startTopY, 0);

        // 计算终点顶部
        float endTopY = end.rect.height * (1f - end.pivot.y);
        Vector3 p2 = p2Local + new Vector3(0, endTopY, 0);

        // =========================================================
        // 计算曲线控制点
        // =========================================================
        Vector3 mid = (p0 + p2) * 0.5f;

        // 【强制向上拱起】
        // 既然连接的是顶部，曲线一定要向上弯，像彩虹一样
        // 直接用 Vector3.up，不再计算复杂的法线
        Vector3 p1 = mid + Vector3.up * Mathf.Abs(curveOffset);

        // =========================================================
        // 生成贝塞尔曲线点 (保持不变)
        // =========================================================
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pt = Mathf.Pow(1 - t, 2) * p0 +
                         2 * (1 - t) * t * p1 +
                         Mathf.Pow(t, 2) * p2;
            points.Add(pt);
        }

        // =========================================================
        // 生成虚线网格 (保持不变)
        // =========================================================
        float accumulated = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];
            float dist = Vector3.Distance(a, b);
            Vector3 dirVec = (b - a).normalized;

            while (accumulated < dist)
            {
                float segmentLen = Mathf.Min(dashLength, dist - accumulated);
                Vector3 segStart = a + dirVec * accumulated;
                Vector3 segEnd = segStart + dirVec * segmentLen;

                AddLineSegment(vh, segStart, segEnd, 3f); // 线宽

                accumulated += segmentLen + gapLength;
            }
            accumulated -= dist;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        this.raycastTarget = false;   // **关键**
    }

    void AddLineSegment(VertexHelper vh, Vector3 start, Vector3 end, float width)
    {
        Vector3 dir = (end - start).normalized;
        Vector3 normal = new Vector3(-dir.y, dir.x, 0) * width * 0.5f;

        Vector3 v0 = start - normal;
        Vector3 v1 = start + normal;
        Vector3 v2 = end + normal;
        Vector3 v3 = end - normal;

        int idx = vh.currentVertCount;
        vh.AddVert(v0, color, Vector2.zero);
        vh.AddVert(v1, color, Vector2.zero);
        vh.AddVert(v2, color, Vector2.zero);
        vh.AddVert(v3, color, Vector2.zero);

        vh.AddTriangle(idx, idx + 1, idx + 2);
        vh.AddTriangle(idx, idx + 2, idx + 3);
    }
}
