using UnityEngine;

public class IntroPanelsController : MonoBehaviour
{
    public static IntroPanelsController Instance { get; private set; }

    [Header("需要激活的全局 Panel")]
    [SerializeField] private GameObject panelA;
    [SerializeField] private GameObject panelB;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 同时激活这两个 Panel
    /// </summary>
    public void ActivateIntroPanels()
    {
        if (panelA != null)
        {
            panelA.SetActive(true);
        }

        if (panelB != null)
        {
            panelB.SetActive(true);
        }
    }
}