using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3SequenceController : MonoBehaviour
{
    [Header("序列中的所有剧情点（PlotPoint）")]
    [SerializeField] private List<PlotPoint> plotPoints;

    public void Play()
    {
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        foreach (var point in plotPoints)
        {
            if (point != null)
                yield return point.Play();
        }

        OnSequenceFinished();
    }

    private void OnSequenceFinished()
    {
        Debug.Log("Sequence finished!");
        Destroy(gameObject);
    }
}
