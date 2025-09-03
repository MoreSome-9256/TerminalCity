using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DictionaryEntryUI : MonoBehaviour
{
    public TMP_Text termText;
    public TMP_Text definitionText;

    public void Setup(string term, string definition, bool showDefinition)
    {
        termText.text = term;
        if (showDefinition)
        {
            definitionText.text = definition;
            definitionText.gameObject.SetActive(true);
        }
        else
        {
            definitionText.gameObject.SetActive(false);
        }
    }
}
