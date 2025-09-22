using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CursorData))]
public class CursorDataEditor : Editor
{
    private const float PreviewSize = 128f;

    public override void OnInspectorGUI()
    {
        CursorData data = (CursorData)target;

        // 贴图字段
        data.texture = (Texture2D)EditorGUILayout.ObjectField("Texture", data.texture, typeof(Texture2D), false);

        if (data.texture == null) return;

        // hotspot 输入框
        data.hotspot = EditorGUILayout.Vector2Field("Hotspot", data.hotspot);

        // 预览区域
        Rect previewRect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.ExpandWidth(false));
        EditorGUI.DrawPreviewTexture(previewRect, data.texture);

        // 缩放比例
        float scaleX = PreviewSize / (float)data.texture.width;
        float scaleY = PreviewSize / (float)data.texture.height;

        // hotspot 在 GUI 坐标系下的位置
        Vector2 hotspotPos = new Vector2(
            previewRect.x + data.hotspot.x * scaleX,
            previewRect.y + data.hotspot.y * scaleY
        );

        // 红色十字
        Handles.BeginGUI();
        Handles.color = Color.red;
        float crossSize = 5f;
        Handles.DrawLine(new Vector3(hotspotPos.x - crossSize, hotspotPos.y),
                         new Vector3(hotspotPos.x + crossSize, hotspotPos.y));
        Handles.DrawLine(new Vector3(hotspotPos.x, hotspotPos.y - crossSize),
                         new Vector3(hotspotPos.x, hotspotPos.y + crossSize));
        Handles.EndGUI();

        // 拖拽区域（透明按钮）
        Rect dragRect = new Rect(hotspotPos.x - 6, hotspotPos.y - 6, 12, 12);
        EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.MoveArrow);

        if (Event.current.type == EventType.MouseDrag && dragRect.Contains(Event.current.mousePosition))
        {
            Undo.RecordObject(data, "Move Hotspot");
            data.hotspot = new Vector2(
                (Event.current.mousePosition.x - previewRect.x) / scaleX,
                (Event.current.mousePosition.y - previewRect.y) / scaleY
            );
            EditorUtility.SetDirty(data);
            Repaint();
        }
    }
}
