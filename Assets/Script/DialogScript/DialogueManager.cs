using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueSegment
    {
        public string charName;
        [TextArea(3, 10)]
        public string dialogueText;
        public Sprite characterSprite;

        [Header("背景切换(可空)")]
        public Sprite backgroundSprite;

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

    public GameObject thisObject;

    [Header("UI References")]
    [SerializeField] private Image backgroundImage;      // 当前背景
    [SerializeField] private TMP_Text charNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button nextButton;

    [Header("Dialogue Configuration")]
    [SerializeField] public List<DialogueSegment> dialogueSequence;
    [SerializeField] public float typingSpeed = 0.05f;

    [Header("背景切换")]
    [SerializeField] private Image backgroundTransition; // 可选的过渡层
    [SerializeField] private float backgroundFadeDuration = 1f;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    [Header("Delay Settings")]
    [SerializeField] public float startDelay = 2f;
    [SerializeField] public float endDelay = 0f;

    [Header("UI Control")]
    [SerializeField] private Button targetButton;
    private bool wasButtonInitiallyActive;
    private bool hasSearchedButton;

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private bool isDialogueActive = false;
    private bool dialogueEnded = false;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float clickVolume = 0.3f;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        charNameText.gameObject.SetActive(true);
        backgroundImage.gameObject.SetActive(true);
        if(backgroundTransition != null)
        {
            backgroundTransition.gameObject.SetActive(true);
        }
        dialogueText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
        characterImage.gameObject.SetActive(true);

        StartCoroutine(StartDialogueWithDelay());
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            PlayClickSound();
            AdvanceDialogue();
        }
    }

    private bool TryFindButton()
    {
        if (targetButton != null) return true;
        if (hasSearchedButton) return false;

        Transform buttonTransform = GameObject.Find("GlobalUI")?
                                    .transform.Find("Canvas/sideScreen/Button");

        if (buttonTransform != null)
        {
            targetButton = buttonTransform.GetComponent<Button>();
        }

        hasSearchedButton = true;
        return targetButton != null;
    }

    public void OnButtonClick()
    {
        PlayClickSound();
        AdvanceDialogue();  // 直接调用
    }

    IEnumerator StartDialogueWithDelay()
    {
        isDialogueActive = false;
        yield return new WaitForSeconds(startDelay);
        isDialogueActive = true;
        StartDialogue();
    }

    public void StartDialogue()
    {
        // 1. 确保父节点开启（无论谁关了 DialogUI，这里都能拉起来）
        if (charNameText != null && charNameText.transform.parent != null)
        {
            // 自动激活父级 (即 DialogUI)
            charNameText.transform.parent.gameObject.SetActive(true);
        }

        // 2. 确保子 UI 开启
        if (charNameText != null) charNameText.gameObject.SetActive(true);
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
        if (backgroundImage != null) backgroundImage.gameObject.SetActive(true);
        if (nextButton != null) nextButton.gameObject.SetActive(true);
        if (backgroundTransition != null) backgroundTransition.gameObject.SetActive(true);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnButtonClick);
        dialogueEnded = false;

        bool hasButton = TryFindButton();
        if (hasButton)
        {
            wasButtonInitiallyActive = targetButton.interactable;
            targetButton.interactable = false;
            targetButton.gameObject.SetActive(false);
        }

        if (dialogueSequence.Count == 0) return;

        currentIndex = 0;
        UpdateDialogueUI();
        onDialogueStart?.Invoke();
    }

    private void UpdateDialogueUI()
    {
        // 切换角色名、立绘等
        charNameText.text = dialogueSequence[currentIndex].LocalizedName;

        if (dialogueSequence[currentIndex].characterSprite != null)
        {
            characterImage.sprite = dialogueSequence[currentIndex].characterSprite;
            characterImage.gameObject.SetActive(true);
        }
        else
        {
            characterImage.gameObject.SetActive(false);
        }

        // --- 背景切换 ---
        Sprite nextBackground = dialogueSequence[currentIndex].backgroundSprite;
        if (nextBackground != null && backgroundImage != null)
        {
            if (backgroundTransition != null)
            {
                StartCoroutine(FadeBackground(nextBackground));
            }
            else
            {
                // 如果没绑过渡层，就直接替换
                backgroundImage.sprite = nextBackground;
            }
        }

        // 打字机效果
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }
        else
        {
            isTyping = false;
        }

        typingCoroutine = StartCoroutine(TypeSentence(dialogueSequence[currentIndex].LocalizedText));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        int visibleCharacters = 0;
        bool insideTag = false;

        while (visibleCharacters < sentence.Length)
        {
            char currentChar = sentence[visibleCharacters];

            if (currentChar == '<') insideTag = true;
            else if (currentChar == '>') insideTag = false;

            visibleCharacters++;
            dialogueText.text = sentence.Substring(0, visibleCharacters);

            if (!insideTag)
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        isTyping = false;
    }

    public void AdvanceDialogue()
    {
        if (dialogueEnded) return;

        if (HandleTypingOrCompleteCurrent()) return;

        currentIndex++;

        if (currentIndex < dialogueSequence.Count)
        {
            UpdateDialogueUI();
        }
        else
        {
            dialogueEnded = true;
            onDialogueEnd?.Invoke();
            StartCoroutine(EndAfterDelay());
        }
    }

    private bool HandleTypingOrCompleteCurrent()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            dialogueText.text = dialogueSequence[currentIndex].LocalizedText;
            isTyping = false;
            return true;
        }
        return false;
    }

    IEnumerator EndAfterDelay()
    {
        yield return new WaitForSeconds(endDelay);
        EndDialogue();
    }

    private void EndDialogue()
    {
        if (targetButton != null && wasButtonInitiallyActive)
        {
            targetButton.interactable = true;
            targetButton.gameObject.SetActive(true);
        }
        nextButton.onClick.RemoveAllListeners();
        charNameText.text = string.Empty;
        dialogueText.text = string.Empty;

        backgroundImage.gameObject.SetActive(false);
        if (backgroundTransition != null)
        {
            backgroundTransition.gameObject.SetActive(false);
        }
        charNameText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        characterImage.gameObject.SetActive(false);
        thisObject.gameObject.SetActive(false);
    }

    public void RestartDialogue()
    {
        dialogueEnded = false;

        backgroundImage.gameObject.SetActive(true);
        if (backgroundTransition != null)
        {
            backgroundTransition.gameObject.SetActive(true);
        }
        charNameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);

        StartDialogue();
    }

    private void PlayClickSound()
    {
        if (!audioSource.enabled || !audioSource.gameObject.activeInHierarchy)
            return;
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, clickVolume);
        }
    }

    // 背景渐变协程
    private IEnumerator FadeBackground(Sprite newSprite)
    {
        if (backgroundTransition == null)
        {
            // 如果没设置过渡层，直接切换
            backgroundImage.sprite = newSprite;
            yield break;
        }

        backgroundTransition.gameObject.SetActive(true);
        backgroundTransition.sprite = newSprite;

        // 从透明到不透明
        Color color = backgroundTransition.color;
        color.a = 0f;
        backgroundTransition.color = color;

        float elapsed = 0f;
        while (elapsed < backgroundFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / backgroundFadeDuration;
            color.a = Mathf.Clamp01(t);
            backgroundTransition.color = color;
            yield return null;
        }

        // 完成后将主背景替换
        backgroundImage.sprite = newSprite;

        // 过渡层隐藏
        backgroundTransition.gameObject.SetActive(false);
    }
}
