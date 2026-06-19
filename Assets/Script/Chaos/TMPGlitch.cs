using TMPro;
using UnityEngine;

public class TMPGlitch : MonoBehaviour
{
    TMP_Text text;

    public float intensity;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (intensity <= 0) return;

        text.ForceMeshUpdate();

        var mesh = text.mesh;
        var verts = mesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += Random.insideUnitSphere * intensity;
        }

        mesh.vertices = verts;
        text.canvasRenderer.SetMesh(mesh);
    }
}