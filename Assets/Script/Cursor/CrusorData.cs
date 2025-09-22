using UnityEngine;

[CreateAssetMenu(fileName = "CursorData", menuName = "Cursor/CursorData")]
public class CursorData : ScriptableObject
{
    public Texture2D texture;
    public Vector2 hotspot;
}
