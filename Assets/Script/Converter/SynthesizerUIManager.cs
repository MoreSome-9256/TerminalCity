using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SynthesizerUIManager : MonoBehaviour
{
    public static SynthesizerUIManager Instance;

    [Header("UI References")]
    public GameObject background;
    public ObjectFadeController backgroundFade;

    public GameObject window;
    public WindowBehavior windowBehavior;

    public ObjectFadeController text1Fade;
    public ObjectFadeController text2Fade;

    public GameObject clueWindow;
    [Header("全局 MoveController 列表")]
    public List<MoveController> moveControllers = new List<MoveController>();

    [Header("二级/三级模式切换时使用")]
    public List<MoveController> modeSwitchControllers = new List<MoveController>();

    public MoveController switchController;

    [Header("背包切换")]
    public MoveController level2BagController;
    public MoveController level3BagController;
    private float bagSwitchDelay = 0.5f; // 背包切换间隔时间
    private Coroutine bagSwitchCoroutine;

    [Header("三级合成核心面板")]
    public GameObject synthesisPanel;

    public enum SynthesisMode
    {
        Level2, // 二级资料合成
        Level3  // 三级资料合成
    }
    public SynthesisMode currentMode = SynthesisMode.Level2;

    public RectTransform windowsRootLevel2;
    public RectTransform windowsRootLevel3;
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
    // prefab 调用这个注册它的按钮
    public void RegisterSynthButton(Button button)
    {
        foreach (var controller in moveControllers)
        {
            controller.RegisterOpenButton(button);
        }
    }
    /// <summary>
    /// 打开合成器界面
    /// </summary>
    public void Open()
    {
        /*if (background != null) background.SetActive(true);
        backgroundFade?.Fade(true);

        if (window != null) window.SetActive(true);
        windowBehavior?.Move(true);

        text1Fade?.Fade(true);
        text2Fade?.Fade(true);

        if (clueWindow != null) clueWindow.SetActive(true);*/
        OpenMode(currentMode);
    }

    /// <summary>
    /// 关闭合成器界面
    /// </summary>
    public void Close()
    {
        text1Fade?.Fade(false);
        text2Fade?.Fade(false);

        windowBehavior?.Move(false);
        if (window != null) window.SetActive(false);

        backgroundFade?.Fade(false);
        //if (background != null) background.SetActive(false);

        if (clueWindow != null) clueWindow.SetActive(false);
    }
    /// <summary>
    /// 在二级 / 三级合成模式之间切换
    /// </summary>
    public void ToggleSynthesisMode()
    {
        if (currentMode == SynthesisMode.Level2)
            SetMode(SynthesisMode.Level3);
        else
            SetMode(SynthesisMode.Level2);
    }

    private void SetMode(SynthesisMode mode)
    {
        if (currentMode == mode) return;

        currentMode = mode;
        ApplyMode(mode);
    }

    private void ApplyMode(SynthesisMode mode)
    {
        bool isLevel2 = (mode == SynthesisMode.Level2);

        // 1. 模式相关 UI
        foreach (var c in modeSwitchControllers)
        {
            if (c != null)
                c.StartMove(isLevel2);
        }

        // 2. 背包切换
        SwitchBags(isLevel2);

        // 3. 窗口与文字
        if (isLevel2)
        {
            if (window != null) window.SetActive(true);
            windowBehavior?.Move(true);

            if (text1Fade != null)
            {
                text1Fade.gameObject.SetActive(true);
                text1Fade.Fade(true);
            }

            if (text2Fade != null)
            {
                text2Fade.gameObject.SetActive(true);
                text2Fade.Fade(true);
            }

            if (clueWindow != null) clueWindow.SetActive(true);

            if (synthesisPanel != null)
                synthesisPanel.SetActive(false);
        }
        else
        {
            text1Fade?.Fade(false);
            text2Fade?.Fade(false);

            windowBehavior?.Move(false);
            if (window != null) window.SetActive(false);

            //if (clueWindow != null) clueWindow.SetActive(false);

            if (synthesisPanel != null)
                synthesisPanel.SetActive(true);
        }
    }
    private void OpenMode(SynthesisMode mode)
    {
        //Debug.Log(mode);
        if (background != null) background.SetActive(true);
        backgroundFade?.Fade(true);
        bool isLevel2 = (mode == SynthesisMode.Level2);
        switchController.StartMove(true);
        foreach (var c in modeSwitchControllers)
        {
            if (c != null)
                c.StartMove(isLevel2);
        }
        if (isLevel2)
        {
            if (window != null) window.SetActive(true);
            windowBehavior?.Move(true);

            if (text1Fade != null)
            {
                text1Fade.gameObject.SetActive(true);
                text1Fade.Fade(true);
            }

            if (text2Fade != null)
            {
                text2Fade.gameObject.SetActive(true);
                text2Fade.Fade(true);
            }

            if (clueWindow != null) clueWindow.SetActive(true);

            if (synthesisPanel != null)
                synthesisPanel.SetActive(false);
        }
        else
        {
            level3BagController.StartMove(true);
            if (window != null) window.SetActive(false);
            //windowBehavior?.Move(false);

            if (clueWindow != null) clueWindow.SetActive(false);

            if (synthesisPanel != null)
                synthesisPanel.SetActive(true);
        }
    }
    private void SwitchBags(bool isLevel2)
    {
        if (bagSwitchCoroutine != null)
            StopCoroutine(bagSwitchCoroutine);

        bagSwitchCoroutine = StartCoroutine(SwitchBagsCoroutine(isLevel2));
    }
    private IEnumerator SwitchBagsCoroutine(bool isLevel2)
    {
        // 先让当前背包移出
        if (isLevel2)
        {
            // 要切到二级模式：先移出三级背包
            if (level3BagController != null)
                level3BagController.StartMove(false);
        }
        else
        {
            // 要切到三级模式：先移出二级背包
            if (level2BagController != null)
                level2BagController.StartMove(false);
        }

        // 等待一段时间（可以等动画时长）
        yield return new WaitForSeconds(bagSwitchDelay);

        // 再让目标背包移入
        if (isLevel2)
        {
            if (level2BagController != null)
                level2BagController.StartMove(true);
        }
        else
        {
            if (level3BagController != null)
                level3BagController.StartMove(true);
        }

        bagSwitchCoroutine = null;
    }
}
