using UnityEngine;
using System.Collections;

public class Level3TestRunner : MonoBehaviour
{
    [SerializeField] private Level3SequenceController sequenceController;

    void Start()
    {
        if (sequenceController == null)
        {
            Debug.LogError("Level3TestRunner: sequenceController not assigned!");
            return;
        }

        // ÷±Ω”»√ SequenceController ≤•∑≈
        sequenceController.Play();
    }
}
