using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainVisual : MonoBehaviour
{
    public Renderer rend;

    public void SetColor(Color color)
    {
        rend.material.color = color;
    }
}