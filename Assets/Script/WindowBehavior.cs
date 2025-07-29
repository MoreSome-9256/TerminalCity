using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class WindowBehavior : MonoBehaviour
{
    public GameObject floatScreen;
    public RectTransform[] rectTransforms; 
    public float animationDuration = 0.2f;
    public float delayBetweenImages = 0.0f;
    public float waitTime = 0f;

    private bool isAnimating = false;
    private Vector2[] originalSizes;
    private Coroutine currentAnimationCoroutine = null;

    [Header("Events")]
    public UnityEvent onMoveEnd;

    private void Start()
    {
        originalSizes = new Vector2[rectTransforms.Length];
        for (int i = 0; i < rectTransforms.Length; i++)
        {
            originalSizes[i] = rectTransforms[i].sizeDelta;

            // 如果是TMP组件，禁用自动调整
            TMP_Text tmpText = rectTransforms[i].GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.autoSizeTextContainer = false;
            }

            rectTransforms[i].sizeDelta = new Vector2(rectTransforms[i].sizeDelta.x, 0);
        }
    }

    public void Move(bool isForward)
    {
        if (isForward)
        {
            floatScreen.SetActive(true);
            if (!isAnimating)
            {
                if (currentAnimationCoroutine != null)
                {
                    StopCoroutine(currentAnimationCoroutine);
                }
                currentAnimationCoroutine = StartCoroutine(AnimateRectTransforms(true));
            }
        }
        else
        {
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
            }
            currentAnimationCoroutine = StartCoroutine(AnimateRectTransforms(false));
        }
    }

    private IEnumerator AnimateRectTransforms(bool isForward)
    {
        yield return new WaitForSeconds(waitTime);
        isAnimating = true;

        List<Coroutine> coroutines = new List<Coroutine>();

        foreach (RectTransform rt in rectTransforms)
        {
            coroutines.Add(StartCoroutine(AnimateSingleRectTransform(rt, isForward)));
        }

        foreach (Coroutine c in coroutines)
        {
            yield return c;
        }

        if (!isForward)
        {
            floatScreen.SetActive(false);
        }
        isAnimating = false;
    }

    private IEnumerator AnimateSingleRectTransform(RectTransform rt, bool isForward)
    {
        int index = System.Array.IndexOf(rectTransforms, rt);
        Vector2 originalSize = originalSizes[index];

        Vector2 startSize = rt.sizeDelta;
        Vector2 targetSize = isForward ?
            new Vector2(startSize.x, originalSize.y) :
            new Vector2(startSize.x, 0);

        float elapsedTime = 0;

        // 检查是否为TMP组件
        TMP_Text tmpText = rt.GetComponent<TMP_Text>();
        bool isTMP = tmpText != null;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            float currentHeight = Mathf.Lerp(startSize.y, targetSize.y, t);

            // 如果是TMP组件，调整其rectTransform和文本区域
            if (isTMP)
            {
                rt.sizeDelta = new Vector2(startSize.x, currentHeight);
                tmpText.rectTransform.sizeDelta = new Vector2(startSize.x, currentHeight);
                tmpText.autoSizeTextContainer = false; // 关闭自动调整
            }
            else
            {
                rt.sizeDelta = new Vector2(startSize.x, currentHeight);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 设置最终尺寸
        rt.sizeDelta = targetSize;
        if (isTMP)
        {
            tmpText.rectTransform.sizeDelta = targetSize;
        }
        if (isForward)
        {
            onMoveEnd.Invoke();
        }
        
    }
}