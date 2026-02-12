using System.Collections;
using UnityEngine;

public class FrameBreakPlotPoint : PlotPoint
{
    [Tooltip("µÈ´ýÖ¡Êý")]
    public int waitFrames = 1;

    public override IEnumerator Execute()
    {
        for (int i = 0; i < waitFrames; i++)
        {
            yield return null;
        }
    }
}
