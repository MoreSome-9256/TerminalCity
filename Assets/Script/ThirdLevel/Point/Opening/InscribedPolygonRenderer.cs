using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class InscribedPolygonRenderer : MaskableGraphic
{
    [Header("图形属性")]
    [SerializeField] private float radius = 200f;
    [SerializeField] private float lineWidth = 3f;
    [SerializeField] private int sides = 4;
    [SerializeField] private Color circleColor = new Color(1f, 1f, 1f, 0.25f); // 外圆微弱透光
    [SerializeField] private Color polygonColor = Color.white;                 // 内接多边形高亮

    [Header("绘制进度")]
    [Range(0f, 1f)]
    [SerializeField] private float drawProgress = 1f; // 0 为未画，1 为画满

    public int Sides
    {
        get => sides;
        set
        {
            sides = Mathf.Max(3, value);
            SetVerticesDirty();
        }
    }

    public float DrawProgress
    {
        get => drawProgress;
        set
        {
            drawProgress = Mathf.Clamp01(value);
            SetVerticesDirty(); // 触发网格重新生成
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (drawProgress <= 0.0001f) return;

        // 1. 绘制外接圆线框（根据 drawProgress 顺时针逐渐勾勒出圆环）
        DrawRing(vh, radius, lineWidth, 64, circleColor, drawProgress);

        // 2. 绘制内接多边形线框（同样按进度勾勒，开场画完圆时再呈现）
        DrawRing(vh, radius, lineWidth, sides, polygonColor, drawProgress);
    }

    private void DrawRing(VertexHelper vh, float r, float width, int segments, Color color, float progress)
    {
        float innerR = r - width * 0.5f;
        float outerR = r + width * 0.5f;

        // 当前需要绘制的总角度范围（0° 到 360° * progress）
        float maxAngle = 360f * progress;
        float angleStep = 360f / segments;

        int baseIndex = vh.currentVertCount;
        int vertexCount = 0;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = i * angleStep;
            bool isLast = false;

            // 超出当前绘制进度时，截断在终点角度并结束
            if (currentAngle >= maxAngle)
            {
                currentAngle = maxAngle;
                isLast = true;
            }

            float rad = Mathf.Deg2Rad * (currentAngle - 90f); // -90° 让顶点从正上方12点钟开始
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector3 innerPos = new Vector3(cos * innerR, sin * innerR, 0);
            Vector3 outerPos = new Vector3(cos * outerR, sin * outerR, 0);

            vh.AddVert(innerPos, color, Vector2.zero);
            vh.AddVert(outerPos, color, Vector2.zero);
            vertexCount++;

            if (vertexCount > 1)
            {
                int curr = baseIndex + (vertexCount - 1) * 2;
                int prev = curr - 2;

                vh.AddTriangle(prev, prev + 1, curr);
                vh.AddTriangle(prev + 1, curr + 1, curr);
            }

            if (isLast) break;
        }
    }
}