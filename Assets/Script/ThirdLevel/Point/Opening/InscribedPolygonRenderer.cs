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

    public int Sides
    {
        get => sides;
        set
        {
            sides = Mathf.Max(3, value);
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        // 1. 绘制外接圆线框（固定细分64段，作为完美的“圆”基准）
        DrawRing(vh, radius, lineWidth, 64, circleColor);

        // 2. 绘制内接多边形线框
        DrawRing(vh, radius, lineWidth, sides, polygonColor);
    }

    private void DrawRing(VertexHelper vh, float r, float width, int segments, Color color)
    {
        float innerR = r - width * 0.5f;
        float outerR = r + width * 0.5f;
        float angleStep = 360f / segments;

        int baseIndex = vh.currentVertCount;

        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angleStep - 90f); // -90让顶点从顶部开始对称
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector3 innerPos = new Vector3(cos * innerR, sin * innerR, 0);
            Vector3 outerPos = new Vector3(cos * outerR, sin * outerR, 0);

            vh.AddVert(innerPos, color, Vector2.zero);
            vh.AddVert(outerPos, color, Vector2.zero);

            if (i > 0)
            {
                int curr = baseIndex + i * 2;
                int prev = curr - 2;

                vh.AddTriangle(prev, prev + 1, curr);
                vh.AddTriangle(prev + 1, curr + 1, curr);
            }
        }
    }
}