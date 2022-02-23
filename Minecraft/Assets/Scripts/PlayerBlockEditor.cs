using UnityEngine;
using static ItemManager;

public class PlayerBlockEditor : MonoBehaviour
{

    public Vector3Int WorldBlockPos;
    public Vector3Int LocalBlockPos;

    public Vector3Int mWorldBlockPos;
    public Vector3Int mLocalBlockPos;

    [SerializeField]
    private float PlayerReach;

    [SerializeField]
    private LayerMask ChunkMask;

    [SerializeField]
    private LayerMask BlocksMask;

    [SerializeField]
    private Animator AnimController;

    [SerializeField]
    private ParticleSystem BreakEffect;

    [SerializeField]
    private MeshRenderer BlockBreakOverlay;

    [SerializeField]
    private HotbarNavigator HotBarManager;

    [SerializeField]
    private float PlacementCooldown = 0.1f;

    [SerializeField]
    private UIManager UIHandler;

    public float BreakSpeed = 4;
    public int BlockToPlace;

    RaycastHit hitinf;
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            AnimController.SetBool("Interacting", true);
            BreakBlocks = true;
            InteractFunction();
            return;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            // Place a block without a delay
            if (Physics.Raycast(transform.position, transform.forward, out hitinf, PlayerReach, ChunkMask))
            {

                Vector3 Tgt = hitinf.point + hitinf.normal * -.1f;
                WorldBlockPos.x = Mathf.RoundToInt(Tgt.x);
                WorldBlockPos.y = Mathf.RoundToInt(Tgt.y);
                WorldBlockPos.z = Mathf.RoundToInt(Tgt.z);

                TerrainChunk chunk = WorldGenerator.GetChunk(WorldBlockPos);

                if (chunk != null)
                {

                    LocalBlockPos.x = Mathf.Abs(WorldBlockPos.x - chunk.Coord.x * 16);
                    LocalBlockPos.y = WorldBlockPos.y;
                    LocalBlockPos.z = Mathf.Abs(WorldBlockPos.z - chunk.Coord.y * 16);

                    if (chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].Interactable)
                    {
                        switch (chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType)
                        {
                            case 12:
                                UIHandler.Toggle(UIManager.InventoryType.CraftingTableUI);
                                break;

                            case 13:
                                UIHandler.Toggle(UIManager.InventoryType.FurnaceUI, WorldBlockPos);
                                break;

                            case 14:
                                UIHandler.Toggle(UIManager.InventoryType.FurnaceUI, WorldBlockPos);
                                break;
                        }

                        return;
                    }

                    Tgt = hitinf.point + hitinf.normal * .1f;
                    WorldBlockPos.x = Mathf.RoundToInt(Tgt.x);
                    WorldBlockPos.y = Mathf.RoundToInt(Tgt.y);
                    WorldBlockPos.z = Mathf.RoundToInt(Tgt.z);

                    chunk = WorldGenerator.GetChunk(WorldBlockPos);

                    LocalBlockPos.x = Mathf.Abs(WorldBlockPos.x - chunk.Coord.x * 16);
                    LocalBlockPos.y = WorldBlockPos.y;
                    LocalBlockPos.z = Mathf.Abs(WorldBlockPos.z - chunk.Coord.y * 16);

                    if (Instance.ItemsDict.ContainsKey(BlockToPlace))
                    {

                        if (Instance.ItemsDict[BlockToPlace].GetProperties().type == ItemProperties.ITEMTYPE.Block)
                        {
                            if (Physics.CheckBox(new Vector3(WorldBlockPos.x, WorldBlockPos.y, WorldBlockPos.z), Vector3.one / 2f * .9f, Quaternion.identity, BlocksMask))
                                return;

                            if (chunk.isUpdating)
                                return;

                            try
                            {
                                int Biome = chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BiomeID;
                                if (BlockToPlace != 13)
                                    chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z] = new Block(false, BlockToPlace, Biome, Block.BlockDirection.None, WorldGenerator.InteractableBlocks.Contains(BlockToPlace));
                                else
                                    chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z] = new Block(false, BlockToPlace, Biome, GetDirectionToPlayer(WorldBlockPos), WorldGenerator.InteractableBlocks.Contains(BlockToPlace));

                                TickManager.TryAddDynamicBlock(WorldBlockPos, new Furnace(), BlockToPlace, WorldBlockPos);

                                chunk.UpdateChunk();

                                if (HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.Stack != null)
                                {
                                    HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.Stack.Count -= 1;
                                    HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.UpdateCell();
                                }

                            }
                            catch (System.Exception ex)
                            {
                                Debug.Log(ex);
                            }
                        }
                    }
                }
            }
        }
        else if(Input.GetMouseButton(1))
        {
            AnimController.SetBool("Interacting", true);
            BreakBlocks = false;
            InteractFunction();
            return;
        }

        Delay = PlacementCooldown;
        AnimController.SetBool("Interacting", false);
        BlockBreakOverlay.material.SetFloat("_BreakAmount", 0);
        BlockBreakOverlay.enabled = false;
    }

    private Vector3Int LastBlockPos = Vector3Int.zero;
    private bool BreakBlocks = false;
    private float Delay;
    public void InteractFunction()
    {
        if(BreakBlocks)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hitinf, PlayerReach, ChunkMask))
            {
                // Setup the breaking effect

                BreakEffect.transform.position = WorldBlockPos;
                BlockBreakOverlay.enabled = true;

                // Pinpoint the position of the block within a chunk.

                Vector3 Tgt = hitinf.point - hitinf.normal * .1f;
                WorldBlockPos.x = Mathf.RoundToInt(Tgt.x);
                WorldBlockPos.y = Mathf.RoundToInt(Tgt.y);
                WorldBlockPos.z = Mathf.RoundToInt(Tgt.z);
                GetDirectionToPlayer(WorldBlockPos);

                // Get tthe chunk and pinpoint the block's local position

                TerrainChunk chunk = WorldGenerator.GetChunk(WorldBlockPos);

                if (chunk != null)
                {
                    if (chunk.isUpdating)
                        return;

                    LocalBlockPos.x = Mathf.Abs(WorldBlockPos.x - chunk.Coord.x * 16);
                    LocalBlockPos.y = WorldBlockPos.y;
                    LocalBlockPos.z = Mathf.Abs(WorldBlockPos.z - chunk.Coord.y * 16);
                }
                else
                {
                    return;
                }

                //Determine the breaking speed

                int ToolMaterialLevel = 0;
                BlockProperties SelectedBlockProperties = Instance.BlockPropertiesDict[chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType];
                BreakSpeed = SelectedBlockProperties.NonHarvestToolBreakSpeed;
                if (HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.Stack != null)
                {
                    ItemProperties selectedItem = HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.Stack.CurrentStackedItem.GetProperties();
                    ToolMaterialLevel = selectedItem.MaterialLevel;

                    BreakSpeed = selectedItem.tooltype == SelectedBlockProperties.HarvestingTool ?
                        SelectedBlockProperties.HarvestToolBreakSpeed + selectedItem.MaterialLevel / 2 :
                        SelectedBlockProperties.NonHarvestToolBreakSpeed;
                }

                //Break the block

                if (WorldBlockPos != LastBlockPos)
                {
                    LastBlockPos = WorldBlockPos;
                    BlockBreakOverlay.material.SetFloat("_BreakAmount", 0);
                    BlockBreakOverlay.transform.position = WorldBlockPos;
                }
                else
                {
                    BlockBreakOverlay.material.SetFloat("_BreakAmount", BlockBreakOverlay.material.GetFloat("_BreakAmount") + Time.deltaTime * BreakSpeed);
                }

                if(BlockBreakOverlay.material.GetFloat("_BreakAmount") >= 4)
                {
                    BlockBreakOverlay.enabled = false;

                    if (ToolMaterialLevel >= Instance.ItemsDict[chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType].GetProperties().MaterialLevel)
                    {
                        ItemEvents.Instance.InvokeBreakEvent(Instance.ItemsDict[chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType].GetProperties().ItemDrop, WorldBlockPos);
                    }

                    int Biome = chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BiomeID;
                    chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z] = new Block(false, 0, Biome);
                    chunk.UpdateChunk();
                    BreakEffect.Play();
                }
            }
            else
            {
                BlockBreakOverlay.enabled = false;
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, transform.forward, out hitinf, PlayerReach, ChunkMask))
            {
                if (Delay > 0)
                {
                    Delay -= Time.deltaTime;
                    return;
                }

                Vector3 Tgt = hitinf.point + hitinf.normal * -.1f;
                WorldBlockPos.x = Mathf.RoundToInt(Tgt.x);
                WorldBlockPos.y = Mathf.RoundToInt(Tgt.y);
                WorldBlockPos.z = Mathf.RoundToInt(Tgt.z);
                GetDirectionToPlayer(WorldBlockPos);

                TerrainChunk chunk = WorldGenerator.GetChunk(WorldBlockPos);

                if (chunk != null)
                {

                    LocalBlockPos.x = Mathf.Abs(WorldBlockPos.x - chunk.Coord.x * 16);
                    LocalBlockPos.y = WorldBlockPos.y;
                    LocalBlockPos.z = Mathf.Abs(WorldBlockPos.z - chunk.Coord.y * 16);

                    Debug.Log(chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType + "" + chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].Interactable);

                    if (chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].Interactable)
                    {
                        switch (chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType)
                        {
                            case 12:
                                UIHandler.Toggle(UIManager.InventoryType.CraftingTableUI);
                                break;

                            case 13:
                                UIHandler.Toggle(UIManager.InventoryType.FurnaceUI, WorldBlockPos);
                                break;

                            case 14:
                                UIHandler.Toggle(UIManager.InventoryType.FurnaceUI, WorldBlockPos);
                                break;
                        }

                        return;
                    }

                    Tgt = hitinf.point + hitinf.normal * .1f;
                    WorldBlockPos.x = Mathf.RoundToInt(Tgt.x);
                    WorldBlockPos.y = Mathf.RoundToInt(Tgt.y);
                    WorldBlockPos.z = Mathf.RoundToInt(Tgt.z);

                    chunk = WorldGenerator.GetChunk(WorldBlockPos);

                    LocalBlockPos.x = Mathf.Abs(WorldBlockPos.x - chunk.Coord.x * 16);
                    LocalBlockPos.y = WorldBlockPos.y;
                    LocalBlockPos.z = Mathf.Abs(WorldBlockPos.z - chunk.Coord.y * 16);

                    if (Instance.ItemsDict.ContainsKey(BlockToPlace))
                    {
                        if (Instance.ItemsDict[BlockToPlace].GetProperties().type == ItemProperties.ITEMTYPE.Block)
                        {
                            if (Physics.CheckBox(new Vector3(WorldBlockPos.x, WorldBlockPos.y, WorldBlockPos.z), Vector3.one / 2f * .9f, Quaternion.identity, BlocksMask))
                                return;

                            if (chunk.isUpdating)
                                return;

                            if (HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.Stack != null)
                            {
                                if (HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.Stack.CurrentStackedItem.GetProperties().type == ItemProperties.ITEMTYPE.Block)
                                {
                                    HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.Stack.Count -= 1;
                                    HotBarManager.HotBarItems[HotBarManager.PointedHotbarIndex].ClassContainer.UpdateCell();

                                    int Biome = chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BiomeID;
                                    if(BlockToPlace != 13)
                                        chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z] = new Block(false, BlockToPlace, Biome, Block.BlockDirection.None, WorldGenerator.InteractableBlocks.Contains(BlockToPlace));
                                    else
                                        chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z] = new Block(false, BlockToPlace, Biome, GetDirectionToPlayer(WorldBlockPos), WorldGenerator.InteractableBlocks.Contains(BlockToPlace));

                                    TickManager.TryAddDynamicBlock(WorldBlockPos, new Furnace(), BlockToPlace, WorldBlockPos);

                                    chunk.UpdateChunk();
                                }
                            }
                        }
                    }

                    Delay = PlacementCooldown;
                }
            }
            else
            {
                Delay = PlacementCooldown;
            }
        }
    }

    public string GetBlockBiome()
    {
        int Biome = 0;
        TerrainChunk chunk;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit _hitinf, Mathf.Infinity, ChunkMask))
        {
            Vector3 Tgt = _hitinf.point - _hitinf.normal * .1f;
            mWorldBlockPos.x = Mathf.RoundToInt(Tgt.x);
            mWorldBlockPos.y = Mathf.RoundToInt(Tgt.y);
            mWorldBlockPos.z = Mathf.RoundToInt(Tgt.z);

            chunk = WorldGenerator.GetChunk(mWorldBlockPos);

            if (chunk != null)
            {
                mLocalBlockPos.x = Mathf.Abs(mWorldBlockPos.x - chunk.Coord.x * 16);
                mLocalBlockPos.y = mWorldBlockPos.y;
                mLocalBlockPos.z = Mathf.Abs(mWorldBlockPos.z - chunk.Coord.y * 16);

                Biome = chunk.Data[mLocalBlockPos.x, mLocalBlockPos.y, mLocalBlockPos.z].BiomeID;
            }
        }

        switch(Biome)
        {
            case 1:
                return "Forest";
            case 2:
                return "Desert";
            case 3:
                return "Mountains";
        }

        return "UNIDENTIFIED";
    }

    public Block.BlockDirection GetDirectionToPlayer(Vector3Int BlockPos)
    {
        Vector3 DirectionVec = BlockPos - transform.parent.position;
        DirectionVec = new Vector3(Mathf.Round(DirectionVec.x), 0f, Mathf.Round(DirectionVec.z));

        if (DirectionVec.x > DirectionVec.z)
        {
            if (DirectionVec.x != 0)
                return DirectionVec.x < 0 ? Block.BlockDirection.Left : Block.BlockDirection.Right;
            else
            {
                return DirectionVec.z < 0 ? Block.BlockDirection.Forward : Block.BlockDirection.Back;
            }
        }
        else if (DirectionVec.x < DirectionVec.z)
        {
            if (DirectionVec.z != 0)
                return DirectionVec.z < 0 ? Block.BlockDirection.Forward : Block.BlockDirection.Back;
            else
            {
                return DirectionVec.x < 0 ? Block.BlockDirection.Left : Block.BlockDirection.Right;
            }
        }
        else if (DirectionVec == Vector3.zero)
        {
            return Block.BlockDirection.Forward;
        }

        return Block.BlockDirection.None;
    }

}
