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
    }

    public GameObject thisObject;

    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text charNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button nextButton;

    [Header("Dialogue Configuration")]
    [SerializeField] private List<DialogueSegment> dialogueSequence;
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    [Header("Delay Settings")]
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float endDelay = 0f;

    [Header("UI Control")]
    [SerializeField] private Button targetButton;
    private bool wasButtonInitiallyActive;
    private bool hasSearchedButton;

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private bool isDialogueActive = false;
    private bool dialogueEnded = false;

    void Start()
    {
        charNameText.gameObject.SetActive(true);
        backgroundImage.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
        characterImage.gameObject.SetActive(true);

        nextButton.onClick.AddListener(OnButtonClick);
        StartCoroutine(StartDialogueWithDelay());
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
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
        charNameText.text = dialogueSequence[currentIndex].charName;

        if (dialogueSequence[currentIndex].characterSprite != null)
        {
            characterImage.sprite = dialogueSequence[currentIndex].characterSprite;
            characterImage.gameObject.SetActive(true);
        }
        else
        {
            characterImage.gameObject.SetActive(false);
        }

        if (typingCoroutine != null)
        {
            //Debug.Log("typingCoroutine != null");
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }
        else
        {
            //Debug.Log("typingCoroutine == null");
            isTyping = false;
        }

        typingCoroutine = StartCoroutine(TypeSentence(dialogueSequence[currentIndex].dialogueText));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        //Debug.Log("TypeSentence start:" + isTyping);
        dialogueText.text = "";
        int visibleCharacters = 0;
        bool insideTag = false;

        while (visibleCharacters < sentence.Length)
        {
            char currentChar = sentence[visibleCharacters];

            if (currentChar == '<')
            {
                insideTag = true;
            }
            else if (currentChar == '>')
            {
                insideTag = false;
            }

            visibleCharacters++;
            dialogueText.text = sentence.Substring(0, visibleCharacters);

            if (!insideTag)
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        isTyping = false;
        //Debug.Log("TypeSentence end:" + isTyping);
    }

    public void AdvanceDialogue()
    {
        if (dialogueEnded) return;

        // 如果是打字中，只补全当前句子
        if (HandleTypingOrCompleteCurrent())
            return;

        // 否则推进对话
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
            dialogueText.text = dialogueSequence[currentIndex].dialogueText;
            isTyping = false;
            //Debug.Log("HandleTypingOrCompleteCurrent");
            return true;  // 表示是补全操作，不需要推进句子
        }

        return false;  // 表示不是打字状态，需要推进句子
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

        charNameText.text = string.Empty;
        dialogueText.text = string.Empty;

        backgroundImage.gameObject.SetActive(false);
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
        charNameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);

        StartDialogue();
    }
}
