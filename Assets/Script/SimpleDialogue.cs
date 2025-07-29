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

    private void Awake()
    {
        nextButton.onClick.AddListener(NextDialogue);
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
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
        bool hasButton = TryFindButton();

        if (hasButton)
        {
            wasButtonInitiallyActive = targetButton.interactable;
            targetButton.interactable = false;
            targetButton.gameObject.SetActive(false);
        }
        if (triggerOnce && hasTriggered)
        {
            // 不再播放对话，但仍调用结束逻辑
            Debug.Log("Dialogue already triggered, skipping dialogue content.");
            EndDialogue();
            return;
        }

        Debug.Log("start dialogue");

        EventSystem.current.SetSelectedGameObject(null);

        charNameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
        characterImage?.gameObject.SetActive(true);
        if (background != null)
        {
            background.SetActive(true);
        }

        onDialogueStart?.Invoke();
        isDialogueActive = true;
        currentIndex = 0;
        ShowDialogueSegment(dialogueSequence[currentIndex]);

        hasTriggered = true;
    }


    private void ShowDialogueSegment(DialogueSegment segment)
    {
        // Set UI
        charNameText.text = segment.charName;
        characterImage.sprite = segment.characterSprite;

        // Start typing effect
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(segment.dialogueText));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in fullText)
        {
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
            // Fast forward typing
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogueSequence[currentIndex].dialogueText;
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
        charNameText.text = string.Empty;     // 清空角色名称
        dialogueText.text = string.Empty;     // 清空对话内容
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
}
