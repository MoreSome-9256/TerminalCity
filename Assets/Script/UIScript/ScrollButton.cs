using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buttonImage;
    public Image background;
    public float transitionDuration = 0.1f;
    private Coroutine colorTransition;

    private Color initialColor;
    private Color hover = new Color(1, 1, 1, 1);

    public ScrollRect scrollRect;
    public float scrollSpeed = 0.1f; // 滚动速度（值越大越快）
    private bool isHovering; // 是否悬停中
    public bool isLeft;
    private Coroutine scrollCoroutine;

    void Start()
    {
        initialColor = buttonImage.color;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartColorTransition(hover);
        isHovering = true;
        // 停止已有协程，避免重复运行
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
        scrollCoroutine = StartCoroutine(SmoothScroll());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartColorTransition(initialColor);
        isHovering = false;
        // 停止滚动协程
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
    }
    private void StartColorTransition(Color targetColor)
    {
        if (colorTransition != null)
            StopCoroutine(colorTransition);

        colorTransition = StartCoroutine(ColorTransition(targetColor));
    }

    private IEnumerator ColorTransition(Color target)
    {
        float elapsedTime = 0f;
        Color startColor1 = buttonImage.color;
        Color startColor2 = background.color;

        while (elapsedTime < transitionDuration)
        {
            buttonImage.color = Color.Lerp(
                startColor1,
                target,
                elapsedTime / transitionDuration
            );
            background.color = Color.Lerp(
                startColor2,
                target,
                elapsedTime / transitionDuration
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        buttonImage.color = target;
        background.color = target;
    }
    IEnumerator SmoothScroll()
    {
        Debug.Log("smooth scroll");
        while (isHovering)
        {
            if (isLeft)
            {
                scrollRect.horizontalNormalizedPosition -= scrollSpeed * Time.deltaTime;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition += scrollSpeed * Time.deltaTime;
            }
            // 限制范围在 0~1
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
            yield return null;
        }
    }
}
