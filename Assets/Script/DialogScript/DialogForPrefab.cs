using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DialogForPrefab : MonoBehaviour
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

    [Header("Dialogue Data")]
    [SerializeField] private List<DialogueSegment> dialogueSequence;
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    private GameObject dialogueUI;
    private TMP_Text charNameText;
    private TMP_Text dialogueText;
    private Image characterImage;
    private Button nextButton;
    private GameObject background;
    private GameObject selectionUI;

    private bool isDialogueActive = false;
    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    [SerializeField] private bool triggerOnce = false;
    private bool hasTriggered = false;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 自动从 UIManager 获取全局 UI
        if (UIManager.Instance != null)
        {
            dialogueUI = UIManager.Instance.dialogueUI;
            charNameText = UIManager.Instance.charNameText;
            dialogueText = UIManager.Instance.dialogueText;
            characterImage = UIManager.Instance.characterImage;
            nextButton = UIManager.Instance.nextButton;
            background = UIManager.Instance.dialogueBackground;
            selectionUI = UIManager.Instance.selectionUI;
        }
        else
        {
            Debug.LogError("DialogForPrefab: UIManager.Instance 为空，无法获取全局 UI！");
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

    public void StartDialogue()
    {
        if (triggerOnce && hasTriggered)
        {
            EndDialogue();
            return;
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                PlayClickSound();
                NextDialogue();
            });
        }

        EventSystem.current.SetSelectedGameObject(null);

        // 启用 UI
        selectionUI.SetActive(false);
        dialogueUI.SetActive(true);
        if (charNameText != null) charNameText.gameObject.SetActive(true);
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
        if (nextButton != null) nextButton.gameObject.SetActive(true);
        if (characterImage != null) characterImage.gameObject.SetActive(true);
        if (background != null) background.SetActive(true);

        onDialogueStart?.Invoke();
        isDialogueActive = true;
        currentIndex = 0;
        ShowDialogueSegment(dialogueSequence[currentIndex]);

        hasTriggered = true;
    }

    private void ShowDialogueSegment(DialogueSegment segment)
    {
        if (charNameText != null) charNameText.text = segment.LocalizedName;
        if (characterImage != null) characterImage.sprite = segment.characterSprite;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(segment.LocalizedText));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        foreach (char letter in fullText)
        {
            if (dialogueText != null)
                dialogueText.text += letter;
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
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (dialogueText != null)
                dialogueText.text = dialogueSequence[currentIndex].LocalizedText;
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
        isDialogueActive = false;

        if (nextButton != null) nextButton.onClick.RemoveAllListeners();
        if (charNameText != null) charNameText.text = string.Empty;
        if (dialogueText != null) dialogueText.text = string.Empty;

        // 建议：如果后面还要无缝衔接 DialogueManager，这里不要直接关掉父节点 dialogueUI
        // 而是让负责真正退出对话的逻辑/最后一个脚本去关，或者只隐藏内容：
        if (charNameText != null) charNameText.gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (characterImage != null) characterImage.gameObject.SetActive(false);
        if (background != null) background.SetActive(false);

        // 如果确定整套对话彻底结束了才关 dialogueUI：
        // dialogueUI.SetActive(false); 

        if (RoomManager.Instance != null && RoomManager.Instance.currentRoomID == 0)
        {
            if (UIManager.Instance.selectionUI != null)
                UIManager.Instance.selectionUI.SetActive(true);
        }

        // 务必放在最后触发：确保当前对话数据已清理干净，下一个 Manager 启动时不会被打架
        onDialogueEnd?.Invoke();
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
