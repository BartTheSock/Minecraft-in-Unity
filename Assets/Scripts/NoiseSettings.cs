using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "noiseSettings", menuName = "Minecraft/NoiseSettings")]
public class NoiseSettings : ScriptableObject
{
    public float noiseZoom;
    public int octaves;
    public Vector2Int offset;
    public float persistance;
    public float redistributionModifier;
    public float exponent;
}