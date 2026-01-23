using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ConverterController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject floatScreen;
    public Image[] imagesToAnimate; // UI中的Image组件数组
    public float animationDuration = 0.2f; // 每个Image动画的持续时间
    public float delayBetweenImages = 0.0f; // 每个Image动画之间的延迟

    private bool isAnimating = false; // 防止动画重复触发的标志
    private Vector2[] originalSizes; // 存储每个Image的原始大小
    private Coroutine currentAnimationCoroutine = null; // 存储当前运行的动画协程

    public Button openButton;

    private void Start()
    {
        // 初始化时存储每个Image的原始大小
        originalSizes = new Vector2[imagesToAnimate.Length];
        for (int i = 0; i < imagesToAnimate.Length; i++)
        {
            originalSizes[i] = imagesToAnimate[i].rectTransform.sizeDelta;
            imagesToAnimate[i].rectTransform.sizeDelta = new Vector2(imagesToAnimate[i].rectTransform.sizeDelta.x, 0);
        }
        if (openButton != null && SynthesizerUIManager.Instance != null)
        {
            SynthesizerUIManager.Instance.RegisterSynthButton(openButton);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        floatScreen.SetActive(true);
        if (!isAnimating)
        {
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
            }
            currentAnimationCoroutine = StartCoroutine(AnimateUIImages(true));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AnimateUIImages(false));
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // 遍历全局 MoveController，执行打开动作
        /*foreach (var mc in SynthesizerUIManager.Instance.moveControllers)
        {
            mc.StartMove(true); // 移入
        }*/

        SynthesizerUIManager.Instance.Open();
    }
    private IEnumerator AnimateUIImages(bool isForward)
    {
        isAnimating = true;

        int startIndex = isForward ? 0 : imagesToAnimate.Length - 1;
        int endIndex = isForward ? imagesToAnimate.Length - 1 : 0;
        int step = isForward ? 1 : -1;

        for (int i = startIndex; isForward ? i <= endIndex : i >= endIndex; i += step)
        {
            Image image = imagesToAnimate[i];
            Vector2 startSize = image.rectTransform.sizeDelta;
            Vector2 targetSize = isForward ? new Vector2(startSize.x, originalSizes[i].y) : new Vector2(startSize.x, 0);
            float elapsedTime = 0;

            while (elapsedTime < animationDuration)
            {
                float t = elapsedTime / animationDuration;
                image.rectTransform.sizeDelta = new Vector2(startSize.x, Mathf.Lerp(startSize.y, targetSize.y, t));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            image.rectTransform.sizeDelta = targetSize;

            if (i != endIndex)
            {
                yield return new WaitForSeconds(delayBetweenImages);
            }
        }

        isAnimating = false;

        // 如果动画是反向的（消失），则在动画完成后关闭floatScreen
        if (!isForward)
        {
            floatScreen.SetActive(false);
        }
    }
}