using UnityEngine;

public class RoomShakeTrigger : MonoBehaviour
{
    [Header("自定义抖动覆盖（若不勾选则使用 Panel 上的默认参数）")]
    [SerializeField] private bool useCustomSettings = false;
    [SerializeField] private float customDuration = 0.35f;
    [SerializeField] private float customMagnitude = 30f; // 建议测试时填大一点，如 30~50
    [SerializeField] private float customFrequency = 50f;

    /// <summary>
    /// 在物品的 EventList / UnityEvent 栏中直接选择这个方法
    /// </summary>
    public void TriggerCanvasShake()
    {
        // 这一行必须最先执行，用来检验 EventList 到底有没有调到这里！
        Debug.Log($"<color=cyan>[RoomShakeTrigger] TriggerCanvasShake 被成功调用！来自物体: {gameObject.name}</color>");

        if (UIManager.Instance == null)
        {
            Debug.LogError("[RoomShakeTrigger] UIManager.Instance 为空！请检查场景中是否有 UIManager。");
            return;
        }

        if (UIManager.Instance.roomPanel == null)
        {
            Debug.LogError("[RoomShakeTrigger] UIManager 上的 roomPanel 没有赋值！请把场景里的 Panel 拖进去。");
            return;
        }

        // 获取 Panel 身上挂的 RoomShake
        RoomShake panelShake = UIManager.Instance.roomPanel.GetComponent<RoomShake>();

        // 防呆设计：如果忘记挂，自动动态补挂一个
        if (panelShake == null)
        {
            Debug.LogWarning("[RoomShakeTrigger] 检测到 Panel 上未挂载 RoomShake，正在自动挂载...");
            panelShake = UIManager.Instance.roomPanel.gameObject.AddComponent<RoomShake>();
        }

        if (useCustomSettings)
        {
            panelShake.TriggerShake(customDuration, customMagnitude, customFrequency);
        }
        else
        {
            panelShake.TriggerShake();
        }
    }
}