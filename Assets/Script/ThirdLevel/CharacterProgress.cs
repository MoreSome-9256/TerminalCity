using System.Collections.Generic;

[System.Serializable]
public class CharacterProgress
{
    public string characterId;
    public bool unlocked;
    public List<int> obtainedItemNums = new();

    public CharacterProgress(string id)
    {
        characterId = id;
        unlocked = false;
    }

    public bool HasCollectible(int collectibleId)
    {
        return obtainedItemNums.Contains(collectibleId);
    }

    public void AddCollectible(int collectibleId)
    {
        if (!obtainedItemNums.Contains(collectibleId))
            obtainedItemNums.Add(collectibleId);
    }
}
