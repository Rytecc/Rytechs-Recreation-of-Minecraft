using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldStructureGenerator : MonoBehaviour
{
    public static Queue<Structure> StructuresToBuild = new Queue<Structure>();

    public List<TerrainChunk> QueueVisualization = new List<TerrainChunk>();

    private void Start()
    {
        WorldGenerator.InstancedGenerator.OnChunkGenerated += CreateStructures;
    }

    private async void CreateStructures(ChunkCoords Coords)
    {
        List<TerrainChunk> ChunksToUpdateTemp = new List<TerrainChunk>();
        Task StructuresThread = Task.Factory.StartNew
        (
            delegate
            {
                for (int i = 0; i < StructuresToBuild.Count; i++)
                {
                    Structure _struct = StructuresToBuild.Dequeue();
                    foreach (BlockInfo b in _struct.BlocksToPlace)
                    {
                        //Get the local coords of the block and check if it is the same
                        //as the given coords of the chunk,
                        //if it is equal then just index and change the blocktype,
                        //if not within the chunk, check if the supposed neighboring chunk exists,
                        //if it exists change and update the chunk,
                        //if it doesn't exist, create additive data for the given coord.

                        WorldGenerator.GetLocalBlockPosition(b.Pos + _struct.RootPos, out ChunkCoords LocalCoords, out Vector3Int LocalPos);

                        //is the block within the generatedchunk?
                        if(LocalCoords.x == Coords.x && LocalCoords.y == Coords.y)
                        {
                            WorldGenerator.ChunksInWorld[LocalCoords].Data[LocalPos.x, LocalPos.y, LocalPos.z].BlockType = b.BlockType;
                        }
                        else
                        {
                            if(WorldGenerator.ChunksInWorld.ContainsKey(LocalCoords))
                            {
                                WorldGenerator.ChunksInWorld[LocalCoords].Data[LocalPos.x, LocalPos.y, LocalPos.z].BlockType = b.BlockType;
                                if (!ChunksToUpdateTemp.Contains(WorldGenerator.ChunksInWorld[LocalCoords]))
                                {
                                    ChunksToUpdateTemp.Add(WorldGenerator.ChunksInWorld[LocalCoords]);
                                }
                            }
                            else
                            {
                                if(WorldGenerator.AdditiveData.ContainsKey(LocalCoords))
                                {
                                    WorldGenerator.AdditiveData[LocalCoords][LocalPos.x, LocalPos.y, LocalPos.z].BlockType = b.BlockType;
                                    continue;
                                }

                                Block[,,] NewAdditiveData = new Block[16, 256, 16];
                                NewAdditiveData[LocalPos.x, LocalPos.y, LocalPos.z].BlockType = b.BlockType;
                                WorldGenerator.AdditiveData.Add(LocalCoords, NewAdditiveData);
                            }

                        }
                    }
                }
            }
        );

        await StructuresThread;

        if(ChunksToUpdateTemp.Count > 0)
        {
            for(int i = 0; i < ChunksToUpdateTemp.Count; i++)
            {
                ChunksToUpdateTemp[i].UpdateChunk();
            }
        }

        WorldGenerator.InstancedGenerator.GenerationCompleted = true;
    }
}
