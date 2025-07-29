using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class MultiChoiceQuiz : MonoBehaviour
{
    [Header("题目数据")]
    public List<ChoiceQuestion> questions;

    [Header("UI 绑定")]
    public TMP_Text questionText;
    public List<Button> optionButtons;
    public List<TMP_Text> optionTexts;

    private int currentQuestionIndex = 0;
    private bool answered = false;

    void Start()
    {
        ShowQuestion(currentQuestionIndex);
    }

    void ShowQuestion(int index)
    {
        answered = false;

        if (index >= questions.Count)
        {
            Debug.Log("所有题目已完成！");
            return;
        }

        var q = questions[index];
        questionText.text = q.question;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < q.options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionTexts[i].text = q.options[i];
                int capturedIndex = i; // 闭包保护
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(capturedIndex));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnOptionSelected(int selectedIndex)
    {
        if (answered) return;
        answered = true;

        var q = questions[currentQuestionIndex];

        if (selectedIndex == q.correctIndex)
        {
            Debug.Log("答对了！");
            q.onCorrect?.Invoke();
            // 答对后 1 秒进入下一题
            Invoke(nameof(NextQuestion), 1f);
        }
        else
        {
            Debug.Log("答错了！");
            q.onWrong?.Invoke();
            // 答错不切题，让玩家可以重新答
            answered = false; // ← 允许再次选择
        }
    }


    public void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.Log("问答全部结束");
        }
    }
}
