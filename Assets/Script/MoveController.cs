using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveController : MonoBehaviour
{
    public GameObject moveObject;
    public bool isMoveX;   // 仅X方向移动
    public bool isMoveY;   // 仅Y方向移动
    public float targetX;
    public float targetY;
    public float moveDuration = 2.0f;
    public float waitTime1 = 0f;  // 移入前等待时间
    public float waitTime2 = 0f;  // 移回前等待时间
    private Button button1;
    public Button button2;

    private Vector3 originalPosition;
    private Vector3 targetScreenPosition;
    private bool isMoving = false;

    void Start()
    {
        originalPosition = moveObject.transform.position;

        float screenRatio = (float)Screen.height / 720f;

        // 根据标记来决定移动方向
        float targetPosX = isMoveY ? originalPosition.x : targetX * screenRatio;
        float targetPosY = isMoveX ? originalPosition.y : targetY * screenRatio;

        targetScreenPosition = new Vector3(
            targetPosX,
            targetPosY,
            originalPosition.z
        );

        if (button2 != null)
            button2.onClick.AddListener(() => StartMove(false));
    }

    // prefab 调用此方法注册按钮
    public void RegisterOpenButton(Button prefabButton)
    {
        if (prefabButton == null) return;
        Debug.Log($"RegisterOpenButton: {prefabButton.name}, active: {prefabButton.gameObject.activeInHierarchy}");
        // 保存引用
        button1 = prefabButton;

        // 确保不会重复注册
        prefabButton.onClick.RemoveListener(OnOpenButtonClicked);
        prefabButton.onClick.AddListener(OnOpenButtonClicked);
    }

    private void OnOpenButtonClicked()
    {
        Debug.Log("OnOpenButtonClicked"); // 确认触发
        StartMove(true);
    }

    public void StartMove(bool moveIn)
    {
        if (!isMoving)
        {
            StartCoroutine(moveIn ? MoveToTarget() : ReturnToOriginal());
        }
    }

    private IEnumerator MoveToTarget()
    {
        isMoving = true;

        if (waitTime1 > 0)
        {
            yield return new WaitForSeconds(waitTime1);
        }

        yield return SmoothMove(moveObject.transform,
            moveObject.transform.position,
            targetScreenPosition,
            moveDuration);

        isMoving = false;
    }

    private IEnumerator ReturnToOriginal()
    {
        isMoving = true;

        if (waitTime2 > 0)
        {
            yield return new WaitForSeconds(waitTime2);
        }

        yield return SmoothMove(moveObject.transform,
            moveObject.transform.position,
            originalPosition,
            moveDuration);

        isMoving = false;
    }

    private IEnumerator SmoothMove(Transform target,
                                Vector3 startPos,
                                Vector3 endPos,
                                float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            target.position = Vector3.Lerp(startPos, endPos, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.position = endPos;
    }
}
