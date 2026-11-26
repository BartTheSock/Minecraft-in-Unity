using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class World : MonoBehaviour
{
    [Header("World Generation Values")]
    public int seed;
    public Vector2 worldSeedOffset;
    public List<Vector2> treeSeedOffsets = new List<Vector2>();
    public List<Vector2> foliageSeedOffsets = new List<Vector2>();
    public List<int> lodeSeedOffsets = new List<int>();
    public Vector2 biomeTemperatureSeedOffset;
    public Vector2 biomeDownfallSeedOffset;
    public BiomeAttributes biome;
    public NoiseSettings noiseSettings;

    [Header("Performance")]
    public bool enableThreading;

    [Range(0f, 1f)]
    public float globalLightLevel;
    public Color day;
    public Color night;

    public Player player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public Material translucentMaterial;
    public Material grassMaterial;
    public Material foliageMaterial;
    public Material birchLeavesMaterial;
    public Material spruceLeavesMaterial;
    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoordinate> activeChunks = new List<ChunkCoordinate>();
    public ChunkCoordinate playerChunkCoordinate;
    ChunkCoordinate playerPreviousChunkCoordinate;

    List<ChunkCoordinate> chunksToCreate = new List<ChunkCoordinate>();
    List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private bool _inUI = false;

    public DebugScreen debugScreen;
    public GameObject creativeInventory;
    public GameObject cursorSlot;

    private MainGameControls gameControls;
    private InputAction debugScreenAction;
    private InputAction exitGame1Action;
    private InputAction exitGame2Action;
    private InputAction exitGame3Action;

    private void Awake()
    {
        gameControls = new MainGameControls();
    }

    private void Start()
    {
        Random.InitState(seed);
        worldSeedOffset = new Vector2(Random.Range(16, 65536), Random.Range(16, 65536));
        
        for (int i = 0; i < 4; i++)
        {
            treeSeedOffsets.Add(new Vector2(Random.Range(16, 65536), Random.Range(16, 65536)));
        }

        for (int i = 0; i < 10; i++)
        {
            foliageSeedOffsets.Add(new Vector2(Random.Range(16, 65536), Random.Range(16, 65536)));
        }

        for (int i = 0; i < 30; i++)
        {
            lodeSeedOffsets.Add(Random.Range(16, 65536));
        }

        biomeTemperatureSeedOffset = new Vector2(Random.Range(16, 65536), Random.Range(16, 65536));
        biomeDownfallSeedOffset = new Vector2(Random.Range(16, 65536), Random.Range(16, 65536));

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        GenerateWorld();
        CalculateSpawn();
        playerChunkCoordinate = GetChunkCoordinateFromVector3(player.transform.position);
        playerPreviousChunkCoordinate = GetChunkCoordinateFromVector3(player.transform.position);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        debugScreenAction = gameControls.Player.DebugScreen;
        exitGame1Action = gameControls.Player.ExitGame1;
        exitGame2Action = gameControls.Player.ExitGame2;
        exitGame3Action = gameControls.Player.ExitGame3;
        debugScreenAction.Enable();
        exitGame1Action.Enable();
        exitGame2Action.Enable();
        exitGame3Action.Enable();
    }

    private void OnDisable()
    {
        debugScreenAction.Disable();
        exitGame1Action.Disable();
        exitGame2Action.Disable();
        exitGame3Action.Disable();
    }

    void CalculateSpawn()
    {
        spawnPosition = new Vector3(((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f) + 0.5f, VoxelData.ChunkHeight - 10f, ((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f) + 0.5f);
        player.transform.position = spawnPosition;

        while (!CheckForVoxel(new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z)))
        {
            spawnPosition += new Vector3(0f, -1f, 0f);
        }

        spawnPosition += new Vector3(0f, 1f, 0f);
        player.transform.position = spawnPosition;
    }

    private void Update()
    {
        playerChunkCoordinate = GetChunkCoordinateFromVector3(player.transform.position);

        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);

        grassMaterial.SetColor("Tint", new Color(0.569f, 0.741f, 0.349f, 1f));
        foliageMaterial.SetColor("Tint", new Color(0.467f, 0.671f, 0.184f, 1f));
        birchLeavesMaterial.SetColor("Tint", new Color(0.5f, 0.655f, 0.333f, 1f));
        spruceLeavesMaterial.SetColor("Tint", new Color(0.38f, 0.6f, 0.38f, 1f));

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!playerChunkCoordinate.Equals(playerPreviousChunkCoordinate))
            CheckViewDistance();

        if (!applyingModifications)
            ApplyModifications();

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (chunksToUpdate.Count > 0)
            UpdateChunks();

        if (chunksToDraw.Count > 0)
            lock (chunksToDraw)
            {
                if (chunksToDraw.Peek().IsEditable)
                    chunksToDraw.Dequeue().CreateMesh();
            }

        if (debugScreenAction.WasPressedThisFrame())
            debugScreen.gameObject.SetActive(!debugScreen.gameObject.activeSelf);

        if ((exitGame1Action.IsPressed() && exitGame2Action.IsPressed()) || exitGame3Action.WasPressedThisFrame())
            Application.Quit();
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoordinate(x, z), this, true);
                activeChunks.Add(new ChunkCoordinate(x, z));
            }
        }
    }

    void CreateChunk()
    {
        ChunkCoordinate c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.z].Initialize();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        while (!updated && index < chunksToUpdate.Count - 1)
        {
            if (chunksToUpdate[index].IsEditable)
            {
                chunksToUpdate[index].UpdateChunk();
                chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
                index++;
        }
    }

    void ApplyModifications()
    {
        applyingModifications = true;

        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                ChunkCoordinate c = GetChunkCoordinateFromVector3(v.position);

                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this, true);
                    activeChunks.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);

                if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
                    chunksToUpdate.Add(chunks[c.x, c.z]);
            }
        }

        applyingModifications = false;
    }

    ChunkCoordinate GetChunkCoordinateFromVector3(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(position.z / VoxelData.ChunkWidth);
        return new ChunkCoordinate(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];

    }

    void CheckViewDistance()
    {
        ChunkCoordinate coordinate = GetChunkCoordinateFromVector3(player.transform.position);
        playerPreviousChunkCoordinate = playerChunkCoordinate;

        List<ChunkCoordinate> previouslyActiveChunks = new List<ChunkCoordinate>(activeChunks);

        // Loop through all chunks currently within view distance of the player.
        for (int x = coordinate.x - VoxelData.ViewDistanceInChunks; x < coordinate.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coordinate.z - VoxelData.ViewDistanceInChunks; z < coordinate.z + VoxelData.ViewDistanceInChunks; z++)
            {
                // If the current chunk is in the world...
                if (IsChunkInWorld(new ChunkCoordinate(x, z)))
                {

                    // Check if it active, if not, activate it.
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoordinate(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoordinate(x, z));
                    }
                    else if (!chunks[x, z].IsActive)
                        chunks[x, z].IsActive = true;
                    
                    activeChunks.Add(new ChunkCoordinate(x, z));
                }

                // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoordinate(x, z)))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (ChunkCoordinate chunk in previouslyActiveChunks)
            chunks[chunk.x, chunk.z].IsActive = false;
    }

    public bool CheckForVoxel(Vector3 position)
    {
        ChunkCoordinate thisChunk = new ChunkCoordinate(position);

        if (!IsChunkInWorld(thisChunk) || position.y < 0 || position.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(position).id].isSolid;

        return blockTypes[GetVoxel(position)].isSolid;
    }

    public VoxelState GetVoxelState(Vector3 position)
    {
        ChunkCoordinate thisChunk = new ChunkCoordinate(position);

        if (!IsChunkInWorld(thisChunk) || position.y < 0 || position.y > VoxelData.ChunkHeight)
            return null;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(position);

        return new VoxelState(GetVoxel(position));
    }
    /*
    public bool CheckVoxelTranslucency(Vector3 position)
    {
        ChunkCoordinate thisChunk = new ChunkCoordinate(position);

        if (!IsChunkInWorld(thisChunk) || position.y < 0 || position.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(position).id].isTransparent;

        return blockTypes[GetVoxel(position)].isTransparent;
    }

    public bool CheckVoxelLeaves(Vector3 position)
    {
        ChunkCoordinate thisChunk = new ChunkCoordinate(position);

        if (!IsChunkInWorld(thisChunk) || position.y < 0 || position.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(position).id].usesFoliageColors;

        return blockTypes[GetVoxel(position)].usesFoliageColors;
    }
    */
    public bool InUI
    {
        get { return _inUI; }

        set
        {
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                creativeInventory.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                creativeInventory.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
    }

    public ushort GetVoxel(Vector3 position)
    {
        int xPosition = Mathf.FloorToInt(position.x);
        int yPosition = Mathf.FloorToInt(position.y);
        int zPosition = Mathf.FloorToInt(position.z);

        /* IMMUTABLE PASS */

        // If outside world, return air.
        if (!IsVoxelInWorld(position))
            return 0;

        // If bottom block of chunk, return bedrock.
        if (yPosition == 0)
            return 6;

        /* BASIC TERRAIN PASS */

        float surfaceHeight = Noise.GetOctavePerlin(position.x, position.z, noiseSettings, worldSeedOffset);
        surfaceHeight = Noise.Redistribution(surfaceHeight, noiseSettings);
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * surfaceHeight) + biome.solidGroundHeight;
        ushort voxelValue = 0;

        if (yPosition == terrainHeight)
            voxelValue = 1;
        else if (yPosition < terrainHeight && yPosition > terrainHeight - 4)
            voxelValue = 2;
        else if (yPosition > terrainHeight)
            voxelValue = 0;
        else
            voxelValue = 3;

        /* SECOND PASS */

        int lodeIndex = 0;
        foreach (Lode lode in biome.lodes)
        {
            if (lode.blocksToChange.Contains(voxelValue))
            {
                if (yPosition > lode.minHeight && yPosition < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(position, lodeSeedOffsets[lodeIndex], lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
                }
            }
            lodeIndex++;
        }

        /* TREE PASS */

        if (yPosition == terrainHeight)
        {
            if (Noise.Get2DPerlin(new Vector2(position.x, position.z), new Vector2(treeSeedOffsets[0].x, treeSeedOffsets[0].y), biome.tree1ZoneScale) > biome.tree1ZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(position.x, position.z), new Vector2(treeSeedOffsets[0].x, treeSeedOffsets[0].y), biome.tree1PlacementScale) > biome.tree1PlacementThreshold)
                {
                    voxelValue = 2;
                    modifications.Enqueue(Structure.MakeOakTree(position, biome.minTree1Height, biome.maxTree1Height));
                }
            }

            if (Noise.Get2DPerlin(new Vector2(position.x, position.z), new Vector2(treeSeedOffsets[1].x, treeSeedOffsets[1].y), biome.tree2ZoneScale) > biome.tree2ZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(position.x, position.z), new Vector2(treeSeedOffsets[1].x, treeSeedOffsets[1].y), biome.tree2PlacementScale) > biome.tree2PlacementThreshold)
                {
                    voxelValue = 2;
                    modifications.Enqueue(Structure.MakeBirchTree(position, biome.minTree2Height, biome.maxTree2Height));
                }
            }
        }

        return voxelValue;
    }

    bool IsChunkInWorld(ChunkCoordinate coordinate)
    {
        if (coordinate.x > 0 && coordinate.x < VoxelData.WorldSizeInChunks - 1 && coordinate.z > 0 && coordinate.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return false;
    }

    bool IsVoxelInWorld(Vector3 position)
    {
        if (position.x >= 0 && position.x < VoxelData.WorldSizeInVoxels && position.y >= 0 && position.y < VoxelData.ChunkHeight && position.z >= 0 && position.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public ushort blockType;
    public bool isSolid;
    public bool renderNeighborFaces;
    public bool isTransparent;
    public float transparency;
    public bool usesGrassColors;
    public bool usesFoliageColors;
    public bool appearsInCreative;
    public Sprite icon;
    public int maxStackSize = 64;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public ushort id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}