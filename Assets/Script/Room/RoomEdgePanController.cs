using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RoomEdgePanController : MonoBehaviour
{
    [Header("当前控制的大房间 RectTransform")]
    public RectTransform roomContent;

    [Header("四个方向的感应按钮 (Image/Button)")]
    public RectTransform btnUp;
    public RectTransform btnDown;
    public RectTransform btnLeft;
    public RectTransform btnRight;

    [Header("移动参数")]
    [Tooltip("鼠标悬停时的每秒平移像素速度")]
    public float moveSpeed = 350f;
    [Tooltip("平滑缓冲时间 (越小越跟手，0为纯线性)")]
    public float smoothTime = 0.08f;

    // 边界完全由当前激活的 BigRoomConfig 动态传入，不在 Inspector 暴露
    private float leftMoveX;
    private float rightMoveX;
    private float downMoveY;
    private float upMoveY;

    private Vector2 startPosition = Vector2.zero;

    [Header("到达边界时是否自动隐藏对应方向按钮")]
    public bool hideButtonAtLimit = true;

    private Vector2 currentHoverDir = Vector2.zero;
    private Vector2 targetPosition;
    private Vector2 currentVelocity;

    private void Awake()
    {
        // 保持自身脚本物体永远激活，只把四个子按钮默认隐藏
        HideAllButtons();
    }

    private void HideAllButtons()
    {
        if (btnUp != null) btnUp.gameObject.SetActive(false);
        if (btnDown != null) btnDown.gameObject.SetActive(false);
        if (btnLeft != null) btnLeft.gameObject.SetActive(false);
        if (btnRight != null) btnRight.gameObject.SetActive(false);
    }
    private void Start()
    {
        BindHoverZone(btnUp, new Vector2(0, 1));
        BindHoverZone(btnDown, new Vector2(0, -1));
        BindHoverZone(btnLeft, new Vector2(-1, 0));
        BindHoverZone(btnRight, new Vector2(1, 0));
    }

    /// <summary>
    /// 离开大房间时关闭平移与四个方向按钮
    /// </summary>
    public void DisablePan()
    {
        roomContent = null;
        currentHoverDir = Vector2.zero;
        HideAllButtons();
        // 注意：不要再写 gameObject.SetActive(false); 让控制器保持待命！
    }

    private void BindHoverZone(RectTransform btn, Vector2 dir)
    {
        if (btn == null) return;
        HoverPanZone zone = btn.GetComponent<HoverPanZone>();
        if (zone == null) zone = btn.gameObject.AddComponent<HoverPanZone>();
        zone.Init(this, dir);
    }

    /// <summary>
    /// 供切换房间时动态绑定新的大房间和它的专属边界
    /// </summary>
    public void SetupBigRoom(RectTransform newRoom, float leftX, float rightX, float downY, float upY, Vector2 initPos)
    {
        roomContent = newRoom;
        leftMoveX = leftX;
        rightMoveX = rightX;
        downMoveY = downY;
        upMoveY = upY;

        // 先打开四个按钮，再根据边界裁剪
        if (btnUp != null) btnUp.gameObject.SetActive(true);
        if (btnDown != null) btnDown.gameObject.SetActive(true);
        if (btnLeft != null) btnLeft.gameObject.SetActive(true);
        if (btnRight != null) btnRight.gameObject.SetActive(true);

        SetPositionImmediate(initPos);
    }

    public void SetPositionImmediate(Vector2 pixelPos)
    {
        // 1. 彻底清零平滑阻尼的残留速度，防止切房间时惯性抽动
        currentVelocity = Vector2.zero;
        currentHoverDir = Vector2.zero;

        targetPosition.x = Mathf.Clamp(pixelPos.x, leftMoveX, rightMoveX);
        targetPosition.y = Mathf.Clamp(pixelPos.y, downMoveY, upMoveY);

        if (roomContent != null)
        {
            roomContent.anchoredPosition = new Vector2(-targetPosition.x, -targetPosition.y);
        }
        UpdateButtonVisibility();
    }

    public void SetHoverDirection(Vector2 dir, bool isHovering)
    {
        if (isHovering)
            currentHoverDir = dir;
        else if (currentHoverDir == dir)
            currentHoverDir = Vector2.zero;
    }

    private void Update()
    {
        if (roomContent == null) return;

        if (currentHoverDir != Vector2.zero)
        {
            targetPosition += currentHoverDir * moveSpeed * Time.unscaledDeltaTime;
            targetPosition.x = Mathf.Clamp(targetPosition.x, leftMoveX, rightMoveX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, downMoveY, upMoveY);
        }

        Vector2 desiredAnchored = new Vector2(-targetPosition.x, -targetPosition.y);

        // 2. 如果已经到达目标点且没有悬停输入，直接对齐并停止阻尼计算
        if (currentHoverDir == Vector2.zero && (roomContent.anchoredPosition - desiredAnchored).sqrMagnitude < 0.01f)
        {
            roomContent.anchoredPosition = desiredAnchored;
            currentVelocity = Vector2.zero;
            return;
        }

        if (smoothTime > 0f)
        {
            roomContent.anchoredPosition = Vector2.SmoothDamp(
                roomContent.anchoredPosition,
                desiredAnchored,
                ref currentVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
        }
        else
        {
            roomContent.anchoredPosition = desiredAnchored;
        }

        if (hideButtonAtLimit)
        {
            UpdateButtonVisibility();
        }
    }

    /// <summary>
    /// 已经移到尽头的方向，自动隐藏该方向指示按钮
    /// </summary>
    private void UpdateButtonVisibility()
    {
        const float epsilon = 0.5f;
        if (btnRight != null) btnRight.gameObject.SetActive(targetPosition.x < rightMoveX - epsilon);
        if (btnLeft != null) btnLeft.gameObject.SetActive(targetPosition.x > leftMoveX + epsilon);
        if (btnUp != null) btnUp.gameObject.SetActive(targetPosition.y < upMoveY - epsilon);
        if (btnDown != null) btnDown.gameObject.SetActive(targetPosition.y > downMoveY + epsilon);
    }

    private void OnDisable()
    {
        currentHoverDir = Vector2.zero;
    }
}

/// <summary>
/// 自动挂载在四个方向按钮上的悬停检测器（无需点击，鼠标移入即触发）
/// </summary>
public class HoverPanZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RoomEdgePanController controller;
    private Vector2 direction;

    public void Init(RoomEdgePanController ctrl, Vector2 dir)
    {
        controller = ctrl;
        direction = dir;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (controller != null)
            controller.SetHoverDirection(direction, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (controller != null)
            controller.SetHoverDirection(direction, false);
    }

    private void OnDisable()
    {
        if (controller != null)
            controller.SetHoverDirection(direction, false);
    }
}