using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Linq;

public class WindowCloseButton : MonoBehaviour
{
    [Header("Settings")]
    public float closeAnimationDuration = 0.2f;

    [Header("References")]
    public Button closeButton;
    public RectTransform[] elementsToAnimate; // 需要包含所有要动画的RectTransform

    private Vector2[] originalSizes;
    private bool isClosing = false;

    private void Awake()
    {
        // 自动获取所有子元素的RectTransform（排除自己）
        if (elementsToAnimate == null || elementsToAnimate.Length == 0)
        {
            elementsToAnimate = GetComponentsInChildren<RectTransform>()
                .Where(rt => rt != transform)
                .ToArray();
        }

        // 存储原始尺寸
        originalSizes = new Vector2[elementsToAnimate.Length];
        for (int i = 0; i < elementsToAnimate.Length; i++)
        {
            if (elementsToAnimate[i] != null)
            {
                originalSizes[i] = elementsToAnimate[i].sizeDelta;
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(StartCloseProcess);
        }
    }

    private void StartCloseProcess()
    {
        if (!isClosing)
        {
            StartCoroutine(CloseAnimation());
        }
    }

    private IEnumerator CloseAnimation()
    {
        isClosing = true;
        closeButton.interactable = false;

        float elapsedTime = 0f;

        while (elapsedTime < closeAnimationDuration)
        {
            float t = elapsedTime / closeAnimationDuration;

            // 同时处理所有元素的尺寸
            for (int i = 0; i < elementsToAnimate.Length; i++)
            {
                if (elementsToAnimate[i] != null)
                {
                    float newHeight = Mathf.Lerp(originalSizes[i].y, 0f, t);
                    elementsToAnimate[i].sizeDelta = new Vector2(
                        elementsToAnimate[i].sizeDelta.x,
                        newHeight
                    );
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终尺寸正确
        foreach (var element in elementsToAnimate)
        {
            if (element != null)
            {
                element.sizeDelta = new Vector2(element.sizeDelta.x, 0);
            }
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}