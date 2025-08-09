using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EdgeSegment : MonoBehaviour
{
    void Awake()
    {
        var line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPositions(new Vector3[] {
            new Vector3(-3f, 0f, 0f),
            new Vector3(3f, 0f, 0f)
        });
    }
}
