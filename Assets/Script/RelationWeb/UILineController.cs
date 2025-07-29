using UnityEngine;
using System.Collections;

public class UILineController : MonoBehaviour
{
    public RectTransform lineRect;
    private RectTransform startNode;
    private RectTransform endNode;
    private Coroutine currentAnimation;

    private void Update()
    {
        // 非动画状态下持续更新线条位置
        if (currentAnimation == null && startNode != null && endNode != null)
        {
            UpdateLinePosition();
        }
    }

    public void SetNodes(RectTransform start, RectTransform end, bool animate = true)
    {
        startNode = start;
        endNode = end;

        if (animate)
        {
            AnimateLineFromStartToEnd(startNode.position, endNode.position, 0.3f);
        }
        else
        {
            UpdateLinePosition();
        }
    }

    public void AnimateLineFromStartToEnd(Vector3 worldStart, Vector3 worldEnd, float duration = 0.4f)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(AnimateLineFromStartToEndCoroutine(worldStart, worldEnd, duration));
    }

    private IEnumerator AnimateLineFromStartToEndCoroutine(Vector3 worldStart, Vector3 worldEnd, float duration)
    {
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        Vector2 localStart = parentRect.InverseTransformPoint(worldStart);
        Vector2 localEnd = parentRect.InverseTransformPoint(worldEnd);
        Vector2 direction = localEnd - localStart;
        float finalLength = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 初始设置：线条从起点开始延伸
        lineRect.pivot = new Vector2(0f, 0.5f); // 左侧为基准点
        lineRect.anchoredPosition = localStart; // 定位到起点
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
        lineRect.sizeDelta = new Vector2(0, lineRect.sizeDelta.y); // 初始长度为0

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float length = Mathf.Lerp(0, finalLength, t);
            lineRect.sizeDelta = new Vector2(length, lineRect.sizeDelta.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 动画完成后切换到中点定位模式
        lineRect.sizeDelta = new Vector2(finalLength, lineRect.sizeDelta.y);
        SwitchToMidpointPosition(localStart, localEnd, angle);
        currentAnimation = null;
    }

    private void SwitchToMidpointPosition(Vector2 localStart, Vector2 localEnd, float angle)
    {
        // 切换到中点定位
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = (localStart + localEnd) / 2;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void UpdateLinePosition()
    {
        if (startNode == null || endNode == null) return;

        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        Vector2 localStart = parentRect.InverseTransformPoint(startNode.position);
        Vector2 localEnd = parentRect.InverseTransformPoint(endNode.position);
        Vector2 direction = localEnd - localStart;
        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 持续跟踪时使用中点定位
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = (localStart + localEnd) / 2;
        lineRect.sizeDelta = new Vector2(length, lineRect.sizeDelta.y);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
    }
}