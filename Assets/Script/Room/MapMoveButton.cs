using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMoveButton : MonoBehaviour
{
    [Header("唯一记忆 Key")]
    [Tooltip("为当前走廊/地图起一个唯一名字（例如 Corridor_4F），用于记录上次离开时的位置")]
    [SerializeField] private string mapMemoryKey = "Corridor_Default";

    [Header("移动目标")]
    [Tooltip("拖入要平移的走廊/地图 Content（RectTransform）")]
    [SerializeField] private RectTransform mapContent; 

    [Header("坐标设置")]
    [Tooltip("目标 X 绝对坐标（像素，例如 1100）")]
    [SerializeField] private float targetXPosition = 1100f; 

    [Header("平滑平移参数")]
    [SerializeField] private float moveDuration = 0.5f;

    [Header("X 轴边界限制 (像素)")]
    [SerializeField] private float leftMoveX = 0f;
    [SerializeField] private float rightMoveX = 3000f;

    // 静态字典：记录每个走廊最后停靠的 anchoredPosition
    private static readonly Dictionary<string, Vector2> savedPositions = new Dictionary<string, Vector2>();

    private static Coroutine activeMoveCoroutine;

    private void OnEnable()
    {
        // 不要直接调用，而是开启协程等当前帧的初始化逻辑全跑完
        StartCoroutine(DeferredRestorePosition());
    }

    private IEnumerator DeferredRestorePosition()
    {
        // 等待当前帧所有脚本的 Start / Update / RoomManager 初始化全部执行完毕
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
            // 瞬间恢复到上次离开时的绝对位置
            mapContent.anchoredPosition = lastPos;
        }
    }

    /// <summary>
    /// 公开平移方法：挂进 StateGateInteractable 的 OnSuccessInteract 即可
    /// </summary>
    public void ExecuteMove()
    {
        if (mapContent == null)
        {
            Debug.LogError($"[{gameObject.name}] MapMoveButton 未绑定 mapContent！");
            return;
        }

        // 1. 仅对 X 轴进行边界限制裁剪
        float clampedX = Mathf.Clamp(targetXPosition, leftMoveX, rightMoveX);

        // 2. X 轴转为 UGUI 的负值绝对坐标；Y 轴直接保持当前的 anchoredPosition.y 绝不修改！
        Vector2 finalAnchoredPos = new Vector2(-clampedX, mapContent.anchoredPosition.y);

        // 3. 记录到静态字典中
        SaveCurrentPosition(finalAnchoredPos);

        // 4. 启动平移
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

            // X 平滑过渡，Y 始终锁定为目标高度
            mapContent.anchoredPosition = new Vector2(
                Mathf.Lerp(startPos.x, targetPos.x, t),
                targetPos.y
            );
            yield return null;
        }

        mapContent.anchoredPosition = targetPos;
        SaveCurrentPosition(targetPos); // 确保移动完全停止时存入最终位置
        activeMoveCoroutine = null;
    }

    /// <summary>
    /// 提供外部手动重置坐标记忆的方法（例如重新开始关卡/新周目时使用）
    /// </summary>
    public static void ClearSavedPosition(string memoryKey)
    {
        if (savedPositions.ContainsKey(memoryKey))
        {
            savedPositions.Remove(memoryKey);
        }
    }
}