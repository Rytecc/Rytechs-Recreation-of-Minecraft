using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class TickManager : MonoBehaviour
{

    public static Dictionary<Vector3Int, OnTick> DynamicBlocksList = new Dictionary<Vector3Int, OnTick>();
    private List<TerrainChunk> ChunksToUpdate = new List<TerrainChunk>();

    public static List<int> DynamicBlocks = new List<int>
    {
        13
    };

    private void Start()
    {
        ItemEvents.Instance.BlockBreakEvent += DeAllocateBlock;
        StartCoroutine(RunTicks());
    }

    public static void TryAddDynamicBlock(Vector3Int Position, OnTick tickClass, int BlockID, object TickParams)
    {
        if(DynamicBlocks.Contains(BlockID))
        {
            DynamicBlocksList.Add(Position, tickClass);
            DynamicBlocksList[Position].Init(TickParams);
        }
        else
        {
            Debug.Log($"Block Registry for {BlockID} failed");
        }
    }

    public static void RemoveDynamicBlock(Vector3Int Position, OnTick tickClass, int BlockID)
    {
        if(DynamicBlocksList.ContainsKey(Position))
        {
            DynamicBlocksList.Remove(Position);
        }
    }

    public void DeAllocateBlock(int ID, Vector3Int Position)
    {
        if(DynamicBlocksList.ContainsKey(Position))
        {
            DynamicBlocksList.Remove(Position);
        }
    }

    // 50000 FixedUpdates equate to 1 second.
    private IEnumerator RunTicks()
    {
        WaitForFixedUpdate fixedUpdateWait = new WaitForFixedUpdate();

        while (true)
        {
            Task t = Task.Factory.StartNew
            (
                delegate
                {
                    lock (DynamicBlocksList)
                    {
                        foreach (KeyValuePair<Vector3Int, OnTick> TickClass in DynamicBlocksList)
                        {
                            if (TickClass.Value.IsInitialized())
                            {
                                TerrainChunk tchunk = TickClass.Value.Tick();

                                if (tchunk != null)
                                {
                                    if (!ChunksToUpdate.Contains(tchunk)) ChunksToUpdate.Add(tchunk);
                                }
                            }
                        }
                    }
                }
            );

            yield return new WaitUntil(() => t.IsCompleted == true || t.IsFaulted == true || t.IsCanceled == true);
            foreach (TerrainChunk tc in ChunksToUpdate) { tc.UpdateChunk(); }
            ChunksToUpdate.Clear();

            yield return fixedUpdateWait;
        }
    }

}
