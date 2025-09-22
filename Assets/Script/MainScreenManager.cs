using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreenManager : MonoBehaviour
{
    public static MainScreenManager Instance;

    [Header("所有主屏 UI 面板（拖到 Inspector）")]
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
    public void ShowPanel(GameObject panelToShow)
    {
        foreach (var panel in panels)
        {
            if (panel != null)
                panel.SetActive(panel == panelToShow);
        }
    }
    public void HideAll()
    {
        foreach (var panel in panels)
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
