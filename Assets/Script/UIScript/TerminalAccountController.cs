using System.Collections;
using UnityEngine;
using TMPro;

public class TerminalAccountController : MonoBehaviour
{
    [Header("状态配置")]
    [SerializeField] private bool isActivated = false;

    [Header("左下角账户信息")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private CanvasGroup accountDetailGroup; // 包含身份、编号的父节点
    [SerializeField] private TMP_Text identityText;
    [SerializeField] private TMP_Text idText;

    [Header("右侧/中栏主面板")]
    [SerializeField] private GameObject inactivePlaceholder;  // “系统未激活”提示物体
    [SerializeField] private CanvasGroup mainContentGroup;    // 激活后显示的所有业务面板

    [Header("音效或反馈 (可选)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activateSound;

    private void Awake()
    {
        // 根据初始设定应用状态（默认为未激活）
        ApplyState(isActivated, playAnimation: false);
    }

    /// <summary>
    /// 对外公开的激活方法，特定剧情或操作触发时直接调用此接口
    /// TerminalAccountController.Instance.ActivateSystem();
    /// </summary>
    [ContextMenu("Trigger Activate (测试激活)")]
    public void ActivateSystem()
    {
        if (isActivated) return;

        isActivated = true;
        StartCoroutine(ActivationSequenceRoutine());
    }

    /// <summary>
    /// 重置为未激活状态（如需支持重置/锁定）
    /// </summary>
    [ContextMenu("Trigger Deactivate (测试重置)")]
    public void DeactivateSystem()
    {
        isActivated = false;
        StopAllCoroutines();
        ApplyState(false, playAnimation: false);
    }

    private void ApplyState(bool active, bool playAnimation)
    {
        if (!active)
        {
            // 1. 设置状态文字
            if (statusText != null)
            {
                statusText.text = "状态：<color=#FFD19E>未激活</color>";
            }

            // 2. 隐藏左侧详细信息（身份、编号）
            if (accountDetailGroup != null)
            {
                accountDetailGroup.alpha = 0f;
                accountDetailGroup.interactable = false;
                accountDetailGroup.blocksRaycasts = false;
            }

            // 3. 右侧显示占位警示，隐藏所有交互内容
            if (inactivePlaceholder != null) inactivePlaceholder.SetActive(true);
            if (mainContentGroup != null)
            {
                mainContentGroup.alpha = 0f;
                mainContentGroup.interactable = false;
                mainContentGroup.blocksRaycasts = false;
            }
        }
        else
        {
            if (!playAnimation)
            {
                if (statusText != null) statusText.text = "状态：<color=#B9D9FF>激活</color>";
                if (accountDetailGroup != null) accountDetailGroup.alpha = 1f;
                if (inactivePlaceholder != null) inactivePlaceholder.SetActive(false);
                if (mainContentGroup != null)
                {
                    mainContentGroup.alpha = 1f;
                    mainContentGroup.interactable = true;
                    mainContentGroup.blocksRaycasts = true;
                }
            }
        }
    }

    /// <summary>
    /// 科幻终端风格的逐步激活流程
    /// </summary>
    private IEnumerator ActivationSequenceRoutine()
    {
        if (audioSource != null && activateSound != null)
        {
            audioSource.PlayOneShot(activateSound);
        }

        // 步骤 1: 状态文字乱码跳变 -> 变为“激活”
        if (statusText != null)
        {
            string[] glitchTexts = { "状态：@#$%", "状态：校验中..", "状态：PERM_OK", "状态：激活" };
            for (int i = 0; i < glitchTexts.Length; i++)
            {
                statusText.text = glitchTexts[i];
                yield return new WaitForSeconds(0.1f);
            }
            statusText.text = "状态：<color=#B9D9FF>激活</color>";
        }

        yield return new WaitForSeconds(0.15f);

        // 步骤 2: 左下角身份与编号逐项打字机/淡入显示
        if (accountDetailGroup != null)
        {
            float timer = 0f;
            while (timer < 0.25f)
            {
                timer += Time.deltaTime;
                accountDetailGroup.alpha = Mathf.Lerp(0f, 1f, timer / 0.25f);
                yield return null;
            }
            accountDetailGroup.alpha = 1f;
        }

        // 步骤 3: 移除未激活占位提示，启动右侧主面板
        if (inactivePlaceholder != null)
        {
            inactivePlaceholder.SetActive(false);
        }

        if (mainContentGroup != null)
        {
            // 主面板从淡出到点亮，模拟屏幕亮起
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                mainContentGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.4f);
                yield return null;
            }
            mainContentGroup.alpha = 1f;
            mainContentGroup.interactable = true;
            mainContentGroup.blocksRaycasts = true;
        }
    }
}