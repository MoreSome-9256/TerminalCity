using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class SideScreenMove : MonoBehaviour, IPointerClickHandler
{
    // 引用要移动的物体
    public GameObject targetObject;
    public GameObject dialogBox;
    public GameObject mainScreen;
    public GameObject topRightScreen;
    public Button button;
    public float targetY;
    public float targetZ;
    public float targetY2;
    public float targetZ2;
    public float targetX3;
    public float targetZ3;
    public float targetX5;
    public float targetZ5;

    public float moveDuration = 2.0f; // 移动的持续时间（秒）
    public float moveDuration2 = 2.0f; //主屏幕的移动时间
    public float rotationDuration = 0.5f; // 旋转持续的时间（秒）
    public float targetAngle = 180f; // 目标旋转角度（度数）

    private RectTransform rectTransform;
    private bool isRotating;
    public bool isMoved = false;

    private Vector3 originalPosition;//保存侧边屏幕的原始位置
    private Vector3 originalPosition2;//保存对话框的原始位置
    private Vector3 originalPosition3;//保留主屏幕的原始位置
    private Vector3 originalPosition5;//保留右上屏幕的原始位置

    public GameObject Menu;
    public float targetX4;
    public float targetZ4;
    public float moveDuration3 = 0.5f;
    private Vector3 originalPosition4;

    public UnityEvent OnBagOpened;
    public UnityEvent OnBagClosed;
    public GameObject mask;

    void Start() {
        if (targetObject != null)
        {
            originalPosition = targetObject.transform.position;
            originalPosition2 = dialogBox.transform.position;
            originalPosition3 = mainScreen.transform.position;
            originalPosition4 = Menu.transform.position;
            originalPosition5 = topRightScreen.transform.position;
        }
        rectTransform = button.GetComponent<RectTransform>();
    }

    // 实现IPointerClickHandler接口的OnPointerClick方法
    private bool isAnimating = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating) return; // 动画进行中，忽略点击

        if (!isMoved)
        {
            StartCoroutine(SequentialMove1());
        }
        else
        {
            StartCoroutine(SequentialMove2());
        }
    }

    // 平滑移动的协程
    private IEnumerator SmoothMove(Transform obj, Vector3 startPos, Vector3 endPos, float duration, float temp)
    {
        float elapsed = 0f;
        float t1 = (float)(temp * ((float)Screen.height / 405.0f));
        Vector3 startPos2 = new Vector3(startPos.x, startPos.y - t1, startPos.z);

        // 第一个阶段：移动到startPos2
        float firstDuration = duration / 5f;
        while (elapsed < firstDuration)
        {
            float t = elapsed / firstDuration;
            obj.position = Vector3.Lerp(startPos, startPos2, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 第二个阶段：从startPos2移动到endPos，使用缓出效果
        while (elapsed < duration)
        {
            float remainingTime = duration - elapsed;
            float t = remainingTime / (duration - firstDuration); // 使用剩余时间计算t

            // 应用缓出效果
            float easeOutT = 1f - t * t;

            obj.position = Vector3.Lerp(startPos2, endPos, easeOutT);

            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.position = endPos;
    }

    private IEnumerator RotateButton(float targetAngle, float duration)
    {
        isRotating = true;

        float startTime = Time.time;
        float startAngle = rectTransform.localEulerAngles.z;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration; // 插值因子
            float newAngle = Mathf.Lerp(startAngle, targetAngle, t); // 线性插值计算新角度

            rectTransform.localEulerAngles = new Vector3(0, 0, newAngle); // 应用新角度

            yield return null; // 等待下一帧
        }

        // 确保最终角度与目标角度完全匹配（由于浮点精度问题，可能有微小差异）
        rectTransform.localEulerAngles = new Vector3(0, 0, targetAngle);

        isRotating = false; // 旋转结束
    }
    private IEnumerator SequentialMove1()
    {
        isAnimating = true;

        if (mask != null)
            mask.SetActive(true);

        float currentX = targetObject.transform.position.x;
        float y1 = targetY * ((float)Screen.height / 405.0f);
        Vector3 targetPosition = new Vector3(currentX, y1, targetZ);

        float currentX2 = dialogBox.transform.position.x;
        float y2 = targetY2 * ((float)Screen.height / 405.0f);
        Vector3 targetPosition2 = new Vector3(currentX2, y2, targetZ2);

        float currentY = mainScreen.transform.position.y;
        float x3 = targetX3 * ((float)Screen.width / 720.0f);
        Vector3 targetPosition3 = new Vector3(x3, currentY, targetZ3);

        float currentY2 = Menu.transform.position.y;
        float x4 = targetX4 * ((float)Screen.width / 720.0f);
        Vector3 targetPosition4 = new Vector3(x4, currentY2, targetZ4);

        float currentY3 = topRightScreen.transform.position.y;
        float x5 = targetX5 * ((float)Screen.width / 720.0f);
        Vector3 targetPosition5 = new Vector3(x5, currentY3, targetZ5);

        // side screen, button, dialogBox 同时动
        Coroutine moveSide = StartCoroutine(SmoothMove(targetObject.transform, targetObject.transform.position, targetPosition, moveDuration, 20));
        Coroutine rotateBtn = StartCoroutine(RotateButton(targetAngle, rotationDuration));
        Coroutine moveDialog = StartCoroutine(SmoothMove(dialogBox.transform, dialogBox.transform.position, targetPosition2, moveDuration, 0));
        Coroutine moveTopRight = StartCoroutine(SmoothMove(topRightScreen.transform, topRightScreen.transform.position, targetPosition5, moveDuration, 0));

        // 稍后再动 mainScreen 和 Menu
        yield return new WaitForSeconds(0.3f);
        Coroutine moveMain = StartCoroutine(SmoothMove(mainScreen.transform, mainScreen.transform.position, targetPosition3, moveDuration2, 0));
        Coroutine moveMenu = StartCoroutine(SmoothMove(Menu.transform, Menu.transform.position, targetPosition4, moveDuration3, 0));

        // 等全部完成
        yield return moveSide;
        yield return rotateBtn;
        yield return moveDialog;
        yield return moveMain;
        yield return moveMenu;
        yield return moveTopRight;

        isMoved = true;
        isAnimating = false;
    }

    private IEnumerator SequentialMove2()
    {
        isAnimating = true;
        float currentX = targetObject.transform.position.x;
        float y1 = (float)(targetY * ((float)Screen.height / 405.0f));
        Vector3 targetPosition = new Vector3(currentX, y1, targetZ);

        float currentX2 = dialogBox.transform.position.x;
        float y2 = (float)(targetY2 * ((float)Screen.height / 405.0f));
        Vector3 targetPosition2 = new Vector3(currentX2, y2, targetZ2);

        float currentY = mainScreen.transform.position.y;
        float x3 = (float)(targetX3 * ((float)Screen.width / 720.0f));
        Vector3 targetPosition3 = new Vector3(x3, currentY, targetZ3);

        float currentY2 = Menu.transform.position.y;
        float x4 = (float)(targetX4 * ((float)Screen.width / 720.0f));
        Vector3 targetPosition4 = new Vector3(x4, currentY2, targetZ4);

        float currentY3 = topRightScreen.transform.position.y;
        float x5 = (float)(targetX5 * ((float)Screen.width / 720.0f));
        Vector3 targetPosition5 = new Vector3(x5, currentY3, targetZ5);

        // 同时关闭菜单和主屏幕
        Coroutine closeMenu = StartCoroutine(SmoothMove(Menu.transform, targetPosition4, originalPosition4, moveDuration3, 0));
        Coroutine closeMain = StartCoroutine(SmoothMove(mainScreen.transform, targetPosition3, originalPosition3, moveDuration, 0));

        // 等两个都结束
        yield return closeMenu;
        yield return closeMain;

        // 同时关闭 side screen、旋转按钮、dialogBox
        Coroutine moveSide = StartCoroutine(SmoothMove(targetObject.transform, targetPosition, originalPosition, moveDuration, 20));
        Coroutine rotateBtn = StartCoroutine(RotateButton(targetAngle + 180f, rotationDuration));
        yield return StartCoroutine(SmoothMove(dialogBox.transform, targetPosition2, originalPosition2, moveDuration, 0));
        yield return StartCoroutine(SmoothMove(topRightScreen.transform, targetPosition5, originalPosition5, moveDuration, 0));

        // 等 side screen 和按钮旋转完成
        yield return moveSide;
        yield return rotateBtn;

        // 最后触发背包关闭事件
        OnBagClosed?.Invoke();
        //InventoryManager.instance.OnInventoryClosed();
        if (mask != null)
            mask.SetActive(false);
        isMoved = false;
        isAnimating = false;
    }

    public void ToggleInventory()
    {
        isMoved = !isMoved;

        if (isMoved)
            OnBagOpened?.Invoke();
        else
            OnBagClosed?.Invoke();
    }
}