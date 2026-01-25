using UnityEngine;

public class UnlockCharacterEvent : MonoBehaviour
{
    [Header("要解锁的角色 ID")]
    public string characterId;

    public GameObject Level3;

    public void Execute()
    {
        Level3.SetActive(true);
        bool unlocked = PlayerDataManager.Instance
            .UnlockCharacter(characterId);
        Level3.SetActive(false);

        if (unlocked)
        {
            Level3UIController.Instance?.RefreshCharacterList();
        }
    }

}
