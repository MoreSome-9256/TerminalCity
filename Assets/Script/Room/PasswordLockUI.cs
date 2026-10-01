using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordLockUI : MonoBehaviour
{
    public static PasswordLockUI Instance;

    [Header("UI 容器与组件")]
    [Tooltip("弹窗面板根节点，平时关闭，调用时开启")]
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text errorHintText;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;

    [Header("多语言错误提示 Key")]
    [SerializeField] private string errorHintLocalizationKey = "UI_PASS_DENIED";
    [SerializeField] private string defaultErrorMsg = "ACCESS DENIED / 认证失败";

    private List<string> acceptedPasscodes = new List<string>();
    private bool caseSensitive;
    private Action onSuccessCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(CheckInput);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseWindow);
        }

        if (passwordInput != null)
        {
            passwordInput.onSubmit.AddListener((val) => CheckInput());
        }

        // 确保启动时窗口处于关闭状态
        CloseWindow();
    }

    private void Update()
    {
        // 窗口处于开启状态时，允许按 ESC 键快速退出输入
        if (windowRoot != null && windowRoot.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseWindow();
            }
        }
    }

    /// <summary>
    /// 打开密码锁并传入多语言可用密码库
    /// </summary>
    public void OpenLock(List<string> validCodes, bool isCaseSensitive, Action onSuccess)
    {
        acceptedPasscodes = validCodes ?? new List<string>();
        caseSensitive = isCaseSensitive;
        onSuccessCallback = onSuccess;

        if (errorHintText != null)
        {
            errorHintText.gameObject.SetActive(false);
        }

        if (passwordInput != null)
        {
            passwordInput.text = string.Empty;
        }

        // 激活弹窗面板
        if (windowRoot != null)
        {
            windowRoot.SetActive(true);
        }

        // 自动聚焦并拉起闪烁光标
        if (passwordInput != null)
        {
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
    }

    /// <summary>
    /// 校验输入内容
    /// </summary>
    public void CheckInput()
    {
        if (passwordInput == null) return;

        string currentInput = passwordInput.text.Trim();
        bool isCorrect = false;

        StringComparison comparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        foreach (var code in acceptedPasscodes)
        {
            if (string.Equals(currentInput, code.Trim(), comparison))
            {
                isCorrect = true;
                break;
            }
        }

        if (isCorrect)
        {
            CloseWindow();
            onSuccessCallback?.Invoke();
        }
        else
        {
            string errorText = GetLocalizedText(errorHintLocalizationKey, defaultErrorMsg);
            ShowErrorFeedback(errorText);
        }
    }

    private void ShowErrorFeedback(string msg)
    {
        if (errorHintText != null)
        {
            errorHintText.text = msg;
            errorHintText.gameObject.SetActive(true);
        }

        if (passwordInput != null)
        {
            passwordInput.text = string.Empty;
            passwordInput.ActivateInputField();
        }
    }

    public void CloseWindow()
    {
        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }
        onSuccessCallback = null;
    }

    private string GetLocalizedText(string key, string fallback)
    {
        if (string.IsNullOrEmpty(key)) return fallback;
        // 对接本地化系统：
        // return LocalizationManager.Instance.Get(key);
        return fallback;
    }
}