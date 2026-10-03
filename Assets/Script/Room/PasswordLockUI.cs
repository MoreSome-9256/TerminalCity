using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PasswordLockUI : MonoBehaviour
{
    public static PasswordLockUI Instance;

    [Header("UI 容器与组件")]
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text errorHintText;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;

    // --- 补全缺失的字段 ---
    [Header("多语言错误提示 Key")]
    [SerializeField] private string errorHintLocalizationKey = "UI_PASS_DENIED";
    [SerializeField] private string defaultErrorMsg = "ACCESS DENIED / 认证失败";

    private List<string> acceptedPasscodes = new List<string>();
    private bool caseSensitive;
    private Action onSuccessCallback;
    private Action onCloseCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (submitButton != null) submitButton.onClick.AddListener(CheckInput);
        if (closeButton != null) closeButton.onClick.AddListener(CloseWindow);
        if (passwordInput != null) passwordInput.onSubmit.AddListener((val) => CheckInput());

        CloseWindow();
    }

    private void Update()
    {
        if (windowRoot != null && windowRoot.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseWindow();
            }
        }
    }

    public void OpenLock(List<string> validCodes, bool isCaseSensitive, Action onSuccess, Action onClose = null)
    {
        acceptedPasscodes = validCodes ?? new List<string>();
        caseSensitive = isCaseSensitive;
        onSuccessCallback = onSuccess;
        onCloseCallback = onClose;

        if (errorHintText != null) errorHintText.gameObject.SetActive(false);
        if (passwordInput != null) passwordInput.text = string.Empty;

        if (windowRoot != null)
        {
            windowRoot.SetActive(true);
        }

        // 延迟到当前帧渲染更新后拉起光标，确保焦点稳定生效
        StartCoroutine(FocusInputFieldNextFrame());
    }

    private IEnumerator FocusInputFieldNextFrame()
    {
        // 1. 等待直到该帧的所有 UI 布局与激活流程完成
        yield return new WaitForEndOfFrame();

        if (passwordInput != null)
        {
            // 2. 核心：强制让全局 EventSystem 选中并锁定该 InputField
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(
                    passwordInput.gameObject,
                    new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current)
                );
            }

            // 3. 原生唤起与聚焦
            passwordInput.Select();
            passwordInput.ActivateInputField();

            // 4. 重置光标位置
            passwordInput.caretPosition = 0;
            passwordInput.selectionStringAnchorPosition = 0;
            passwordInput.selectionStringFocusPosition = 0;

            // 5. 强制刷新 TMP 内部组件与网格渲染，确保即便文本为空也绘制光标
            passwordInput.ForceLabelUpdate();
        }
    }

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
            Action success = onSuccessCallback;
            CloseWindow();
            success?.Invoke();
        }
        else
        {
            string errorText = GetLocalizedText(errorHintLocalizationKey, defaultErrorMsg);
            ShowErrorFeedback(errorText);
        }
    }

    // --- 补全缺失的辅助方法 ---
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

    private string GetLocalizedText(string key, string fallback)
    {
        if (string.IsNullOrEmpty(key)) return fallback;
        // 如果接入了多语言系统，可在此调用：
        // return LocalizationManager.Instance.Get(key);
        return fallback;
    }

    public void CloseWindow()
    {
        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }

        onCloseCallback?.Invoke();
        onCloseCallback = null;
        onSuccessCallback = null;
    }
}