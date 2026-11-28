using System.Collections;
using System.Collections.Generic;
//using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;


public class Chunk
{
    public ChunkCoordinate coordinate;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshRenderer grassSideMeshRenderer;
    MeshFilter grassSideMeshFilter;
    MeshRenderer leavesMeshRenderer;
    MeshFilter leavesMeshFilter;
    MeshRenderer birchLeavesMeshRenderer;
    MeshFilter birchLeavesMeshFilter;
    MeshRenderer spruceLeavesMeshRenderer;
    MeshFilter spruceLeavesMeshFilter;
    GameObject chunkObject;
    GameObject grassSide;
    GameObject leaves;
    GameObject birchLeaves;
    GameObject spruceLeaves;

    int vertexIndex = 0;
    int grassSideVertexIndex = 0;
    int leavesVertexIndex = 0;
    int birchLeavesVertexIndex = 0;
    int spruceLeavesVertexIndex = 0;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();
    
    List<Vector3> grassSideVertices = new List<Vector3>();
    List<int> grassSideTriangles = new List<int>();
    List<Vector2> grassSideUvs = new List<Vector2>();
    List<Color> grassSideColors = new List<Color>();
    
    List<Vector3> leavesVertices = new List<Vector3>();
    List<int> leavesTriangles = new List<int>();
    List<Vector2> leavesUvs = new List<Vector2>();
    List<Color> leavesColors = new List<Color>();
    
    List<Vector3> birchLeavesVertices = new List<Vector3>();
    List<int> birchLeavesTriangles = new List<int>();
    List<Vector2> birchLeavesUvs = new List<Vector2>();
    List<Color> birchLeavesColors = new List<Color>();
    
    List<Vector3> spruceLeavesVertices = new List<Vector3>();
    List<int> spruceLeavesTriangles = new List<int>();
    List<Vector2> spruceLeavesUvs = new List<Vector2>();
    List<Color> spruceLeavesColors = new List<Color>();

    public Vector3 Position;

    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    World world;

    private bool _isActive;
    private bool isVoxelMapPopulated = false;

    public Chunk(ChunkCoordinate _coordinate, World _world)
    {
        coordinate = _coordinate;
        world = _world;
    }

    public void Initialize()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        grassSide = new GameObject();
        grassSide.transform.SetParent(chunkObject.transform);
        grassSideMeshFilter = grassSide.AddComponent<MeshFilter>();
        grassSideMeshRenderer = grassSide.AddComponent<MeshRenderer>();

        leaves = new GameObject();
        leaves.transform.SetParent(chunkObject.transform);
        leavesMeshFilter = leaves.AddComponent<MeshFilter>();
        leavesMeshRenderer = leaves.AddComponent<MeshRenderer>();

        birchLeaves = new GameObject();
        birchLeaves.transform.SetParent(chunkObject.transform);
        birchLeavesMeshFilter = birchLeaves.AddComponent<MeshFilter>();
        birchLeavesMeshRenderer = birchLeaves.AddComponent<MeshRenderer>();

        spruceLeaves = new GameObject();
        spruceLeaves.transform.SetParent(chunkObject.transform);
        spruceLeavesMeshFilter = spruceLeaves.AddComponent<MeshFilter>();
        spruceLeavesMeshRenderer = spruceLeaves.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        grassSideMeshRenderer.material = world.grassMaterial;
        leavesMeshRenderer.material = world.foliageMaterial;
        birchLeavesMeshRenderer.material = world.birchLeavesMaterial;
        spruceLeavesMeshRenderer.material = world.spruceLeavesMaterial;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coordinate.x * VoxelData.ChunkWidth, 0f, coordinate.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk " + coordinate.x + ", " + coordinate.z;
        Position = chunkObject.transform.position;

        PopulateVoxelMap();
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + Position));
                }
            }
        }

        isVoxelMapPopulated = true;

        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Add(this);
        }
    }

    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position -= Position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = v.id;
        }

        ClearMeshData();
        CalculateLight();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }

        world.chunksToDraw.Enqueue(this);
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();

        grassSideVertexIndex = 0;
        grassSideVertices.Clear();
        grassSideTriangles.Clear();
        grassSideUvs.Clear();
        grassSideColors.Clear();
        
        leavesVertexIndex = 0;
        leavesVertices.Clear();
        leavesTriangles.Clear();
        leavesUvs.Clear();
        leavesColors.Clear();
        
        birchLeavesVertexIndex = 0;
        birchLeavesVertices.Clear();
        birchLeavesTriangles.Clear();
        birchLeavesUvs.Clear();
        birchLeavesColors.Clear();
        
        spruceLeavesVertexIndex = 0;
        spruceLeavesVertices.Clear();
        spruceLeavesTriangles.Clear();
        spruceLeavesUvs.Clear();
        spruceLeavesColors.Clear();
    }

    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public bool IsEditable
    {
        get
        {
            if (!isVoxelMapPopulated)
                return false;
            else
                return true;
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;
    }

    public void EditVoxel(Vector3 position, ushort newID)
    {
        int xCheck = Mathf.FloorToInt(position.x);
        int yCheck = Mathf.FloorToInt(position.y);
        int zCheck = Mathf.FloorToInt(position.z);
        
        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck].id = newID;

        lock (world.ChunkUpdateThreadLock)
        {
            world.chunksToUpdate.Insert(0, this);
            UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        }

    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
                world.chunksToUpdate.Insert(0, world.GetChunkFromVector3(currentVoxel + Position));
        }
    }

    VoxelState CheckVoxel(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.GetVoxelState(position + Position);

        return voxelMap[x, y, z];
    }
    /*
    VoxelState CheckVoxelTranslucent(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.CheckVoxelTranslucency(position + Position);

        return voxelMap[x, y, z];
    }

    VoxelState CheckVoxelLeaves(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.CheckVoxelLeaves(position + Position);

        return voxelMap[x, y, z];
    }
    */
    public VoxelState GetVoxelFromGlobalVector3(Vector3 position)
    {
        int xCheck = Mathf.FloorToInt(position.x);
        int yCheck = Mathf.FloorToInt(position.y);
        int zCheck = Mathf.FloorToInt(position.z);

        xCheck -= Mathf.FloorToInt(Position.x);
        zCheck -= Mathf.FloorToInt(Position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    void UpdateMeshData(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        ushort blockID = voxelMap[x, y, z].id;

        // bool isTransparent = world.blockTypes[blockID].renderNeighborFaces;
        // bool isTranslucent = world.blockTypes[blockID].isTransparent;
        // bool isGrass = world.blockTypes[blockID].usesGrassColors;
        // bool isFoliage = world.blockTypes[blockID].usesFoliageColors;

        for (int p = 0; p < 6; p++)
        {
            VoxelState neighbor = CheckVoxel(position + VoxelData.faceChecks[p]);

            if (neighbor != null && (world.blockTypes[neighbor.id].renderNeighborFaces /*|| world.blockTypes[neighbor.id].usesFoliageColors || !world.blockTypes[neighbor.id].isTransparent */))
            {
                float tintLevel;
                float lightLevel;

                if (world.blockTypes[blockID].usesGrassColors && (world.blockTypes[blockID].GetTextureID(p) == 0 || world.blockTypes[blockID].GetTextureID(p) == 1))
                {
                    if (world.blockTypes[blockID].GetTextureID(p) == 1)
                    {
                        vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 0]]);
                        vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 1]]);
                        vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 2]]);
                        vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 3]]);
                        AddTexture(world.blockTypes[blockID].GetTextureID(p));

                        tintLevel = AddTint(p);
                        lightLevel = Mathf.Lerp(neighbor.globalLightPercent, 0, tintLevel);

                        colors.Add(new Color(0, 0, 0, lightLevel));
                        colors.Add(new Color(0, 0, 0, lightLevel));
                        colors.Add(new Color(0, 0, 0, lightLevel));
                        colors.Add(new Color(0, 0, 0, lightLevel));

                        /*
                        if (isTranslucent)
                        {
                            translucentTriangles.Add(vertexIndex);
                            translucentTriangles.Add(vertexIndex + 1);
                            translucentTriangles.Add(vertexIndex + 2);
                            translucentTriangles.Add(vertexIndex + 2);
                            translucentTriangles.Add(vertexIndex + 1);
                            translucentTriangles.Add(vertexIndex + 3);
                        }
                        else if (isTransparent)
                        {
                            transparentTriangles.Add(vertexIndex);
                            transparentTriangles.Add(vertexIndex + 1);
                            transparentTriangles.Add(vertexIndex + 2);
                            transparentTriangles.Add(vertexIndex + 2);
                            transparentTriangles.Add(vertexIndex + 1);
                            transparentTriangles.Add(vertexIndex + 3);
                        }
                        else if (isFoliage)
                        {
                            foliageTriangles.Add(vertexIndex);
                            foliageTriangles.Add(vertexIndex + 1);
                            foliageTriangles.Add(vertexIndex + 2);
                            foliageTriangles.Add(vertexIndex + 2);
                            foliageTriangles.Add(vertexIndex + 1);
                            foliageTriangles.Add(vertexIndex + 3);
                        }
                        else if (world.blockTypes[blockID].GetTextureID(p) == 0)
                        {
                            grassTriangles.Add(vertexIndex);
                            grassTriangles.Add(vertexIndex + 1);
                            grassTriangles.Add(vertexIndex + 2);
                            grassTriangles.Add(vertexIndex + 2);
                            grassTriangles.Add(vertexIndex + 1);
                            grassTriangles.Add(vertexIndex + 3);
                        }
                        */

                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 3);
                        vertexIndex += 4;
                    }
                    
                    grassSideVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 0]]);
                    grassSideVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 1]]);
                    grassSideVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 2]]);
                    grassSideVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 3]]);

                    if (world.blockTypes[blockID].GetTextureID(p) == 0)
                        AddGrassTexture(0);
                    else if (world.blockTypes[blockID].GetTextureID(p) == 1)
                        AddGrassTexture(2);

                    tintLevel = AddTint(p);
                    lightLevel = Mathf.Lerp(neighbor.globalLightPercent, 0, tintLevel);

                    grassSideColors.Add(new Color(0, 0, 0, lightLevel));
                    grassSideColors.Add(new Color(0, 0, 0, lightLevel));
                    grassSideColors.Add(new Color(0, 0, 0, lightLevel));
                    grassSideColors.Add(new Color(0, 0, 0, lightLevel));

                    grassSideTriangles.Add(grassSideVertexIndex);
                    grassSideTriangles.Add(grassSideVertexIndex + 1);
                    grassSideTriangles.Add(grassSideVertexIndex + 2);
                    grassSideTriangles.Add(grassSideVertexIndex + 2);
                    grassSideTriangles.Add(grassSideVertexIndex + 1);
                    grassSideTriangles.Add(grassSideVertexIndex + 3);
                    grassSideVertexIndex += 4;
                }
                else if (world.blockTypes[blockID].GetTextureID(p) == 112)
                {
                    birchLeavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 0]]);
                    birchLeavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 1]]);
                    birchLeavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 2]]);
                    birchLeavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 3]]);

                    AddBirchLeavesTexture(112);

                    tintLevel = AddTint(p);
                    lightLevel = Mathf.Lerp(neighbor.globalLightPercent, 0, tintLevel);

                    birchLeavesColors.Add(new Color(0, 0, 0, lightLevel));
                    birchLeavesColors.Add(new Color(0, 0, 0, lightLevel));
                    birchLeavesColors.Add(new Color(0, 0, 0, lightLevel));
                    birchLeavesColors.Add(new Color(0, 0, 0, lightLevel));

                    birchLeavesTriangles.Add(birchLeavesVertexIndex);
                    birchLeavesTriangles.Add(birchLeavesVertexIndex + 1);
                    birchLeavesTriangles.Add(birchLeavesVertexIndex + 2);
                    birchLeavesTriangles.Add(birchLeavesVertexIndex + 2);
                    birchLeavesTriangles.Add(birchLeavesVertexIndex + 1);
                    birchLeavesTriangles.Add(birchLeavesVertexIndex + 3);
                    birchLeavesVertexIndex += 4;
                }
                else if (world.blockTypes[blockID].GetTextureID(p) == 115)
                {
                    spruceLeavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 0]]);
                    spruceLeavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 1]]);
                    spruceLeavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 2]]);
                    spruceLeavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 3]]);

                    AddSpruceLeavesTexture(115);

                    tintLevel = AddTint(p);
                    lightLevel = Mathf.Lerp(neighbor.globalLightPercent, 0, tintLevel);

                    spruceLeavesColors.Add(new Color(0, 0, 0, lightLevel));
                    spruceLeavesColors.Add(new Color(0, 0, 0, lightLevel));
                    spruceLeavesColors.Add(new Color(0, 0, 0, lightLevel));
                    spruceLeavesColors.Add(new Color(0, 0, 0, lightLevel));

                    spruceLeavesTriangles.Add(spruceLeavesVertexIndex);
                    spruceLeavesTriangles.Add(spruceLeavesVertexIndex + 1);
                    spruceLeavesTriangles.Add(spruceLeavesVertexIndex + 2);
                    spruceLeavesTriangles.Add(spruceLeavesVertexIndex + 2);
                    spruceLeavesTriangles.Add(spruceLeavesVertexIndex + 1);
                    spruceLeavesTriangles.Add(spruceLeavesVertexIndex + 3);
                    spruceLeavesVertexIndex += 4;
                }
                else if (world.blockTypes[blockID].usesFoliageColors)
                {
                    leavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 0]]);
                    leavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 1]]);
                    leavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 2]]);
                    leavesVertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 3]]);

                    AddLeavesTexture(world.blockTypes[blockID].GetTextureID(p));

                    tintLevel = AddTint(p);
                    lightLevel = Mathf.Lerp(neighbor.globalLightPercent, 0, tintLevel);

                    leavesColors.Add(new Color(0, 0, 0, lightLevel));
                    leavesColors.Add(new Color(0, 0, 0, lightLevel));
                    leavesColors.Add(new Color(0, 0, 0, lightLevel));
                    leavesColors.Add(new Color(0, 0, 0, lightLevel));

                    leavesTriangles.Add(leavesVertexIndex);
                    leavesTriangles.Add(leavesVertexIndex + 1);
                    leavesTriangles.Add(leavesVertexIndex + 2);
                    leavesTriangles.Add(leavesVertexIndex + 2);
                    leavesTriangles.Add(leavesVertexIndex + 1);
                    leavesTriangles.Add(leavesVertexIndex + 3);
                    leavesVertexIndex += 4;
                }
                else
                {
                    vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 0]]);
                    vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 1]]);
                    vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 2]]);
                    vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[p, 3]]);

                    AddTexture(world.blockTypes[blockID].GetTextureID(p));

                    tintLevel = AddTint(p);
                    lightLevel = Mathf.Lerp(neighbor.globalLightPercent, 0, tintLevel);

                    colors.Add(new Color(0, 0, 0, lightLevel));
                    colors.Add(new Color(0, 0, 0, lightLevel));
                    colors.Add(new Color(0, 0, 0, lightLevel));
                    colors.Add(new Color(0, 0, 0, lightLevel));

                    /*
                    if (isTranslucent)
                    {
                        translucentTriangles.Add(vertexIndex);
                        translucentTriangles.Add(vertexIndex + 1);
                        translucentTriangles.Add(vertexIndex + 2);
                        translucentTriangles.Add(vertexIndex + 2);
                        translucentTriangles.Add(vertexIndex + 1);
                        translucentTriangles.Add(vertexIndex + 3);
                    }
                    else if (isTransparent)
                    {
                        transparentTriangles.Add(vertexIndex);
                        transparentTriangles.Add(vertexIndex + 1);
                        transparentTriangles.Add(vertexIndex + 2);
                        transparentTriangles.Add(vertexIndex + 2);
                        transparentTriangles.Add(vertexIndex + 1);
                        transparentTriangles.Add(vertexIndex + 3);
                    }
                    else if (isFoliage)
                    {
                        foliageTriangles.Add(vertexIndex);
                        foliageTriangles.Add(vertexIndex + 1);
                        foliageTriangles.Add(vertexIndex + 2);
                        foliageTriangles.Add(vertexIndex + 2);
                        foliageTriangles.Add(vertexIndex + 1);
                        foliageTriangles.Add(vertexIndex + 3);
                    }
                    else if (world.blockTypes[blockID].GetTextureID(p) == 0)
                    {
                        grassTriangles.Add(vertexIndex);
                        grassTriangles.Add(vertexIndex + 1);
                        grassTriangles.Add(vertexIndex + 2);
                        grassTriangles.Add(vertexIndex + 2);
                        grassTriangles.Add(vertexIndex + 1);
                        grassTriangles.Add(vertexIndex + 3);
                    }
                    */

                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                    vertexIndex += 4;
                }
            }
        }
    }

    void CalculateLight()
    {
        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();

        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkWidth; z++)
            {
                float lightRay = 1f;

                for (int y = VoxelData.ChunkHeight - 1; y >= 0; y--)
                {
                    VoxelState thisVoxel = voxelMap[x, y, z];

                    if (thisVoxel.id > 0 && world.blockTypes[thisVoxel.id].transparency < lightRay)
                        lightRay = world.blockTypes[thisVoxel.id].transparency;

                    thisVoxel.globalLightPercent = lightRay;

                    voxelMap[x, y, z] = thisVoxel;

                    if (lightRay > VoxelData.lightFalloff)
                        litVoxels.Enqueue(new Vector3Int(x, y, z));
                }
            }
        }
        
        while (litVoxels.Count > 0)
        {
            Vector3Int v = litVoxels.Dequeue();

            for (int p = 0; p < 6; p++)
            {
                Vector3 currentVoxel = v + VoxelData.faceChecks[p];
                Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

                if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
                {
                    if (voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent < voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFalloff)
                    {
                        voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent = voxelMap[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFalloff;

                        if (voxelMap[neighbor.x, neighbor.y, neighbor.z].globalLightPercent > VoxelData.lightFalloff)
                            litVoxels.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    float AddTint(int faceIndex)
    {
        if (faceIndex == 0 || faceIndex == 1)
            return 0.3f;
        else if (faceIndex == 4 || faceIndex == 5)
            return 0.55f;
        else if (faceIndex == 3)
            return 0.7f;
        else
            return 0f;
    }

    public void CreateMesh()
    {
        Mesh mesh1 = new Mesh();
        mesh1.vertices = vertices.ToArray();

        // mesh1.subMeshCount = 5;
        // mesh1.SetTriangles(triangles.ToArray(), 0);
        // mesh1.SetTriangles(transparentTriangles.ToArray(), 1);
        // mesh1.SetTriangles(translucentTriangles.ToArray(), 2);
        // mesh1.SetTriangles(grassTriangles.ToArray(), 3);
        // mesh1.SetTriangles(foliageTriangles.ToArray(), 4);

        mesh1.triangles = triangles.ToArray();
        mesh1.uv = uvs.ToArray();
        mesh1.colors = colors.ToArray();
        mesh1.RecalculateNormals();
        meshFilter.mesh = mesh1;

        Mesh mesh2 = new Mesh();
        mesh2.vertices = grassSideVertices.ToArray();

        mesh2.triangles = grassSideTriangles.ToArray();
        mesh2.uv = grassSideUvs.ToArray();
        mesh2.colors = grassSideColors.ToArray();
        mesh2.RecalculateNormals();
        grassSideMeshFilter.mesh = mesh2;

        Mesh mesh3 = new Mesh();
        mesh3.vertices = leavesVertices.ToArray();

        mesh3.triangles = leavesTriangles.ToArray();
        mesh3.uv = leavesUvs.ToArray();
        mesh3.colors = leavesColors.ToArray();
        mesh3.RecalculateNormals();
        leavesMeshFilter.mesh = mesh3;

        Mesh mesh4 = new Mesh();
        mesh4.vertices = birchLeavesVertices.ToArray();

        mesh4.triangles = birchLeavesTriangles.ToArray();
        mesh4.uv = birchLeavesUvs.ToArray();
        mesh4.colors = birchLeavesColors.ToArray();
        mesh4.RecalculateNormals();
        birchLeavesMeshFilter.mesh = mesh4;

        Mesh mesh5 = new Mesh();
        mesh5.vertices = spruceLeavesVertices.ToArray();

        mesh5.triangles = spruceLeavesTriangles.ToArray();
        mesh5.uv = spruceLeavesUvs.ToArray();
        mesh5.colors = spruceLeavesColors.ToArray();
        mesh5.RecalculateNormals();
        spruceLeavesMeshFilter.mesh = mesh5;
    }

    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    void AddGrassTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        grassSideUvs.Add(new Vector2(x, y));
        grassSideUvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        grassSideUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        grassSideUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    void AddLeavesTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        leavesUvs.Add(new Vector2(x, y));
        leavesUvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        leavesUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        leavesUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    void AddBirchLeavesTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        birchLeavesUvs.Add(new Vector2(x, y));
        birchLeavesUvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        birchLeavesUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        birchLeavesUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    void AddSpruceLeavesTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        spruceLeavesUvs.Add(new Vector2(x, y));
        spruceLeavesUvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        spruceLeavesUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        spruceLeavesUvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}

public class ChunkCoordinate
{
    public int x;
    public int z;

    public ChunkCoordinate()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoordinate(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoordinate(Vector3 position)
    {
        int xCheck = Mathf.FloorToInt(position.x);
        int zCheck = Mathf.FloorToInt(position.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoordinate other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;
    }
}

public class VoxelState
{
    public ushort id;
    public float globalLightPercent;

    public VoxelState()
    {
        id = 0;
        globalLightPercent = 0f;
    }

    public VoxelState(ushort _id)
    {
        id = _id;
        globalLightPercent = 0f;
    }
}