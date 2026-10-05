using System.Collections;
using UnityEngine;

public class MapMoveButton : MonoBehaviour
{
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

    private static Coroutine activeMoveCoroutine;

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

        // 3. 启动平移
        if (activeMoveCoroutine != null)
        {
            StopCoroutine(activeMoveCoroutine);
        }
        activeMoveCoroutine = StartCoroutine(SmoothMoveRoutine(finalAnchoredPos, moveDuration));
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
        activeMoveCoroutine = null;
    }
}