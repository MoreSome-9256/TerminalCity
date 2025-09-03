using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("全局 UI 面板")]
    public GameObject previewArea;  // 对应 PreView 面板
    public GameObject firstLevel;   // 对应 FirstLevel 面板 / 背包 UI

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 如果需要跨场景
    }
}
