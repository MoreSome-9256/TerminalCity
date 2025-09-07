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
        if (charNameText != null) charNameText.text = segment.charName;
        if (characterImage != null) characterImage.sprite = segment.characterSprite;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(segment.dialogueText));
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
        dialogueUI.SetActive(false);
        if (nextButton != null) nextButton.onClick.RemoveAllListeners();

        if (charNameText != null) charNameText.text = string.Empty;
        if (dialogueText != null) dialogueText.text = string.Empty;

        if (charNameText != null) charNameText.gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (characterImage != null) characterImage.gameObject.SetActive(false);
        if (background != null) background.SetActive(false);

        isDialogueActive = false;
        onDialogueEnd?.Invoke();

        if (RoomManager.Instance != null && RoomManager.Instance.currentRoomID == 0)
        {
            if (UIManager.Instance.selectionUI != null)
                UIManager.Instance.selectionUI.SetActive(true);
        }
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
