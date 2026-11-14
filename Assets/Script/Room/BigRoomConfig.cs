using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class BigRoomConfig : MonoBehaviour
{
    [Header("当前大房间的手动移动边界 (像素)")]
    public float leftMoveX = -300f;
    public float rightMoveX = 300f;
    public float downMoveY = -200f;
    public float upMoveY = 200f;

    [Header("进入房间时的初始坐标")]
    public Vector2 startPosition = Vector2.zero;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
        RegisterToManager();
    }

    private void OnEnable()
    {
        RegisterToManager();
    }

    private void RegisterToManager()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        RoomEdgePanController controller = null;

        if (UIManager.Instance != null && UIManager.Instance.edgePanController != null)
        {
            controller = UIManager.Instance.edgePanController;
        }
        else
        {
            // 兜底：如果 UIManager 没绑定槽位，直接按类型查找当前场景里的控制器
            controller = FindObjectOfType<RoomEdgePanController>(true);
        }

        if (controller != null)
        {
            controller.SetupBigRoom(
                rectTransform, leftMoveX, rightMoveX, downMoveY, upMoveY, startPosition
            );
        }
        else
        {
            Debug.LogWarning("[BigRoomConfig] 场景中找不到 RoomEdgePanController！");
        }
    }

    private void OnDisable()
    {
        // 房间隐藏或销毁时，如果当前正控制着自己，则自动关闭四向按钮
        if (UIManager.Instance != null && UIManager.Instance.edgePanController != null)
        {
            if (UIManager.Instance.edgePanController.roomContent == rectTransform)
            {
                UIManager.Instance.edgePanController.DisablePan();
            }
        }
    }
}