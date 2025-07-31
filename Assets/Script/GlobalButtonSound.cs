using UnityEngine;
using UnityEngine.UI;

public class GlobalButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    public float volume = 0.5f;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        GlobalSoundManager.Instance.PlayClick(clickSound, volume);
    }
}
