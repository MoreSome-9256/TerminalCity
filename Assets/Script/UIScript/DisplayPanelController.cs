using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class DisplayPanelController : MonoBehaviour
{
    public static DisplayPanelController Instance { get; private set; }

    [Header("Scroll Views 列表")]
    [Tooltip("依次放入 Scroll View 1, Scroll View 2 等子物体")]
    [SerializeField] private List<GameObject> displayScrollViews = new List<GameObject>();

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();

        // 初始化：默认完全隐藏且不阻挡点击
        HideDisplayPanel();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Prefab 里的物品点击时调用
    /// </summary>
    public void ShowDisplayPanel(int index)
    {
        // 显示面板
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 切换 Scroll View
        if (displayScrollViews != null)
        {
            for (int i = 0; i < displayScrollViews.Count; i++)
            {
                if (displayScrollViews[i] != null)
                {
                    displayScrollViews[i].SetActive(i == index);
                }
            }
        }
    }

    /// <summary>
    /// 关闭按钮调用
    /// </summary>
    public void HideDisplayPanel()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}