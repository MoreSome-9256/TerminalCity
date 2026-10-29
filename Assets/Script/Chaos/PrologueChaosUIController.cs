using System.Collections.Generic;
using UnityEngine;

public class PrologueChaosUIController : MonoBehaviour
{
    public static PrologueChaosUIController Instance { get; private set; }

    [Header("绑定的动态音轨设置")]
    [Tooltip("要控制的动态分轨名称 (如 'Guitar' 或 'Noise')")]
    [SerializeField] private string targetTrackName = "Guitar";

    [System.Serializable]
    public class RoomUIConfig
    {
        public int roomID;
        [Header("混乱度展示区间 (若无波动需求，上下限设为一致即可)")]
        [Range(0f, 1f)] public float minRatio = 0.5f;
        [Range(0f, 1f)] public float maxRatio = 0.6f;

        [Header("当前房间该分轨的目标音量")]
        [Range(0f, 1f)] public float targetTrackVolume = 0f;
    }

    [Header("序章开局 UI 默认展示区间")]
    [Range(0f, 1f)][SerializeField] private float initialMinRatio = 0.1f;
    [Range(0f, 1f)][SerializeField] private float initialMaxRatio = 0.12f;

    [Header("波动设置")]
    [Tooltip("波动变化的快慢频率，数值越大数字跳动/游走越快")]
    [SerializeField] private float fluctuationSpeed = 0.8f;
    [Tooltip("切换房间时基准过渡到新区间的平滑速度")]
    [SerializeField] private float transitionSpeed = 1.0f;

    [Header("每个房间对应的 UI 演出目标")]
    [SerializeField] private List<RoomUIConfig> roomDisplayConfigs = new List<RoomUIConfig>();

    // ✨ 修改：直接把整个 Config 存进字典，方便同时拿取 UI 区间和音量
    private Dictionary<int, RoomUIConfig> roomConfigMap = new Dictionary<int, RoomUIConfig>();
    private HashSet<int> visitedRooms = new HashSet<int>();

    // 当前处于的目标区间
    private float targetMin;
    private float targetMax;

    // 平滑插值中的基准区间
    private float currentMin;
    private float currentMax;

    private float noiseSeed;

    private void Awake()
    {
        Instance = this;
        noiseSeed = Random.Range(0f, 100f);

        roomConfigMap.Clear();
        foreach (var config in roomDisplayConfigs)
        {
            if (!roomConfigMap.ContainsKey(config.roomID))
            {
                roomConfigMap.Add(config.roomID, config);
            }
        }
    }

    private void Start()
    {
        targetMin = Mathf.Min(initialMinRatio, initialMaxRatio);
        targetMax = Mathf.Max(initialMinRatio, initialMaxRatio);

        currentMin = targetMin;
        currentMax = targetMax;

        UpdateChaosUI();
    }

    private void Update()
    {
        // 1. 基准区间向目标区间平滑靠近（切房间时顺滑过渡，避免瞬间跳变）
        currentMin = Mathf.MoveTowards(currentMin, targetMin, transitionSpeed * Time.deltaTime);
        currentMax = Mathf.MoveTowards(currentMax, targetMax, transitionSpeed * Time.deltaTime);

        UpdateChaosUI();
    }

    private void UpdateChaosUI()
    {
        float finalRatio;

        // 如果上下限相同，则无波动，直接使用固定值
        if (Mathf.Approximately(currentMin, currentMax))
        {
            finalRatio = currentMin;
        }
        else
        {
            // 使用柏林噪声实现自然平滑的游走波动，避免 Random.Range 造成的剧烈闪烁抽搐
            float t = Mathf.PerlinNoise(Time.time * fluctuationSpeed, noiseSeed);
            finalRatio = Mathf.Lerp(currentMin, currentMax, t);
        }

        // 推送给前端 ChaosUI
        if (ChaosUI.Instance != null)
        {
            ChaosUI.Instance.SetOverrideChaos(finalRatio);
        }
    }

    /// <summary>
    /// 进入新房间时调用
    /// </summary>
    public void OnRoomExplored(int roomID)
    {
        if (!visitedRooms.Contains(roomID))
        {
            visitedRooms.Add(roomID);

            if (roomConfigMap.TryGetValue(roomID, out RoomUIConfig config))
            {
                float configMin = Mathf.Min(config.minRatio, config.maxRatio);
                float configMax = Mathf.Max(config.minRatio, config.maxRatio);

                // 仅当新房间的上限大于等于当前上限时才更新，防止向后探索时倒退
                if (configMax >= targetMax)
                {
                    targetMin = configMin;
                    targetMax = configMax;
                    Debug.Log($"<color=#00FFB2>[序章UI] 匹配房间 {roomID}，展示区间设定为: [{targetMin * 100}% ~ {targetMax * 100}%]</color>");

                    // ✨ 新增：同步控制 BGM 分轨音量
                    if (BGMManager.Instance != null && !string.IsNullOrEmpty(targetTrackName))
                    {
                        BGMManager.Instance.SetTrackVolume(targetTrackName, config.targetTrackVolume);
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        if (ChaosUI.Instance != null)
        {
            ChaosUI.Instance.ClearOverride();
        }
    }

    private void OnDestroy()
    {
        if (ChaosUI.Instance != null)
        {
            ChaosUI.Instance.ClearOverride();
        }
    }
}