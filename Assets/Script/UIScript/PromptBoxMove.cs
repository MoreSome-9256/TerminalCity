using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class PromptBoxMove : MonoBehaviour
{
    public GameObject promptBox;
    public float targetY = 210f;
    public float moveDuration = 1.0f;
    private RectTransform rectTransform;
    private Vector3 originalPosition;

    public TMP_Text tipText;
    public string promptText;

    public Button button;

    [Header("事件配置")]
    public UnityEvent OnMoveComplete = new UnityEvent();

    private Coroutine currentRoutine;
    private bool isOperationDone;
    private float remainingWaitTime;
    public bool isAuto = true;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = promptBox.transform.position;
        isOperationDone = false;
        if (button != null)
        {
            // 移除旧监听避免重复
            button.onClick.RemoveListener(OnTargetButtonClicked);
            // 添加新监听
            button.onClick.AddListener(OnTargetButtonClicked);
        }
    }
    private void OnTargetButtonClicked()
    {
        // 双重验证确保提示框处于激活状态
        if (!isOperationDone && promptBox.activeInHierarchy)
        {
            MarkOperationComplete();
        }
    }
    public void MarkOperationComplete()
    {
        if (!isOperationDone)
        {
            isOperationDone = true;
            // 立即中断等待流程
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                currentRoutine = StartCoroutine(ReturnAnimation());
            }
        }
    }

    public void StartPromptAnimation()
    {
        //Debug.Log("prompt box");
        promptBox.SetActive(true);
        tipText.text = promptText;
        isOperationDone = false; // 重置状态
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        currentRoutine = StartCoroutine(SequentialMove());
    }
    private IEnumerator SequentialMove()
    {
        float currentX = promptBox.transform.position.x;
        float currentZ = promptBox.transform.position.z;
        float y = (float)(targetY * ((float)Screen.height / 416.0f));
        Vector3 targetPosition = new Vector3(currentX, y, currentZ);

        // 上升动画
        yield return StartCoroutine(SmoothMove(promptBox.transform, promptBox.transform.position, targetPosition, moveDuration));
        if (isAuto)
        {
            remainingWaitTime = 3.0f;
            // 可中断的等待阶段
            while (remainingWaitTime > 0 && !isOperationDone)
            {
                remainingWaitTime -= Time.deltaTime;
                yield return null;
            }

            // 下降动画
            yield return StartCoroutine(ReturnAnimation());
        }
        
    }

    private IEnumerator ReturnAnimation()
    {
        Vector3 currentPosition = promptBox.transform.position;
        yield return StartCoroutine(SmoothMove(promptBox.transform, currentPosition, originalPosition, moveDuration));

        // 收尾工作
        //promptBox.SetActive(false);
        OnMoveComplete?.Invoke();
        currentRoutine = null;
    }
    private IEnumerator SmoothMove(Transform obj, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float remainingTime = duration - elapsed;
            float t = remainingTime / duration;
            // 应用缓出效果
            float easeOutT = 1f - t * t;

            obj.position = Vector3.Lerp(startPos, endPos, easeOutT);

            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.position = endPos;
    }
}