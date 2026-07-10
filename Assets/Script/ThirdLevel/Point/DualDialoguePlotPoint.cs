using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DualDialoguePlotPoint : PlotPoint
{
    public enum Speaker
    {
        Left,
        Right
    }

    [System.Serializable]
    public class DualDialogueSegment
    {
        public Speaker speaker;
        public string charName;

        [TextArea(3, 10)]
        public string dialogueText;

        public Sprite leftSprite;
        public Sprite rightSprite;

        [Header("背景切换(可空)")]
        public Sprite backgroundSprite;  // 可空

        // ========================================================
        // ✨ 新增：多语言拦截属性（只读，不影响 Inspector 原有数据）
        // ========================================================
        public string LocalizedName
        {
            get
            {
                // 如果以后接入了多语言组件，这里可以用 charName 作为 Key 去查表
                // string key = $"char_{charName}";
                // return GetGlobalLocalizedText(key, charName);
                return charName; // 目前阶段：直接返回原有中文
            }
        }

        public string LocalizedText
        {
            get
            {
                // 因为 List 里没有唯一 ID，我们生成一个基于“名字+文本哈希”的临时 Key，或者直接用原始中文当 Key 查表
                // 推荐后期：直接拿原中文作为 Key 去本地化表里索引英文
                // return GetGlobalLocalizedText(dialogueText, dialogueText);
                return dialogueText; // 目前阶段：直接返回原有中文
            }
        }
        // 后期引入多语言时改用这个
        // public string LocalizedName => LocalizationHelper.GetText($"char_{charName}", charName);
        // public string LocalizedText => LocalizationHelper.GetText(dialogueText, dialogueText); // 用原中文当 Key
    }

    [Header("UI References")]
    [SerializeField] private TMP_Text charNameText;
    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private Image leftCharacterImage;
    [SerializeField] private Image rightCharacterImage;

    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject rootObject;   // 整个UI根物体
    [SerializeField] private Image backgroundImage;   // 背景Image

    [Header("Dialogue Data")]
    [SerializeField] private List<DualDialogueSegment> dialogueSequence;
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Visual Settings")]
    [SerializeField] private float dimBrightness = 0.5f;

    private int currentIndex = 0;
    private bool isTyping = false;
    private bool nextPressed = false;
    private Coroutine typingCoroutine;

    // ==========================
    // ⭐ 核心：PlotPoint 执行入口
    // ==========================
    public override IEnumerator Execute()
    {
        if (rootObject != null)
            rootObject.SetActive(true);

        nextButton.gameObject.SetActive(true);
        leftCharacterImage.gameObject.SetActive(true);
        rightCharacterImage.gameObject.SetActive(true);
        charNameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => nextPressed = true);

        currentIndex = 0;

        while (currentIndex < dialogueSequence.Count)
        {
            yield return PlaySegment(dialogueSequence[currentIndex]);
            currentIndex++;
        }

        Cleanup();
    }

    // ==========================
    // 播放单段对话
    // ==========================
    private IEnumerator PlaySegment(DualDialogueSegment segment)
    {
        nextPressed = false;

        // 名字
        charNameText.text = segment.LocalizedName;

        // 背景切换
        if (segment.backgroundSprite != null && backgroundImage != null)
        {
            backgroundImage.sprite = segment.backgroundSprite;
            backgroundImage.gameObject.SetActive(true);
        }
        // 更新立绘
        if (segment.leftSprite != null)
        {
            leftCharacterImage.sprite = segment.leftSprite;
            leftCharacterImage.gameObject.SetActive(true);
        }

        if (segment.rightSprite != null)
        {
            rightCharacterImage.sprite = segment.rightSprite;
            rightCharacterImage.gameObject.SetActive(true);
        }

        UpdateSpeakerVisual(segment.speaker);

        // 🔍 1. 将打字协程存下来，方便在按下空格时停止它
        typingCoroutine = StartCoroutine(TypeSentence(segment.LocalizedText));

        // 🔍 2. 改进等待输入的逻辑，兼容“打字中跳过”和“打完字翻页”
        while (true)
        {
            if (nextPressed || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    // 如果正在打字，按空格或点按钮改为“立刻显示全句”
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
                    isTyping = false;
                    nextPressed = false; // 重置标记，防止直接跳到下一句
                }
                else
                {
                    // 如果字已经打完了，按空格或点按钮才真正进入下一句
                    break;
                }
            }
            yield return null;
        }
    }

    // ==========================
    // 打字机效果
    // ==========================
    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;

        dialogueText.text = sentence;
        dialogueText.ForceMeshUpdate();

        int totalVisible = dialogueText.textInfo.characterCount;
        dialogueText.maxVisibleCharacters = 0;

        int visibleCount = 0;

        while (visibleCount < totalVisible)
        {
            visibleCount++;
            dialogueText.maxVisibleCharacters = visibleCount;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // ==========================
    // 立绘高亮
    // ==========================
    // ==========================
    // 立绘高亮（改亮度而不是透明度）
    // ==========================
    private void UpdateSpeakerVisual(Speaker speaker)
    {
        if (speaker == Speaker.Left)
        {
            SetBrightness(leftCharacterImage, 1f);
            SetBrightness(rightCharacterImage, dimBrightness);
        }
        else
        {
            SetBrightness(leftCharacterImage, dimBrightness);
            SetBrightness(rightCharacterImage, 1f);
        }
    }

    private void SetBrightness(Image img, float brightness)
    {
        Color c = img.color;

        c.r = brightness;
        c.g = brightness;
        c.b = brightness;
        c.a = 1f; // 始终保持不透明

        img.color = c;
    }

    // ==========================
    // 收尾
    // ==========================
    private void Cleanup()
    {
        charNameText.text = "";
        dialogueText.text = "";

        leftCharacterImage.gameObject.SetActive(false);
        rightCharacterImage.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        leftCharacterImage.gameObject.SetActive(false);
        rightCharacterImage.gameObject.SetActive(false);

        nextButton.onClick.RemoveAllListeners();

        if (rootObject != null)
            rootObject.SetActive(false);
    }
}
