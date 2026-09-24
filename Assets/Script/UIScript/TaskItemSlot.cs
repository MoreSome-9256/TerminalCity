using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class TaskItemSlot : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private TMP_Text taskDescText;
    [SerializeField] private TMP_InputField taskInputField; // 原地编辑框
    [SerializeField] private Button checkToggleBtn;
    [SerializeField] private GameObject markObj;

    [Header("完成透明度控制")]
    [Range(0f, 1f)]
    [SerializeField] private float completedAlpha = 0.45f;

    private CanvasGroup canvasGroup;
    private TaskData currentData;
    private System.Action<string> onStatusChangedCallback;
    private System.Action<TaskItemSlot, string> onEditFinishedCallback;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (checkToggleBtn != null)
        {
            checkToggleBtn.onClick.AddListener(OnToggleClicked);
        }

        if (taskInputField != null)
        {
            taskInputField.onSubmit.AddListener(OnSubmitEdit);
            taskInputField.onEndEdit.AddListener(OnSubmitEdit);
        }
    }

    /// <summary>
    /// 常规初始化
    /// </summary>
    public void Setup(TaskData data, System.Action<string> onStatusChanged)
    {
        currentData = data;
        onStatusChangedCallback = onStatusChanged;

        // 默认进入常规展示模式
        if (taskInputField != null) taskInputField.gameObject.SetActive(false);
        if (taskDescText != null) taskDescText.gameObject.SetActive(true);

        RefreshUI();
    }

    /// <summary>
    /// 进入新建/就地编辑模式（带闪烁光标）
    /// </summary>
    public void EnterEditMode(System.Action<TaskItemSlot, string> onEditFinished)
    {
        onEditFinishedCallback = onEditFinished;

        if (taskDescText != null) taskDescText.gameObject.SetActive(false);

        if (taskInputField != null)
        {
            taskInputField.gameObject.SetActive(true);
            taskInputField.text = string.Empty;
            taskInputField.Select();
            taskInputField.ActivateInputField(); // 激活输入框并弹出闪烁光标
        }

        if (markObj != null) markObj.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    private void OnSubmitEdit(string text)
    {
        if (onEditFinishedCallback != null)
        {
            var callback = onEditFinishedCallback;
            onEditFinishedCallback = null; // 防止回车和失去焦点触发两次
            callback.Invoke(this, text.Trim());
        }
    }

    private void RefreshUI()
    {
        if (currentData == null) return;

        if (taskDescText != null)
        {
            taskDescText.text = currentData.description;
        }

        if (markObj != null)
        {
            markObj.SetActive(currentData.isCompleted);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = currentData.isCompleted ? completedAlpha : 1f;
        }
    }

    private void OnToggleClicked()
    {
        if (currentData == null) return;

        currentData.isCompleted = !currentData.isCompleted;
        RefreshUI();
        onStatusChangedCallback?.Invoke(currentData.taskID);
    }
}