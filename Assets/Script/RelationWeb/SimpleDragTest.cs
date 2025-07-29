using UnityEngine;

public class SimpleDragTest : MonoBehaviour
{
    public RectTransform graphRoot;

    private Vector2 lastMousePos;
    private bool dragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && dragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
            graphRoot.anchoredPosition += delta;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }
    }
}
