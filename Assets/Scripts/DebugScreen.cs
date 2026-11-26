using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GameObject.Find("DebugText").GetComponent<Text>();

        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    void Update()
    {
        string debugText = "Minecraft in Unity, made by BartTheSock using resources from Mojang and tutorials by b3agz and Jadon Unity.";
        debugText += "\n\n";
        debugText += $"{frameRate} FPS";
        debugText += "\n";
        debugText += $"Position X: {Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels} / Y: {Mathf.FloorToInt(world.player.transform.position.y - 64)} / Z: {Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels}";
        debugText += "\n";
        debugText += $"Chunk: {world.playerChunkCoordinate.x - halfWorldSizeInChunks} / {world.playerChunkCoordinate.z - halfWorldSizeInChunks}";
        debugText += "\n";
        string directionString = "I have no idea";
        float yDirection = ((world.player.transform.rotation.eulerAngles.y + 180f) % 360f) - 180f;
        if ((yDirection >= -180f && yDirection < -157.5f) || (yDirection < 180f && yDirection >= 157.5f))
            directionString = "North";
        else if (yDirection >= -157.5f && yDirection < -112.5f)
            directionString = "Northeast";
        else if (yDirection >= -112.5f && yDirection < -67.5f)
            directionString = "East";
        else if (yDirection >= -67.5f && yDirection < -22.5f)
            directionString = "Southeast";
        else if (yDirection >= -22.5f && yDirection < 22.5f)
            directionString = "South";
        else if (yDirection >= 22.5f && yDirection < 67.5f)
            directionString = "Southwest";
        else if (yDirection >= 67.5f && yDirection < 112.5f)
            directionString = "West";
        else if (yDirection >= 112.5f && yDirection < 157.5f)
            directionString = "Northwest";
        var playerCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        float xDirection = ((-playerCamera.transform.rotation.eulerAngles.x - 180f) % 360f) + 180f;
        debugText += $"Rotation X: {yDirection.ToString("0.0")} / Y: {xDirection.ToString("0.0")}";
        debugText += "\n";
        debugText += $"Facing {directionString}.";

        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
            timer += Time.deltaTime;
    }
}