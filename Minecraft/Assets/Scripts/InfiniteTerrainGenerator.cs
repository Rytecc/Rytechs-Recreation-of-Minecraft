using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour
{

    private static Movement PlayerMovementScript;
    private static Queue<ChunkCoords> ChunksToGenerate = new Queue<ChunkCoords>();
    private static ChunkCoords LastGeneratedCoords;
    public static bool Init;

    private void Awake()
    {
        PlayerMovementScript = FindObjectOfType<Movement>();
    }

    private void LateUpdate()
    {
        if(Init && !WorldGenerator.IsGenerating)
        {
            Cycle(false);
        }
    }

    public static void Cycle(bool Force = false)
    {

        ChunksToGenerate.Clear(); 

        if (Force)
        {
            for (int x = PlayerMovementScript.CurrentCoord.x - 16; x < PlayerMovementScript.CurrentCoord.x + 16; x++)
            {
                for (int y = PlayerMovementScript.CurrentCoord.y - 16; y < PlayerMovementScript.CurrentCoord.y + 16; y++)
                {
                    ChunkCoords coord = new ChunkCoords(x, y);
                    if (WorldGenerator.ChunksInWorld.ContainsKey(coord))
                    {
                        WorldGenerator.ChunksInWorld[coord].UpdateChunkCull(PlayerMovementScript.CurrentCoord);
                    }
                    else
                    {
                        if (ChunkCoords.Distance(PlayerMovementScript.CurrentCoord, coord) <= WorldGenerator.InstancedGenerator.RenderDistance)
                        {
                            ChunksToGenerate.Enqueue(coord);
                        }
                    }
                }
            }

            return;
        }
        else
        {
            if(PlayerMovementScript.CurrentCoord.x != LastGeneratedCoords.x || PlayerMovementScript.CurrentCoord.y != LastGeneratedCoords.y)
            {
                for (int x = PlayerMovementScript.CurrentCoord.x - 16; x < PlayerMovementScript.CurrentCoord.x + 16; x++)
                {
                    for (int y = PlayerMovementScript.CurrentCoord.y - 16; y < PlayerMovementScript.CurrentCoord.y + 16; y++)
                    {
                        ChunkCoords coord = new ChunkCoords(x, y);
                        if (WorldGenerator.ChunksInWorld.ContainsKey(coord))
                        {
                            WorldGenerator.ChunksInWorld[coord].UpdateChunkCull(PlayerMovementScript.CurrentCoord);
                        }
                        else
                        {
                            if (ChunkCoords.Distance(PlayerMovementScript.CurrentCoord, coord) <= WorldGenerator.InstancedGenerator.RenderDistance)
                            {
                                ChunksToGenerate.Enqueue(coord);
                            }
                        }
                    }
                }
            }
        }


        WorldGenerator.InstancedGenerator.StartCoroutine(WorldGenerator.InstancedGenerator.CreateChunkBatch(ChunksToGenerate, null));
        LastGeneratedCoords = PlayerMovementScript.CurrentCoord;
    }
}
