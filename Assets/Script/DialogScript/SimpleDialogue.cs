using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SimpleDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueSegment
    {
        public string charName;
        [TextArea(3, 10)]
        public string dialogueText;
        public Sprite characterSprite;

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
    [SerializeField] private Image characterImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject background;

    [Header("Dialogue Configuration")]
    [SerializeField] private List<DialogueSegment> dialogueSequence;
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    private bool isDialogueActive = false;
    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    [SerializeField] private bool triggerOnce = false;
    private bool hasTriggered = false;

    [Header("UI Control")]
    [SerializeField] private Button targetButton;
    private bool wasButtonInitiallyActive;
    private bool hasSearchedButton;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            PlayClickSound();
            NextDialogue();
        }
    }
    private bool TryFindButton()
    {
        if (targetButton != null) return true;
        if (hasSearchedButton) return false; // 避免重复查找

        // 尝试按路径查找
        Transform buttonTransform = GameObject.Find("GlobalUI")?
                                    .transform.Find("Canvas/sideScreen/Button");

        if (buttonTransform != null)
        {
            targetButton = buttonTransform.GetComponent<Button>();
            //Debug.Log("自动查找到按钮: " + targetButton.name);
        }

        hasSearchedButton = true;
        return targetButton != null;
    }
    public void StartDialogue()
    {
        // 0. 确保自身 GameObject 是激活的（防止协程无法启动）
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        bool hasButton = TryFindButton();
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            PlayClickSound();
            NextDialogue();
        });
        if (hasButton)
        {
            wasButtonInitiallyActive = targetButton.interactable;
            targetButton.interactable = false;
            targetButton.gameObject.SetActive(false);
        }
        if (triggerOnce && hasTriggered)
        {
            Debug.Log("Dialogue already triggered, skipping dialogue content.");
            EndDialogue();
            return;
        }

        Debug.Log("start dialogue");

        EventSystem.current.SetSelectedGameObject(null);

        // ✨ 核心修复 1：把对话框的父级节点拉起来（防止被 DialogueManager 随手关掉）
        if (charNameText != null && charNameText.transform.parent != null)
        {
            charNameText.transform.parent.gameObject.SetActive(true);
        }

        // ✨ 核心修复 2：如果背景有单独的父节点或自身，一并确保开启
        if (background != null)
        {
            background.SetActive(true);
        }

        // 激活子 UI
        if (charNameText != null) charNameText.gameObject.SetActive(true);
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
        if (nextButton != null) nextButton.gameObject.SetActive(true);
        if (characterImage != null) characterImage.gameObject.SetActive(true);

        onDialogueStart?.Invoke();
        isDialogueActive = true;
        currentIndex = 0;
        ShowDialogueSegment(dialogueSequence[currentIndex]);

        hasTriggered = true;
    }

    private void ShowDialogueSegment(DialogueSegment segment)
    {
        // Set UI
        charNameText.text = segment.LocalizedName;
        characterImage.sprite = segment.characterSprite;

        // Start typing effect
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(segment.LocalizedText));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;

        // 1. 一次性填入全部文本，让 TMP 瞬间计算好所有的换行、自动撑开高度
        dialogueText.text = fullText;
        dialogueText.maxVisibleCharacters = 0; // 初始全隐身

        // 2. 强制更新网格以解析富文本，获取准确的真实字符总数
        dialogueText.ForceMeshUpdate();
        int totalVisibleCharacters = dialogueText.textInfo.characterCount;

        // 3. 通过增加可见数量来实现打字效果
        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void NextDialogue()
    {
        if (!isDialogueActive)
            return;

        if (isTyping)
        {
            // 玩家按下空格，快进打字效果
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogueSequence[currentIndex].LocalizedText;
            dialogueText.maxVisibleCharacters = 99999; // 🌟 瞬间显示全部字符
            isTyping = false;
        }
        else
        {
            currentIndex++;
            if (currentIndex < dialogueSequence.Count)
            {
                ShowDialogueSegment(dialogueSequence[currentIndex]);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void EndDialogue()
    {
        if (targetButton != null && wasButtonInitiallyActive)
        {
            targetButton.interactable = true;
            targetButton.gameObject.SetActive(true);
        }
        nextButton.onClick.RemoveAllListeners();

        dialogueText.maxVisibleCharacters = 99999; // 🌟 恢复默认状态
        charNameText.text = string.Empty;
        dialogueText.text = string.Empty;

        charNameText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        characterImage?.gameObject.SetActive(false);
        if (background != null)
        {
            background.SetActive(false);
        }
        isDialogueActive = false;
        onDialogueEnd?.Invoke();
    }
    private void PlayClickSound()
    {
        if (!audioSource.enabled || !audioSource.gameObject.activeInHierarchy)
            return;
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
