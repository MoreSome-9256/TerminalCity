using UnityEngine;
using UnityEngine.UI;

public class UIConnectionLine : MonoBehaviour
{
    [Header("组件引用")]
    public RectTransform horizontalLine;
    public RectTransform angledLine;

    [Header("配置")]
    public float fixedArmLength = 100f;  // 水平臂长度
    public float thickness = 3f;         // 粗细

    private RectTransform startNode;
    private RectTransform endNode;
    private RectTransform myRect;

    // 记录这条线在起点的垂直偏移量 (解决线重叠问题)
    private float startOffsetY = 0f;

    void Awake()
    {
        myRect = GetComponent<RectTransform>();
    }

    public void Init(RectTransform start, RectTransform end, float offsetY = 0f)
    {
        startNode = start;
        endNode = end;
        startOffsetY = offsetY;

        if (horizontalLine) horizontalLine.sizeDelta = new Vector2(0, thickness);
        if (angledLine) angledLine.sizeDelta = new Vector2(0, thickness);

        UpdateLine();
    }

    void LateUpdate()
    {
        if (startNode == null || endNode == null)
        {
            Destroy(gameObject);
            return;
        }
        UpdateLine();
    }

    void UpdateLine()
    {
        // 1. 获取基础坐标 (父容器局部坐标)
        Vector3 startLocal = GetLocalPos(startNode.position);
        Vector3 endLocal = GetLocalPos(endNode.position);

        // 2. 应用起点的垂直偏移 (排线效果)
        startLocal.y += startOffsetY;

        // 3. 判断方向 (1为向右, -1为向左)
        bool isRight = endLocal.x > startLocal.x;
        int direction = isRight ? 1 : -1;
        // ====================================================
        // 起点修正：仅当起点不是中心节点时才使用边缘
        // ====================================================
        if (startNode != SynthesisManager.Instance.centerPoint)
        {
            float parentHalfWidth = startNode.rect.width / 2f;
            float parentAestheticOffset = 30f;
            startLocal.x += direction * (parentHalfWidth - parentAestheticOffset);
        }

        // ====================================================
        // 【核心修改】计算子节点边缘位置
        // ====================================================
        // 获取子节点的半宽
        // 注意：这里假设子节点的 Pivot 是中心点 (0.5, 0.5)
        float childHalfWidth = endNode.rect.width / 2f;

        // 如果想让线稍微插入一点点防止出现缝隙，可以减去一个微小值(如 2f)，否则直接用 childHalfWidth
        float aestheticOffset = 20f;

        // 调整终点 X 坐标
        // 如果在右边(dir=1)，终点要向左(-1)偏移半宽
        // 如果在左边(dir=-1)，终点要向右(1)偏移半宽
        endLocal.x -= direction * (childHalfWidth - aestheticOffset);

        // ====================================================
        // 4. 计算拐点
        // ====================================================

        // 计算新的水平距离 (基于修正后的边缘终点)
        float distX = Mathf.Abs(endLocal.x - startLocal.x);

        // 确保水平臂不要超过总距离 (留一点空间给斜线，乘以 0.8 或减去一个固定值)
        float actualArmLength = Mathf.Min(fixedArmLength, distX - 150f);
        // 如果距离极近，防止反向
        if (actualArmLength < 0) actualArmLength = 0;

        float elbowX = startLocal.x + (actualArmLength * direction);

        // 拐点位置
        Vector3 elbowPos = new Vector3(elbowX, startLocal.y, 0);

        // 5. 绘制线段
        UpdateSegment(horizontalLine, startLocal, elbowPos);
        UpdateSegment(angledLine, elbowPos, endLocal);
    }

    void UpdateSegment(RectTransform lineRect, Vector3 p1, Vector3 p2)
    {
        if (lineRect == null) return;
        lineRect.localPosition = p1;
        Vector3 diff = p2 - p1;
        float distance = diff.magnitude;
        lineRect.sizeDelta = new Vector2(distance, thickness);
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    }

    Vector3 GetLocalPos(Vector3 worldPos)
    {
        Vector2 localPoint;
        // 注意：如果是 Overlay 模式，cam 参数传 null；如果是 Camera 模式，传 UI Camera
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            myRect,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out localPoint
        );
        return localPoint;
    }
    public void SetStartOffset(float y)
    {
        startOffsetY = y;
    }
}
