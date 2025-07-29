using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MapRoomNavigator : MonoBehaviour
{
    [Header("地图 Content")]
    public RectTransform mapContent;

    [Header("地图总尺寸 (像素)")]
    public Vector2 mapSize = new Vector2(4227, 2480);

    [Header("Viewport 尺寸 (像素)")]
    public Vector2 viewportSize = new Vector2(3200, 1800);

    [Header("初始位置 (像素)")]
    public Vector2 startPosition = Vector2.zero;

    [Header("平滑移动时长 (秒)")]
    public float moveDuration = 0.5f;

    [Header("按钮映射")]
    public List<ButtonMoveMapping> buttonMappings = new List<ButtonMoveMapping>();

    private Coroutine moveCoroutine;

    void Start()
    {
        // 注册所有按钮
        foreach (var mapping in buttonMappings)
        {
            Vector2 pos = mapping.targetPosition;
            mapping.button.onClick.AddListener(() =>
            {
                MoveToPosition(pos);
            });
        }

        MoveToPosition(startPosition, immediate: true);
    }

    public void MoveToPosition(Vector2 pixelPosition, bool immediate = false)
    {
        // Clamp 移动范围（允许显示空白）
        float maxX = Mathf.Max(0, mapSize.x - viewportSize.x);
        float maxY = Mathf.Max(0, mapSize.y - viewportSize.y);

        float targetX = Mathf.Clamp(pixelPosition.x, -viewportSize.x, mapSize.x);
        float targetY = Mathf.Clamp(pixelPosition.y, -viewportSize.y, mapSize.y);

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
}

[Serializable]
public class ButtonMoveMapping
{
    public Button button;
    public Vector2 targetPosition;
}
