using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DisplayPanelTrigger : MonoBehaviour
{
    [Tooltip("指定要打开哪一个 Scroll View：0 对应 Scroll View 1，1 对应 Scroll View 2...")]
    [SerializeField] private int targetScrollViewIndex = 0;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Debug.Log($"[{gameObject.name}] 触发点击，请求打开索引: {targetScrollViewIndex}");
        if (DisplayPanelController.Instance != null)
        {
            DisplayPanelController.Instance.ShowDisplayPanel(targetScrollViewIndex);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 未找到场景中的 DisplayPanelController，可能当前场景不包含此面板。");
        }
    }
}