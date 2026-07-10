using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MapRoomNavigator : MonoBehaviour
{
    [Header("地图 Content")]
    public RectTransform mapContent;

    // 注释掉原本由尺寸计算边界的逻辑，改为手动控制
    // public Vector2 mapSize = new Vector2(4227, 2480);
    // public Vector2 viewportSize = new Vector2(3200, 1800);

    [Header("手动边界限制 (像素)")]
    [Tooltip("targetX 的最大值。值越大，地图能往左滑得越多（露出更多右侧藏在 UI 后的内容）")]
    public float rightMoveX = 2000f;
    public float leftMoveX = 0f;
    [Tooltip("targetY 的最大值。值越大，地图能往上滑得越多（露出更多下侧藏在 UI 后的内容）")]
    public float upMoveY = 1000f;
    public float downMoveY = 0f;

    [Header("初始位置 (像素)")]
    public Vector2 startPosition = Vector2.zero;

    [Header("平滑移动时长 (秒)")]
    public float moveDuration = 0.5f;

    [Header("按钮映射 (此时的 Target Position 变为 相对移动距离)")]
    public List<ButtonMoveMapping> buttonMappings = new List<ButtonMoveMapping>();

    public bool hasRestoredPosition = false;

    private Coroutine moveCoroutine;

    void Start()
    {
        // 只有没有恢复过位置，才走默认初始位置（绝对坐标）
        if (!hasRestoredPosition)
        {
            MoveToPosition(startPosition, immediate: true);
        }

        // 注册所有按钮
        foreach (var mapping in buttonMappings)
        {
            Vector2 delta = mapping.targetPosition; // 相对位移
            mapping.button.onClick.AddListener(() =>
            {
                MoveRelative(delta);
            });
        }
    }

    /// <summary>
    /// 沿轴移动一段相对距离
    /// </summary>
    public void MoveRelative(Vector2 deltaPosition, bool immediate = false)
    {
        // Debug.Log("MoveRelative");
        // 1. 获取当前地图对应的绝对坐标位置
        Vector2 currentPos = GetCurrentTargetPosition();

        // 2. 计算叠加相对位移后的目标绝对坐标
        Vector2 targetPos = currentPos + deltaPosition;

        // 3. 调用带手动边界限制的绝对移动
        MoveToPosition(targetPos, immediate);
    }

    /// <summary>
    /// 移动到绝对坐标位置（带手动边界 Clamp 裁剪）
    /// </summary>
    public void MoveToPosition(Vector2 pixelPosition, bool immediate = false)
    {
        // --- 核心改动：使用 Inspector 手动配置的 maxMoveX 和 maxMoveY 进行卡位 ---
        // 最小值为 0（代表地图左上角对齐视口左上角）
        float targetX = Mathf.Clamp(pixelPosition.x, leftMoveX, rightMoveX);
        float targetY = Mathf.Clamp(pixelPosition.y, downMoveY, upMoveY);
        // -----------------------------------------------------------------

        Vector2 anchoredPos = new Vector2(-targetX, -targetY);

        if (immediate)
        {
            mapContent.anchoredPosition = anchoredPos;
        }
        else
        {
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            moveCoroutine = StartCoroutine(SmoothMove(anchoredPos, moveDuration));
        }
    }

    private IEnumerator SmoothMove(Vector2 targetPos, float duration)
    {
        Vector2 startPos = mapContent.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            mapContent.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        mapContent.anchoredPosition = targetPos;
    }

    public Vector2 GetCurrentTargetPosition()
    {
        // anchoredPosition 是取反的，所以要再取反回来
        return new Vector2(-mapContent.anchoredPosition.x, -mapContent.anchoredPosition.y);
    }
}

[Serializable]
public class ButtonMoveMapping
{
    public Button button;
    [Tooltip("相对移动的距离，例如 (100, 0) 代表向右移100像素")]
    public Vector2 targetPosition;
}