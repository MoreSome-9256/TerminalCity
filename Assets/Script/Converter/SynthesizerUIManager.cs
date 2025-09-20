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
        if (background != null) background.SetActive(true);
        backgroundFade?.Fade(true);

        if (window != null) window.SetActive(true);
        windowBehavior?.Move(true);

        text1Fade?.Fade(true);
        text2Fade?.Fade(true);

        if (clueWindow != null) clueWindow.SetActive(true);
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
        if (background != null) background.SetActive(false);

        if (clueWindow != null) clueWindow.SetActive(false);
    }
}
