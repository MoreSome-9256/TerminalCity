using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData")]
public class GameData : ScriptableObject
{
    public List<CharacterInfo> AllCharacters;
    public List<Relationship> AllRelationships;

    public bool HasRelationship(string nameA, string nameB)
    {
        foreach (var rel in AllRelationships)
        {
            // 不分先后顺序
            if ((rel.CharacterA == nameA && rel.CharacterB == nameB) ||
                (rel.CharacterA == nameB && rel.CharacterB == nameA))
            {
                return true;
            }
        }
        return false;
    }
}
