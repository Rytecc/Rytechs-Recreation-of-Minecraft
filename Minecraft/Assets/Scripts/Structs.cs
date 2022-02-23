using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[System.Serializable]
public struct BlockInfo
{

    public BlockInfo(Vector3Int Pos, int type)
    {
        this.Pos = Pos;
        BlockType = type;
    }

    public Vector3Int Pos;
    public int BlockType;
}

[System.Serializable]
public class TerrainChunk
{

    internal struct ChunkMeshInfo
    {
        public ChunkMeshInfo(List<Vector3> Vertices, List<int> Tris, List<Vector2> uvs)
        {
            Verts = Vertices;
            tris = Tris;
            Uvs = uvs;
        }

        public List<Vector3> Verts;
        public List<int> tris;
        public List<Vector2> Uvs;
    }

    #region Read-only

    static readonly Vector3Int Self = Vector3Int.zero;
    static readonly Vector3Int Right = Vector3Int.right;
    static readonly Vector3Int Left = Vector3Int.left;
    static readonly Vector3Int Up = Vector3Int.up;
    static readonly Vector3Int Down = Vector3Int.down;
    static readonly Vector3Int Forward = new Vector3Int(0, 0, 1);
    static readonly Vector3Int Back = new Vector3Int(0, 0, -1);

    static readonly Vector3[] RightFace = new Vector3[]
    {
        new Vector3(.5f, -.5f, -.5f),
        new Vector3(.5f, -.5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, .5f, -.5f)
    };

    static readonly int[] RightTris = new int[]
    {
        0,2,1,0,3,2
    };

    static readonly Vector3[] LeftFace = new Vector3[]
    {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(-.5f, .5f, -.5f)
    };

    static readonly int[] LeftTris = new int[]
    {
        0,1,2,0,2,3
    };

    static readonly Vector3[] UpFace = new Vector3[]
    {
        new Vector3(-.5f, .5f, -.5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, .5f, -.5f)
    };

    static readonly int[] UpTris = new int[]
    {
        0,1,2,0,2,3
    };

    static readonly Vector3[] DownFace = new Vector3[]
    {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(.5f, -.5f, .5f),
        new Vector3(.5f, -.5f, -.5f)
    };

    static readonly int[] DownTris = new int[]
    {
        0,2,1,0,3,2
    };

    static readonly Vector3[] ForwardFace = new Vector3[]
    {
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, -.5f, .5f)
    };

    static readonly int[] ForwardTris = new int[]
    {
        0,2,1,0,3,2
    };

    static readonly Vector3[] BackFace = new Vector3[]
    {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, .5f, -.5f),
        new Vector3(.5f, .5f, -.5f),
        new Vector3(.5f, -.5f, -.5f)
    };

    static readonly int[] BackTris = new int[]
    {
        0,1,2,0,2,3
    };

    #endregion

    /// <summary>
    /// Use this constructor for initialization.
    /// </summary>
    public TerrainChunk(ChunkCoords ChunkID, Block[,,] ChunkData, GameObject ChunkPrefab, Material AtlasMaterial, bool IsBorder)
    {
        ChunkObject = Object.Instantiate(ChunkPrefab, new Vector3(ChunkID.x, 0f, ChunkID.y) * 16f, Quaternion.identity);
        ChunkObject.layer = 8;
        Filter = ChunkObject.GetComponent<MeshFilter>();
        Renderer = ChunkObject.GetComponent<MeshRenderer>();
        collider = ChunkObject.GetComponent<MeshCollider>();

        ChunkObject.isStatic = true;
        Renderer.material = AtlasMaterial;
        Data = ChunkData;

        DrawFaceTowardsList = new List<int>() { 0, 7, 9, 10, 11 };

        Coord = ChunkID;
        IsBiomeBorder = IsBorder;
    }

    public ChunkCoords Coord;
    public Block[,,] Data;

    public bool isActive = true;
    public bool isUpdating;
    public bool Generated;
    public bool IsBiomeBorder;

    private GameObject ChunkPrefab = null;
    private List<int> DrawFaceTowardsList;
    public GameObject ChunkObject;
    public MeshFilter Filter;
    public MeshRenderer Renderer;
    public MeshCollider collider;

    public void UpdateChunk()
    {
        Update();
    }

    public void UpdateChunkCull(ChunkCoords CenterCoord)
    {
        float Distance = ChunkCoords.Distance(Coord, CenterCoord);
        if (Distance <= WorldGenerator.InstancedGenerator.RenderDistance)
        {
            ChunkObject.SetActive(true);
            isActive = true;
            return;
        }

        ChunkObject.SetActive(false);
        isActive = false;
    }

    private async void Update()
    {
        isUpdating = true;

        List<Vector3> Verts = new List<Vector3>();
        List<int> Indices = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Task t = Task.Factory.StartNew(() =>
        {
            SetChunkMeshData(Data, out Verts, out Indices, out uvs);
        });

        await t;
        if(Filter)
        {
            Filter.mesh.Clear();
            Filter.mesh.SetVertices(Verts);
            Filter.mesh.SetTriangles(Indices, 0);
            Filter.mesh.SetUVs(0, uvs);
            Filter.mesh.RecalculateNormals();
            collider.sharedMesh = Filter.mesh;
        }

        isUpdating = false;
    }

    private void SetChunkMeshData(Block[,,] Data, out List<Vector3> Verts, out List<int> Indices, out List<Vector2> uvs)
    {

        Verts = new List<Vector3>();
        Indices = new List<int>();
        uvs = new List<Vector2>();

        Vector3 Pos;
        Vector2[] UvsToAdd;
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 256; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    Pos.x = x;
                    Pos.y = y;
                    Pos.z = z;



                    if (Data[x, y, z].BlockType > 0)
                    {
                        BlockMaterial mat = WorldGenerator.BlockUVs[Data[x, y, z].BlockType];

                        #region Foilage

                        #endregion

                        #region GeneralBlockDrawing

                        UvsToAdd = GetBlockUV(mat, Data[x, y, z].Direction, Block.BlockDirection.Up);
                        try
                        {
                            if (DrawFaceTowardsList.Contains(Data[x, y + 1, z].BlockType))
                            {
                                UvsToAdd = mat.Topuv.Length == 0 ? mat.Base : mat.Topuv;
                                for (int i = 0; i < 4; i++)
                                {
                                    Verts.Add(UpFace[i] + Pos);
                                }

                                for (int i = 0; i < 6; i++)
                                {
                                    Indices.Add(Verts.Count - 4 + UpTris[i]);
                                }

                                uvs.Add(UvsToAdd[0]);
                                uvs.Add(UvsToAdd[1]);
                                uvs.Add(UvsToAdd[3]);
                                uvs.Add(UvsToAdd[2]);
                            }
                        }
                        catch (System.Exception) 
                        {
                            
                            for (int i = 0; i < 4; i++)
                            {
                                Verts.Add(UpFace[i] + Pos);
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                Indices.Add(Verts.Count - 4 + UpTris[i]);
                            }

                            uvs.Add(UvsToAdd[0]);
                            uvs.Add(UvsToAdd[1]);
                            uvs.Add(UvsToAdd[3]);
                            uvs.Add(UvsToAdd[2]);
                        }

                        UvsToAdd = GetBlockUV(mat, Data[x, y, z].Direction, Block.BlockDirection.Down);
                        try
                        {
                            if (DrawFaceTowardsList.Contains(Data[x, y - 1, z].BlockType))
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Verts.Add(DownFace[i] + Pos);
   
                                }
                        
                                for (int i = 0; i < 6; i++)
                                {
                                    Indices.Add(Verts.Count - 4 + DownTris[i]);
                                }

                                uvs.Add(UvsToAdd[0]);
                                uvs.Add(UvsToAdd[1]);
                                uvs.Add(UvsToAdd[3]);
                                uvs.Add(UvsToAdd[2]);
                            }


                        }
                        catch (System.Exception)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Verts.Add(DownFace[i] + Pos);
                            }
                        
                            for (int i = 0; i < 6; i++)
                            {
                                Indices.Add(Verts.Count - 4 + DownTris[i]);
                            }

                            uvs.Add(UvsToAdd[0]);
                            uvs.Add(UvsToAdd[1]);
                            uvs.Add(UvsToAdd[3]);
                            uvs.Add(UvsToAdd[2]);
                        }

                        UvsToAdd = GetBlockUV(mat, Data[x, y, z].Direction, Block.BlockDirection.Right);
                        try
                        {

                            if (DrawFaceTowardsList.Contains(Data[x + 1, y, z].BlockType))
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Verts.Add(RightFace[i] + Pos);
   
                                }
                        
                                for (int i = 0; i < 6; i++)
                                {
                                    Indices.Add(Verts.Count - 4 + RightTris[i]);
                                }

                                uvs.Add(UvsToAdd[2]);
                                uvs.Add(UvsToAdd[3]);
                                uvs.Add(UvsToAdd[1]);
                                uvs.Add(UvsToAdd[0]);
                            }
                        }
                        catch (System.Exception) 
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Verts.Add(RightFace[i] + Pos);
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                Indices.Add(Verts.Count - 4 + RightTris[i]);
                            }

                            uvs.Add(UvsToAdd[2]);
                            uvs.Add(UvsToAdd[3]);
                            uvs.Add(UvsToAdd[1]);
                            uvs.Add(UvsToAdd[0]);
                        }

                        UvsToAdd = GetBlockUV(mat, Data[x, y, z].Direction, Block.BlockDirection.Left);
                        try
                        {

                            if (DrawFaceTowardsList.Contains(Data[x - 1, y, z].BlockType))
                            {

                                for (int i = 0; i < 4; i++)
                                {
                                    Verts.Add(LeftFace[i] + Pos);
   
                                }
                        
                                for (int i = 0; i < 6; i++)
                                {
                                    Indices.Add(Verts.Count - 4 + LeftTris[i]);
                                }

                                uvs.Add(UvsToAdd[2]);
                                uvs.Add(UvsToAdd[3]);
                                uvs.Add(UvsToAdd[1]);
                                uvs.Add(UvsToAdd[0]);
                            }
                        }
                        catch (System.Exception) 
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Verts.Add(LeftFace[i] + Pos);
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                Indices.Add(Verts.Count - 4 + LeftTris[i]);
                            }

                            uvs.Add(UvsToAdd[2]);
                            uvs.Add(UvsToAdd[3]);
                            uvs.Add(UvsToAdd[1]);
                            uvs.Add(UvsToAdd[0]);
                        }

                        UvsToAdd = GetBlockUV(mat, Data[x, y, z].Direction, Block.BlockDirection.Forward);
                        try
                        {
                            if (DrawFaceTowardsList.Contains(Data[x, y, z + 1].BlockType))
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Verts.Add(ForwardFace[i] + Pos);
   
                                }
                        
                                for (int i = 0; i < 6; i++)
                                {
                                    Indices.Add(Verts.Count - 4 + ForwardTris[i]);
                                }

                                uvs.Add(UvsToAdd[3]);
                                uvs.Add(UvsToAdd[1]);
                                uvs.Add(UvsToAdd[0]);
                                uvs.Add(UvsToAdd[2]);
                            }
                        }
                        catch (System.Exception) 
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Verts.Add(ForwardFace[i] + Pos);
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                Indices.Add(Verts.Count - 4 + ForwardTris[i]);
                            }

                            uvs.Add(UvsToAdd[3]);
                            uvs.Add(UvsToAdd[1]);
                            uvs.Add(UvsToAdd[0]);
                            uvs.Add(UvsToAdd[2]);
                        }

                        UvsToAdd = GetBlockUV(mat, Data[x, y, z].Direction, Block.BlockDirection.Back);
                        try
                        {
                            if (DrawFaceTowardsList.Contains(Data[x, y, z - 1].BlockType))
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Verts.Add(BackFace[i] + Pos);
                                }
                        
                                for (int i = 0; i < 6; i++)
                                {
                                    Indices.Add(Verts.Count - 4 + BackTris[i]);
                                }

                                uvs.Add(UvsToAdd[3]);
                                uvs.Add(UvsToAdd[1]);
                                uvs.Add(UvsToAdd[0]);
                                uvs.Add(UvsToAdd[2]);
                            }
                        }
                        catch (System.Exception) 
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Verts.Add(BackFace[i] + Pos);
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                Indices.Add(Verts.Count - 4 + BackTris[i]);
                            }

                            uvs.Add(UvsToAdd[3]);
                            uvs.Add(UvsToAdd[1]);
                            uvs.Add(UvsToAdd[0]);
                            uvs.Add(UvsToAdd[2]);
                        }

                        #endregion
                    }
                }
            }
        }
    }

    /// <summary>
    /// The Faceposittion parameter must never be NONE
    /// </summary>
    /// <param name="BlockIdentity"></param>
    /// <param name="dir"></param>
    /// <param name="FacePosition"></param>
    /// <returns></returns>
    public Vector2[] GetBlockUV(BlockMaterial BlockIdentity, Block.BlockDirection dir, Block.BlockDirection FacePosition)
    {
        //Check if the blockdirection is equivalent to the FaceDirection
        if(dir == Block.BlockDirection.None)
        {
            return BlockIdentity.Base.Length == 0 ? BlockIdentity.GetUV(FacePosition) : BlockIdentity.Base;
        }
        else
        {
            switch(dir)
            {
                case Block.BlockDirection.Forward:
                    switch(FacePosition)
                    {
                        case Block.BlockDirection.Forward:
                            return BlockIdentity.Forwarduv;
                        case Block.BlockDirection.Back:
                            return BlockIdentity.Backuv;
                        case Block.BlockDirection.Left:
                            return BlockIdentity.Leftuv;
                        case Block.BlockDirection.Right:
                            return BlockIdentity.Rightuv;
                    }
                    break;
                case Block.BlockDirection.Back:
                    switch (FacePosition)
                    {
                        case Block.BlockDirection.Forward:
                            return BlockIdentity.Backuv;
                        case Block.BlockDirection.Back:
                            return BlockIdentity.Forwarduv;
                        case Block.BlockDirection.Left:
                            return BlockIdentity.Leftuv;
                        case Block.BlockDirection.Right:
                            return BlockIdentity.Rightuv;
                    }
                    break;
                case Block.BlockDirection.Left:
                    switch (FacePosition)
                    {
                        case Block.BlockDirection.Forward:
                            return BlockIdentity.Leftuv;
                        case Block.BlockDirection.Back:
                            return BlockIdentity.Rightuv;
                        case Block.BlockDirection.Left:
                            return BlockIdentity.Backuv;
                        case Block.BlockDirection.Right:
                            return BlockIdentity.Forwarduv;
                    }
                    break;
                case Block.BlockDirection.Right:
                    switch (FacePosition)
                    {
                        case Block.BlockDirection.Forward:
                            return BlockIdentity.Rightuv;
                        case Block.BlockDirection.Back:
                            return BlockIdentity.Leftuv;
                        case Block.BlockDirection.Left:
                            return BlockIdentity.Forwarduv;
                        case Block.BlockDirection.Right:
                            return BlockIdentity.Backuv;
                    }
                    break;
            }
        }

        return null;
    }
}

public interface OnTick
{
    object GetProperty(int PropertyIndex);
    void SetProperty(object Value, int PropertyIndex);
    bool IsInitialized();
    void Init(object Params);
    TerrainChunk Tick();
}

public struct Block
{

    public enum BlockDirection
    {
        None,
        Up,
        Down,
        Forward,
        Back,
        Left,
        Right
    }

    public Block(bool Natural = true, int blocktype = -1, int biome = -1, BlockDirection dir = BlockDirection.None, bool Interactable = false, OnTick TickFlag = null)
    {
        BlockType = blocktype;
        BiomeID = biome;
        NaturallyGenerated = Natural;
        Direction = dir;
        this.Interactable = Interactable;
        TickUpdateEvent = TickFlag;
    }

    public OnTick TickUpdateEvent;

    public BlockDirection Direction;
    public bool Interactable;
    public bool NaturallyGenerated;
    public int BlockType;
    public int BiomeID;
}

[System.Serializable]
public class BlockMaterial
{
    public Sprite BaseTexture;

    public Sprite Top;
    public Sprite Bottom;
    [Space]
    public Sprite Left;
    public Sprite Right;
    [Space]
    public Sprite Forward;
    public Sprite Back;

    [HideInInspector] public Vector2[] Base;
    [HideInInspector] public Vector2[] Topuv;
    [HideInInspector] public Vector2[] Bottomuv;
    [HideInInspector] public Vector2[] Leftuv;
    [HideInInspector] public Vector2[] Rightuv;
    [HideInInspector] public Vector2[] Forwarduv;
    [HideInInspector] public Vector2[] Backuv;

    public void Init()
    {
        if(Top != null) Topuv = Top.uv;
        if(Bottom != null)Bottomuv = Bottom.uv;
        if(Left != null)Leftuv = Left.uv;
        if(Right != null)Rightuv = Right.uv;
        if(Forward != null)Forwarduv = Forward.uv;
        if(Back != null)Backuv = Back.uv;
        if (BaseTexture != null) Base = BaseTexture.uv;
    }


    public Vector2[] GetUV(Block.BlockDirection dir)
    {
        switch(dir)
        {
            case Block.BlockDirection.Forward:
                return Forwarduv;
            case Block.BlockDirection.Back:
                return Backuv;
            case Block.BlockDirection.Left:
                return Leftuv;
            case Block.BlockDirection.Right:
                return Rightuv;
            case Block.BlockDirection.Up:
                return Topuv;
            case Block.BlockDirection.Down:
                return Bottomuv;
        }

        return null;
    }
}

[System.Serializable]
public struct ChunkCoords
{
    public static float Distance(ChunkCoords a, ChunkCoords b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
    }

    public ChunkCoords(int x, int z)
    {
        this.x = x;
        y = z;
    }

    public int x;
    public int y;

    public static ChunkCoords operator +(ChunkCoords a, ChunkCoords b)
    {
        return new ChunkCoords(a.x + b.x, a.y + b.y);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(x, y);
    }
}

public struct Structure
{
    public Structure(BlockInfo[] Blocks, Vector3Int Root)
    {
        BlocksToPlace = Blocks;
        RootPos = Root;
    }

    public Vector3Int RootPos;
    public BlockInfo[] BlocksToPlace;
}  

[System.Serializable]
public class ItemStack
{

    public ItemStack(Item stackedItem, int Count)
    {
        CurrentStackedItem = stackedItem;
        this.Count = Count;
        MaxCount = 64;
    }

    public void AddToStack(int amount, out int OverflowAmount)
    {
        if(CurrentStackedItem.GetProperties().Stackable)
        {
            OverflowAmount = 0;
            Count += amount;

            if (Count > MaxCount)
            {
                OverflowAmount = Count - MaxCount;
                Count = MaxCount;
                return;
            }
        }
        else
        {
            OverflowAmount = -1;
        }
    }

    public Item CurrentStackedItem;
    public int MaxCount;
    public int Count;

}

/// <summary>
/// Contains the pointer and properties of the item
/// the pointer is the index of it's material
/// </summary>
[System.Serializable]
public class Item
{
    public Item(int ID, ItemProperties.ITEMTYPE type, ItemProperties.TOOLTYPE tooltype)
    {
        this.ID = ID;
        Properties = new ItemProperties(type, tooltype);
    }

    public ItemProperties GetProperties()
    {
        return Properties;
    }

    public int GetID()
    {
        return ID;
    }

    [SerializeField] private ItemProperties Properties;
    [SerializeField] private int ID;
}

/// <summary>
/// Determines the properties of the item
/// </summary>
[System.Serializable]
public class ItemProperties
{
    [System.Serializable]
    public class FuelParams
    {
        public FuelParams(int TickBurnTime, int FuelSmeltSpeed)
        {
            BurnTime = TickBurnTime;
            SmeltSpeed = FuelSmeltSpeed;
        }

        public bool IsFuel = false;
        public int BurnTime;
        public int SmeltSpeed;
    }

    [System.Serializable]
    public class SmeltingParams
    {
        public int SmeltingResult = -1;
    }

    public ItemProperties(ITEMTYPE type, TOOLTYPE tooltype)
    {
        this.type = type;
        this.tooltype = tooltype;

        if (type == ITEMTYPE.Block)
            this.tooltype = TOOLTYPE.None;

        HarvestLevel = 0;

    }

    public ItemProperties(ITEMTYPE type, TOOLTYPE tooltype, int Level)
    {
        this.type = type;
        this.tooltype = tooltype;

        if (type == ITEMTYPE.Block)
            this.tooltype = TOOLTYPE.None;

        HarvestLevel = Level;

    }

    public enum ITEMTYPE
    {
        Tool,
        Block,
        Material
    }

    public enum TOOLTYPE
    {
        None,
        Pickaxe,
        Axe,
        Shovel,
        Offense
    }

    public TOOLTYPE tooltype;
    public ITEMTYPE type;

    public SmeltingParams SmeltingResult;
    public FuelParams FuelProperties;
    [Space]
    public bool Stackable = true;
    public int HarvestLevel = 0;
    public int MaterialLevel = 0;
    public int ItemDrop = 2;
}

// vampire