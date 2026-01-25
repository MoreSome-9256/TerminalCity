using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public List<CharacterProgress> characters = new();

    public void InitEmpty()
    {
        characters.Clear();
    }

    public CharacterProgress GetCharacter(string characterId)
    {
        return characters.Find(c => c.characterId == characterId);
    }

    public CharacterProgress GetOrCreateCharacter(string characterId)
    {
        var cp = GetCharacter(characterId);
        if (cp == null)
        {
            cp = new CharacterProgress(characterId);
            characters.Add(cp);
        }
        return cp;
    }
}
