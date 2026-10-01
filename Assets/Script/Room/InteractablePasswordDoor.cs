using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InteractablePasswordDoor : MonoBehaviour, IPointerClickHandler
{
    [Header("密码与多语言验证")]
    [Tooltip("密码的本地化 Key。如果设置了，会优先读取当前游戏语言下的翻译作为有效密码")]
    [SerializeField] private string passcodeLocalizationKey;

    [Tooltip("默认/主密码（支持短语、句子或数字）")]
    [TextArea(1, 2)]
    [SerializeField] private string defaultPasscode = "Project-Delta";

    [Tooltip("多语言别名列表：所有填入此处的字符串都会被视作有效密码（例如中英双语、常见近义表达）")]
    [SerializeField] private List<string> passcodeAliases = new List<string>();

    [Tooltip("是否区分英文字母大小写")]
    [SerializeField] private bool caseSensitive = false;

    [Tooltip("是否已解锁")]
    [SerializeField] private bool isUnlocked = false;

    [Header("解锁成功事件列表")]
    public UnityEvent OnUnlockSuccess;

    /// <summary>
    /// UGUI 原生点击检测接口，无需配置 OnClick 列表
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 仅响应鼠标左键点击
        if (eventData.button != PointerEventData.InputButton.Left) return;

        InteractWithDoor();
    }

    /// <summary>
    /// 触发交互核心逻辑
    /// </summary>
    public void InteractWithDoor()
    {
        if (isUnlocked)
        {
            Debug.Log("[Door] 门已解锁，无需再次输入。");
            return;
        }

        if (PasswordLockUI.Instance == null)
        {
            Debug.LogError("[Door] 场景中缺少 PasswordLockUI 实例！");
            return;
        }

        // 收集所有合法的有效密码
        List<string> validCodes = new List<string>();

        // 1. 加入默认密码
        if (!string.IsNullOrEmpty(defaultPasscode))
        {
            validCodes.Add(defaultPasscode);
        }

        // 2. 如果配置了本地化 Key，获取当前语种下的翻译文本
        if (!string.IsNullOrEmpty(passcodeLocalizationKey))
        {
            string localizedPasscode = GetLocalizedText(passcodeLocalizationKey, string.Empty);
            if (!string.IsNullOrEmpty(localizedPasscode) && !validCodes.Contains(localizedPasscode))
            {
                validCodes.Add(localizedPasscode);
            }
        }

        // 3. 加入预设的多语言别名（如繁体、英文等）
        foreach (var alias in passcodeAliases)
        {
            if (!string.IsNullOrEmpty(alias) && !validCodes.Contains(alias))
            {
                validCodes.Add(alias);
            }
        }

        // 打开弹窗输入框并传入密码集合
        PasswordLockUI.Instance.OpenLock(validCodes, caseSensitive, OnPasswordCorrect);
    }

    private void OnPasswordCorrect()
    {
        isUnlocked = true;
        Debug.Log("[Door] 密码校验通过，触发解锁事件。");
        OnUnlockSuccess?.Invoke();
    }

    private string GetLocalizedText(string key, string fallback)
    {
        if (string.IsNullOrEmpty(key)) return fallback;
        // 对接本地化系统：
        // return LocalizationManager.Instance.Get(key);
        return fallback;
    }
}