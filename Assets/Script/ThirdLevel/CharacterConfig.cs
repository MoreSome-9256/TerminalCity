using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/CharacterConfig")]
public class CharacterConfig : ScriptableObject
{
    public string characterId;
    public string displayName;

    public Sprite silhouetteSprite;
    public Sprite portraitSprite;

    public List<Level3Data> collectibles;
}
