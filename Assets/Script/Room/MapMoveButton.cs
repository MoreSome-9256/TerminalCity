using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // 引入 UnityEvent

public class MapMoveButton : MonoBehaviour
{
    public enum MoveAxis
    {
        XAxis,
        YAxis
    }

    [Header("移动轴向设置")]
    [Tooltip("选择本次平移控制的是 X 轴还是 Y 轴，未选中的轴将严格保持当前值不变")]
    [SerializeField] private MoveAxis moveAxis = MoveAxis.XAxis;

    [Header("唯一记忆 Key")]
    [Tooltip("为当前走廊/地图起一个唯一名字（例如 Corridor_4F），用于记录上次离开时的位置")]
    [SerializeField] private string mapMemoryKey = "Corridor_Default";

    [Header("移动目标")]
    [Tooltip("拖入要平移的走廊/地图 Content（RectTransform）")]
    [SerializeField] private RectTransform mapContent;

    [Header("坐标设置")]
    [Tooltip("目标 X 绝对坐标（像素，例如 1100）")]
    [SerializeField] private float targetXPosition = 1100f;
    [Tooltip("目标 Y 绝对坐标（像素，向下平移填正值，代码会自动转为 -Y）")]
    [SerializeField] private float targetYPosition = 800f;

    [Header("平滑平移参数")]
    [SerializeField] private float moveDuration = 0.5f;

    [Header("边界限制 (像素)")]
    [SerializeField] private float leftMoveX = 0f;
    [SerializeField] private float rightMoveX = 3000f;
    [SerializeField] private float bottomMoveY = 0f;
    [SerializeField] private float topMoveY = 3000f;

    [Header("移动结束触发的事件列表")]
    public UnityEvent onMoveEnd; // ✨ 新增：移动彻底停止后的事件回调

    // 静态字典：记录每个走廊最后停靠的 anchoredPosition
    private static readonly Dictionary<string, Vector2> savedPositions = new Dictionary<string, Vector2>();

    private static Coroutine activeMoveCoroutine;

    private void OnEnable()
    {
        StartCoroutine(DeferredRestorePosition());
    }

    private IEnumerator DeferredRestorePosition()
    {
        yield return new WaitForEndOfFrame();
        RestoreSavedPosition();
    }

    /// <summary>
    /// 恢复上次离开时的位置
    /// </summary>
    public void RestoreSavedPosition()
    {
        if (mapContent == null || string.IsNullOrEmpty(mapMemoryKey)) return;

        if (savedPositions.TryGetValue(mapMemoryKey, out Vector2 lastPos))
        {
            mapContent.anchoredPosition = lastPos;
        }
    }

    /// <summary>
    /// 公开平移方法
    /// </summary>
    public void ExecuteMove()
    {
        if (mapContent == null)
        {
            Debug.LogError($"[{gameObject.name}] MapMoveButton 未绑定 mapContent！");
            return;
        }

        Vector2 finalAnchoredPos;

        if (moveAxis == MoveAxis.XAxis)
        {
            // 1. 仅对 X 轴裁剪，Y 轴保持不动
            float clampedX = Mathf.Clamp(targetXPosition, leftMoveX, rightMoveX);
            finalAnchoredPos = new Vector2(-clampedX, mapContent.anchoredPosition.y);
        }
        else
        {
            // 2. 仅对 Y 轴裁剪，X 轴保持不动（向下平移采用 -clampedY）
            float clampedY = Mathf.Clamp(targetYPosition, bottomMoveY, topMoveY);
            finalAnchoredPos = new Vector2(mapContent.anchoredPosition.x, -clampedY);
        }

        SaveCurrentPosition(finalAnchoredPos);

        if (activeMoveCoroutine != null)
        {
            StopCoroutine(activeMoveCoroutine);
        }
        activeMoveCoroutine = StartCoroutine(SmoothMoveRoutine(finalAnchoredPos, moveDuration));
    }

    private void SaveCurrentPosition(Vector2 pos)
    {
        if (!string.IsNullOrEmpty(mapMemoryKey))
        {
            savedPositions[mapMemoryKey] = pos;
        }
    }

    private IEnumerator SmoothMoveRoutine(Vector2 targetPos, float duration)
    {
        Vector2 startPos = mapContent.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (moveAxis == MoveAxis.XAxis)
            {
                mapContent.anchoredPosition = new Vector2(
                    Mathf.Lerp(startPos.x, targetPos.x, t),
                    targetPos.y
                );
            }
            else
            {
                mapContent.anchoredPosition = new Vector2(
                    startPos.x,
                    Mathf.Lerp(startPos.y, targetPos.y, t)
                );
            }

            yield return null;
        }

        mapContent.anchoredPosition = targetPos;
        SaveCurrentPosition(targetPos);
        activeMoveCoroutine = null;

        // ✨ 触发移动结束事件
        onMoveEnd?.Invoke();
    }

    public static void ClearSavedPosition(string memoryKey)
    {
        if (savedPositions.ContainsKey(memoryKey))
        {
            savedPositions.Remove(memoryKey);
        }
    }
}