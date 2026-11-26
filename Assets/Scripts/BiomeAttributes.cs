using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;

    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    [Header("Trees 1")]
    public float tree1ZoneScale = 1.3f;
    [Range(0.1f, 1f)]
    public float tree1ZoneThreshold = 0.6f;
    public float tree1PlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float tree1PlacementThreshold = 0.8f;
    public int maxTree1Height = 4;
    public int minTree1Height = 2;

    [Header("Trees 2")]
    public float tree2ZoneScale = 1.3f;
    [Range(0.1f, 1f)]
    public float tree2ZoneThreshold = 0.6f;
    public float tree2PlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float tree2PlacementThreshold = 0.8f;
    public int maxTree2Height = 5;
    public int minTree2Height = 3;

    public Foliage[] foliage;
    public Lode[] lodes;
}

[System.Serializable]
public class Foliage
{
    public string foliageName;
    public ushort blockID;
    public float zoneScale;
    [Range(0.1f, 1f)]
    public float zoneThreshold;
    public float placementScale;
    [Range(0.1f, 1f)]
    public float placementThreshold;
}

[System.Serializable]
public class Lode
{
    public string lodeName;
    public ushort blockID;
    public List<ushort> blocksToChange;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
}