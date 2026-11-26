using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> MakeOakTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        var randomness1 = new System.Random();

        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)((maxTrunkHeight - minTrunkHeight) * Noise.Get2DPerlin(new Vector2(position.x, position.z), new Vector2(250f, 250f), 2f)) + minTrunkHeight;

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 14));

        for (int x = -2; x < 3; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    if (y == 0 || y == 1)
                    {
                        if (Mathf.Abs(x) == 2 && Mathf.Abs(z) == 2 && randomness1.Next(0, 2) == 1)
                            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 13));
                        else if (x == 0 && z == 0)
                            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 14));
                        else if (Mathf.Abs(x) != 2 || Mathf.Abs(z) != 2)
                            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 13));
                    }
                    else if (y == 2)
                    {
                        if ((x >= -1 && x <= 1) && (z >= -1 && z <= 1))
                        {
                            if (x == 0 && z == 0)
                                queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 14));
                            else
                                queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 13));
                        }
                    }
                    else if (y == 3)
                    {
                        if (Mathf.Abs(x) != 2 && Mathf.Abs(z) != 2 && ((Mathf.Abs(x) == 1 && Mathf.Abs(z) == 1 && randomness1.Next(0, 4) == 1) || (x == 0 || z == 0)))
                            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 13));
                    }
                }
            }
        }

        return queue;
    }

    public static Queue<VoxelMod> MakeBirchTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        var randomness2 = new System.Random();

        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)((maxTrunkHeight - minTrunkHeight) * Noise.Get2DPerlin(new Vector2(position.x, position.z), new Vector2(-250f, -250f), 2f)) + minTrunkHeight;

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 66));

        for (int x = -2; x < 3; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    if (y == 0 || y == 1)
                    {
                        if (Mathf.Abs(x) == 2 && Mathf.Abs(z) == 2 && randomness2.Next(0, 2) == 1)
                            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 67));
                        else if (x == 0 && z == 0)
                            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 66));
                        else if (Mathf.Abs(x) != 2 || Mathf.Abs(z) != 2)
                            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 67));
                    }
                    else if (y == 2)
                    {
                        if ((x >= -1 && x <= 1) && (z >= -1 && z <= 1))
                        {
                            if (x == 0 && z == 0)
                                queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 66));
                            else
                                queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 67));
                        }
                    }
                    else if (y == 3)
                    {
                        if (Mathf.Abs(x) != 2 && Mathf.Abs(z) != 2 && ((Mathf.Abs(x) == 1 && Mathf.Abs(z) == 1 && randomness2.Next(0, 4) == 1) || (x == 0 || z == 0)))
                            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 67));
                    }
                }
            }
        }

        return queue;
    }
}