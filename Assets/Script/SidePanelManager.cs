using System.Collections.Generic;
using UnityEngine;

public class SidePanelManager : MonoBehaviour
{
    public static SidePanelManager Instance;

    [Header("所有右侧 UI 面板（拖到 Inspector）")]
    public List<GameObject> panels = new List<GameObject>();

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

    /// <summary>
    /// 显示指定面板，其他面板自动隐藏
    /// </summary>
    public void ShowPanel(GameObject panelToShow)
    {
        foreach (var panel in panels)
        {
            if (panel != null)
                panel.SetActive(panel == panelToShow);
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    public void HideAll()
    {
        foreach (var panel in panels)
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
