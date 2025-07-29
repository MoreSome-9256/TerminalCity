using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class ChoiceQuestion
{
    public string question;
    public List<string> options;
    public int correctIndex;

    public UnityEvent onCorrect;
    public UnityEvent onWrong;
}
