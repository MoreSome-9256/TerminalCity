using UnityEngine;

public class PlayerChaos : MonoBehaviour
{
    public static PlayerChaos Instance;

    [Header("混乱度参数")]
    [Range(0f, 1f)] public float chaos = 0.3f; // 初始30%
    public float baseRate = 0.01f;             // 基础增长速率
    public float rate = 50f;

    private bool hasTriggeredDialogue = false;
    private bool hasTriggeredDialogue1 = false;
    private bool hasTriggeredDialogue2 = false;

    private RoomState currentRoomState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (currentRoomState != null)
        {
            float rate = CalculateChaosIncreaseRate(currentRoomState.instability);
            chaos += rate * Time.deltaTime;
            chaos = Mathf.Clamp01(chaos);
        }
        CheckChaosThreshold();
    }

    /// <summary>
    /// 进入房间时更新房间信息
    /// </summary>
    public void OnEnterRoom(RoomState room)
    {
        currentRoomState = room;
    }

    /// <summary>
    /// 根据房间逆恒值计算混乱度增长速率
    /// </summary>
    private float CalculateChaosIncreaseRate(int instability)
    {
        // 示例公式：逆恒值 9800 以下不涨；9800~10000 区间线性增长
        if (instability < 9800) return 0f;

        float t = (instability - 9800) / rate;  // 0~1
        return baseRate * t; // 最多达到 baseRate
    }

    public void ReduceChaos(float amount)
    {
        chaos -= amount;
        chaos = Mathf.Clamp01(chaos);
    }

    // 返回当前混乱度对应的房间逆恒值每秒增长量
    public int GetRoomInstabilityGrowth()
    {
        if (chaos >= 1f)
            return 5;
        else if (chaos >= 0.9f)
            return 3;
        else if (chaos >= 0.8f)
            return 2;
        else if (chaos >= 0.7f)
            return 1;
        else
            return 0;
    }
    private void CheckChaosThreshold()
    {
        if (!hasTriggeredDialogue1 && chaos >= 0.7f)
        {
            hasTriggeredDialogue1 = true;
            GlobalDialogManager.Instance?.TriggerDialogue("30");
        }
        if (!hasTriggeredDialogue2 && chaos >= 0.8f)
        {
            hasTriggeredDialogue2 = true;
            GlobalDialogManager.Instance?.TriggerDialogue("32");
        }
        if (!hasTriggeredDialogue && chaos >= 0.9f)
        {
            hasTriggeredDialogue = true;
            Debug.Log("玩家混乱度首次达到 90%，触发对话");

            // 调用 GlobalDialogManager
            GlobalDialogManager.Instance?.TriggerDialogue("27");
            // "Chaos90" 替换为你在对话表里配置的 ID
        }
    }
}
