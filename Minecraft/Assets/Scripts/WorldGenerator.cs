using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine; 

/// <summary>
///"0 = Air
///"1 = Grass
///"2 = Dirt
///"3 = Stone
///"4 = Bedrock
///"5 = Sand
///"6 = Oak_Log
///"7 = Oak_Leaves
///"8 = Cactus
///"9 = Rose
///"10 = Dandelion
///"11 = Dead Bush
///"12 = Crafting_Table
///"13 = Furnace
///"14 = Furnace_On
///"15 = Oak_Planks
///"16 = Coal_Ore
///"17 = Iron_Ore
///"18 = Diamond_Ore
///"19 = Cobblestone
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    #region Variables
    public static bool IsGenerating;
    public static List<int> InteractableBlocks = new List<int>() { 12, 13, 14 };
    public static Dictionary<ChunkCoords, Block[,,]> AdditiveData = new Dictionary<ChunkCoords, Block[,,]>();
    public static Dictionary<ChunkCoords, TerrainChunk> ChunksInWorld = new Dictionary<ChunkCoords, TerrainChunk>();
    public static WorldGenerator InstancedGenerator;

    public int RenderDistance = 12;

    [SerializeField]
    private AnimationCurve InterpolateCurve;
    [Space]
    [SerializeField]
    private NoiseGeneratorParams BiomeGenerator;

    [SerializeField]
    private int WorldOffset = 15;

    [SerializeField]
    private int MountainOffset;

    [SerializeField]
    private float CoalChance;

    [SerializeField]
    private float IronChance;

    [SerializeField]
    private float DiamondChance;

    public bool GenerationCompleted = false;

    [SerializeField]
    [Tooltip
    (
        "0 = Air \n" +
        "1 = Grass\n" +
        "2 = Dirt\n" +
        "3 = Stone\n" +
        "4 = Bedrock\n" +
        "5 = Sand\n" +
        "6 = Oak_Log\n" +
        "7 = Oak_Leaves\n" +
        "8 = Cactus\n" +
        "9 = Rose\n" +
        "10 = Dandelion\n" +
        "11 = Dead Bush" +
        "12 = Crafting_Table\n" +
        "13 = Furnace\n" +
        "14 = Furnace_On\n" +
        "15 = Oak_Planks\n" +
        "16 = Coal_Ore\n" +
        "17 = Iron_Ore\n" +
        "18 = Diamond_Ore\n" +
        "19 = Cobblestone\n"
    )]
    private BlockMaterial[] TextureSprites;

    [SerializeField]
    private Material blockAtlasMaterial;
    [SerializeField] private GameObject ChunkPrefab;
    [Space]
    [SerializeField] private NoiseGeneratorParams ForestParams;
    [SerializeField] private NoiseGeneratorParams MountainParams;
    [SerializeField] private NoiseGeneratorParams DesertParams;
    [Space]
    [SerializeField] private NoiseGeneratorParams CaveGenerator;
    [SerializeField] private NoiseGeneratorParams CaveGeneratorMask;
    [Space]
    [SerializeField] private GenerationFeature[] GenerationFeatures;

    public static Dictionary<int, BlockMaterial> BlockUVs;
    private FastNoiseLite BiomeNoise;

    private static WorldStructureGenerator StructureGenerator;
    private static float[] InterpTimes;
    private static Unity.Mathematics.Random WorldGenRandom;
    #endregion

    #region Generator_Events

    public delegate void OnChunkComplete(ChunkCoords ChunkCoordGenerated);
    public event OnChunkComplete OnChunkGenerated;

    #endregion

    #region Unity_Methods

    private IEnumerator Start()
    {

        #region Variable Initialization
        StructureGenerator = GetComponent<WorldStructureGenerator>();
        InstancedGenerator = this;
        InterpTimes = new float[16];
        InterpTimes[0] = 0;
        for(int i = 1; i <= 15; i++)
        {
            float f = i ;
            InterpTimes[i] = InterpolateCurve.Evaluate(f / 16);
        }

        WorldGenRandom = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        
        BlockUVs = new Dictionary<int, BlockMaterial>();

        for(int i = 0; i < TextureSprites.Length; i++)
        {
            BlockUVs.Add(i + 1, TextureSprites[i]);
            TextureSprites[i].Init();
        }

        BiomeNoise = new FastNoiseLite();
        SetNoiseParameters(BiomeNoise, BiomeGenerator, out _);

        Queue<ChunkCoords> CoordsToGenerate = new Queue<ChunkCoords>();
        for(int x = -RenderDistance; x < RenderDistance; x++)
        {
            for(int z = -RenderDistance; z < RenderDistance; z++)
            {
                CoordsToGenerate.Enqueue(new ChunkCoords(x, z));
            }
        }

        LoadMenuClass loadMenu = FindObjectOfType<LoadMenuClass>();
        StartCoroutine(CreateChunkBatch(CoordsToGenerate, loadMenu));

        #endregion

        while(GenerationCompleted != true)
        {
            yield return null;
        }

        PlayerManager.Instance.Init();
        InfiniteTerrainGenerator.Init = true;
        InfiniteTerrainGenerator.Cycle(true);
    }

    #endregion

    #region Generation Methods

    public IEnumerator CreateChunkBatch(Queue<ChunkCoords> ChunksToCreate, LoadMenuClass loadclass)
    {
        IsGenerating = true;
        int StartingCount = ChunksToCreate.Count;

        if (loadclass != null)
        {
            loadclass.SetProgressState("Generating World");


            for (int i = 0; i < StartingCount; i++)
            {
                GenerationCompleted = false;
                ChunkCoords coord = ChunksToCreate.Dequeue();
                CreateChunk(coord.x, coord.y);

                yield return new WaitUntil(() => GenerationCompleted == true);

                loadclass.SetProgressBarAmount((float)i / StartingCount);
            }

            loadclass.Dispose();
        }
        else
        {
            for (int i = 0; i < StartingCount; i++)
            {
                GenerationCompleted = false;
                ChunkCoords coord = ChunksToCreate.Dequeue();
                CreateChunk(coord.x, coord.y);

                yield return new WaitUntil(() => GenerationCompleted == true);
            }
        }

        IsGenerating = false;
        //OnChunksGenerated?.Invoke();
    }

    public async void CreateChunk(int x, int z)
    {
        ChunkCoords c = new ChunkCoords(x * 16, z * 16);
        ChunkCoords trueCH = new ChunkCoords(c.x / 16, c.y / 16);
        trueCH.x = x;
        trueCH.y = z;

        Vector2Int[,] hmap = null;
        Block[,,] DataToDraw = null;
        bool isborder = false;

        Task ts = Task.Factory.StartNew(() => DataToDraw = CreateChunkData(trueCH, out isborder, out hmap), TaskCreationOptions.LongRunning);
        await ts;

        CreateChunk(DataToDraw, trueCH, isborder, hmap);
        OnChunkGenerated?.Invoke(trueCH);
    }

    private void CreateChunk(Block[,,] DataToDraw, ChunkCoords position, bool isBorder, Vector2Int[,] HMap) 
    {
        ChunksInWorld.Add(position, new TerrainChunk(position, DataToDraw, ChunkPrefab, blockAtlasMaterial, true));
        ChunksInWorld[position].UpdateChunk();
    }

    #endregion

    #region Getters

    public static TerrainChunk GetChunk(Vector3Int Position, Vector3Int Direction)
    {
        Vector3Int ReferencedBlockWorldPosition = Position + Direction;
        Vector3Int LocalBlockPosition = Vector3Int.zero;
        ChunkCoords ReferencedChunk = new ChunkCoords();

        if (ReferencedBlockWorldPosition.x < 0)
        {
            ReferencedChunk.x = Mathf.FloorToInt((float)ReferencedBlockWorldPosition.x / 16);
        }
        else if (ReferencedBlockWorldPosition.x > 0)
        {
            ReferencedChunk.x = Mathf.CeilToInt((float)ReferencedBlockWorldPosition.x / 16);
        }

        if (ReferencedBlockWorldPosition.z < 0)
        {
            ReferencedChunk.y = Mathf.FloorToInt((float)ReferencedBlockWorldPosition.z / 16);
        }
        else if (ReferencedBlockWorldPosition.z > 0)
        {
            ReferencedChunk.y = Mathf.CeilToInt((float)ReferencedBlockWorldPosition.z / 16);
        }

        //If the referenced chunk exists, index the referencedblockposition
        if (ChunksInWorld.ContainsKey(ReferencedChunk))
        {
            return ChunksInWorld[ReferencedChunk];
        }

        return null;
    }

    public static TerrainChunk GetChunk(Vector3Int Position, out ChunkCoords ChunkPosition)
    {
        ChunkPosition.x = Mathf.FloorToInt((float)Position.x / 16);
        ChunkPosition.y = Mathf.FloorToInt((float)Position.z / 16);

        //If the referenced chunk exists, index the referencedblockposition
        if (ChunksInWorld.ContainsKey(ChunkPosition))
        {
            return ChunksInWorld[ChunkPosition];
        }

        return null;
    }

    public static TerrainChunk GetChunk(Vector3Int Position)
    {
        ChunkCoords ReferencedChunk = new ChunkCoords();

        ReferencedChunk.x = Mathf.FloorToInt((float)Position.x / 16);
        ReferencedChunk.y = Mathf.FloorToInt((float)Position.z / 16);

        //If the referenced chunk exists, index the referencedblockposition
        if (ChunksInWorld.ContainsKey(ReferencedChunk))
        {
            return ChunksInWorld[ReferencedChunk];
        }

        return null;
    }

    public static bool BlockExists(Vector3Int Position)
    {
        lock(ChunksInWorld)
        {
            TerrainChunk ch = GetChunk(Position);

            if (ch == null)
            {
                return false;
            }

            return true;
        }
    }

    public static ChunkCoords GetChunkCoords(Vector3Int Position)
    {
        ChunkCoords ReferencedChunk = new ChunkCoords();

        ReferencedChunk.x = Mathf.FloorToInt((float)Position.x / 16);
        ReferencedChunk.y = Mathf.FloorToInt((float)Position.z / 16);

        return ReferencedChunk;
    }

    public static int GetBlockType(Vector3Int Position, Vector3Int Direction)
    {
        TerrainChunk chunk = GetChunk(Position);

        try
        {
            Vector3Int LocalBlockPos = new Vector3Int(math.abs(Position.x - chunk.Coord.x * 16), Position.y, math.abs(Position.z - chunk.Coord.y * 16));
            return chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType;
        }
        catch (System.Exception)
        {
            return -1;
        }
    }

    public static void GetLocalBlockPosition(Vector3Int Position, out ChunkCoords Coord, out Vector3Int LocalPos)
    {
        Coord.x = Mathf.FloorToInt((float)Position.x / 16);
        Coord.y = Mathf.FloorToInt((float)Position.z / 16);
        LocalPos = new Vector3Int(math.abs(Position.x - Coord.x * 16), Position.y, math.abs(Position.z - Coord.y * 16));
    }

    public static bool PlaceBlock(Vector3Int Position, int ID, int Biome, bool Update = false, bool Natural = true)
    {
        TerrainChunk chunk = GetChunk(Position);

        try
        {
            Vector3Int LocalBlockPos = new Vector3Int(math.abs(Position.x - chunk.Coord.x * 16), Position.y, math.abs(Position.z - chunk.Coord.y * 16));
            lock (chunk.Data) 
            { 
                chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType = ID;
                chunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].NaturallyGenerated = Natural;
            }

            if (Update)
            {
                chunk.UpdateChunk();
            }

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public static bool PlaceBlock(out TerrainChunk ReferencedChunk, Vector3Int Position, int ID, int Biome, bool Update = false, bool Natural = true)
    {
        ReferencedChunk = GetChunk(Position);

        try
        {
            Vector3Int LocalBlockPos = new Vector3Int(math.abs(Position.x - ReferencedChunk.Coord.x * 16), Position.y, math.abs(Position.z - ReferencedChunk.Coord.y * 16));
            lock (ReferencedChunk.Data)
            {
                ReferencedChunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].BlockType = ID;
                ReferencedChunk.Data[LocalBlockPos.x, LocalBlockPos.y, LocalBlockPos.z].NaturallyGenerated = Natural;
            }

            if (Update)
            {
                ReferencedChunk.UpdateChunk();
            }

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    static void SetNoiseParameters(FastNoiseLite NoiseGenerator, NoiseGeneratorParams param, out float referencedIntensity)
    {
        NoiseGenerator.SetCellularDistanceFunction(param.CellularDistanceFunction);
        NoiseGenerator.SetCellularJitter(param.CellularJitterModifier);
        NoiseGenerator.SetCellularReturnType(param.CellularReturnType);
        NoiseGenerator.SetDomainWarpAmp(param.DomainWarpAmp);
        NoiseGenerator.SetDomainWarpType(param.DomainWarpType);
        NoiseGenerator.SetFractalGain(param.Gain);
        NoiseGenerator.SetFractalLacunarity(param.Lacunarity);
        NoiseGenerator.SetFractalOctaves(param.Octaves);
        NoiseGenerator.SetFractalPingPongStrength(param.PingPongStength);
        NoiseGenerator.SetFractalType(param.mFractalType);
        NoiseGenerator.SetFractalWeightedStrength(param.WeightedStrength);
        NoiseGenerator.SetFrequency(param.mFrequency);
        NoiseGenerator.SetNoiseType(param.mNoiseType);
        NoiseGenerator.SetRotationType3D(param.mRotationType3D);
        NoiseGenerator.SetSeed(param.mSeed == 0 ? WorldGenRandom.NextInt(-int.MaxValue, int.MaxValue) : param.mSeed);

        referencedIntensity = param.Intensity;
    }

    static float Blerp(float LowerX, float UpperX, float UpperY, float LowerY, int x, int y)
    {
        float BottomLerp = math.lerp(LowerX, UpperX, InterpTimes[x]);
        float TopLerp = math.lerp(LowerY, UpperY, InterpTimes[x]);

        return math.lerp(BottomLerp, TopLerp, InterpTimes[y]);
    }

    private Block[,,] CreateChunkData(ChunkCoords Position, out bool Border, out Vector2Int[,] HeightMap)
    {
        Block[,,] Data = new Block[16, 256, 16];
        HeightMap = new Vector2Int[16, 16];

        if(AdditiveData.ContainsKey(Position))
        {
            Data = AdditiveData[Position];
            AdditiveData.Remove(Position);
        }

        FastNoiseLite NoiseGenerator = new FastNoiseLite();

          float LastNoiseGen = 0;
        int LastBiome = -1;
        bool Interpolate = false;
        Border = false;

        // Get the initial Heights
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                float BiomeValue = math.lerp(-1, 1, math.abs(BiomeNoise.GetNoise(Position.x * 16 + x, Position.y * 16 + z)));

                float NoiseGen = 0;
                int height = 0;
                float Intensity = 0;

                BiomeValue = math.clamp(BiomeValue, 0f, 1f);

                if (BiomeValue < 0.1f) // Forest = 1
                {
                    SetNoiseParameters(NoiseGenerator, ForestParams, out Intensity);
                    NoiseGen = NoiseGenerator.GetNoise(Position.x * 16 + x, Position.y * 16 + z) * Intensity + WorldOffset;

                    height = Mathf.CeilToInt(NoiseGen);

                    if (LastBiome != 1 && LastBiome >= 0)
                    {
                        Interpolate = true;
                        Border = true;
                    }

                    LastBiome = 1;
                }
                else if (BiomeValue > 0.2f) // Desert = 2
                {
                    SetNoiseParameters(NoiseGenerator, DesertParams, out Intensity);
                    NoiseGen = NoiseGenerator.GetNoise(Position.x * 16 + x, Position.y * 16 + z) * Intensity + WorldOffset;

                    height = Mathf.CeilToInt(NoiseGen);

                    if (LastBiome != 0 && LastBiome >= 0)
                    {
                        Interpolate = true;
                        Border = true;
                    }

                    LastBiome = 2;
                }
                else if (BiomeValue >= 0.1f && BiomeValue < 0.2f) // Mountains = 3
                {
                    SetNoiseParameters(NoiseGenerator, MountainParams, out Intensity);
                    NoiseGen = NoiseGenerator.GetNoise(Position.x * 16 + x, Position.y * 16 + z) * Intensity + MountainOffset;

                    height = Mathf.CeilToInt(NoiseGen);

                    if (LastBiome != 3 && LastBiome >= 0)
                    {
                        Interpolate = true;
                        Border = true;
                    }

                    LastBiome = 3;
                }

                HeightMap[x, z].x = height;
                HeightMap[x, z].y = LastBiome;
                LastNoiseGen = NoiseGen;

            }
        }

        // If Chunk Borders Biome Bilinearly Interpolate between the heights
        if (Interpolate)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    HeightMap[x, z].x = Mathf.RoundToInt(Blerp(HeightMap[0, 0].x, HeightMap[15, 0].x, HeightMap[15, 15].x, HeightMap[0, 15].x, x, z));
                }
            }
        }

        //Apply the blocks
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                switch (HeightMap[x, z].y)
                {
                    case 2: // Desert

                        for (int y = 255; y >= 0; y--)
                        {
                            if (y > HeightMap[x, z].x)
                            {
                                Data[x, y, z].BiomeID = 2;
                                continue;
                            }

                            Data[x, y, z].BiomeID = 2;
                            if (y == HeightMap[x, z].x) Data[x, y, z].BlockType = 5;
                            if (y >= HeightMap[x, z].x - 4 && y < HeightMap[x, z].x) Data[x, y, z].BlockType = 5;
                            if (y >= 1 && y < HeightMap[x, z].x - 4) Data[x, y, z].BlockType = 3;
                            if (y == 0) Data[x, y, z].BlockType = 4;
                        }

                        break;

                    case 1: // Forest

                        for (int y = 255; y >= 0; y--)
                        {

                            if (y > HeightMap[x, z].x)
                            {
                                Data[x, y, z].BiomeID = 1;
                                continue;
                            }

                            Data[x, y, z].BiomeID = 1;
                            if (y == HeightMap[x, z].x) Data[x, y, z].BlockType = 1;
                            if (y >= HeightMap[x, z].x - 4 && y < HeightMap[x, z].x) Data[x, y, z].BlockType = 2;
                            if (y >= 1 && y < HeightMap[x, z].x - 4) Data[x, y, z].BlockType = 3;
                            if (y == 0) Data[x, y, z].BlockType = 4;
                        }

                        break;

                    case 3: // Mountain

                        for (int y = 255; y >= 0; y--)
                        {

                            if (y > HeightMap[x, z].x)
                            {
                                Data[x, y, z].BiomeID = 3;
                                continue;
                            }

                            Data[x, y, z].BiomeID = 3;
                            if (y == HeightMap[x, z].x) Data[x, y, z].BlockType = 3;
                            if (y >= HeightMap[x, z].x - 4 && y < HeightMap[x, z].x) Data[x, y, z].BlockType = 3;
                            if (y >= 1 && y < HeightMap[x, z].x - 4) Data[x, y, z].BlockType = 3;
                            if (y == 0) Data[x, y, z].BlockType = 4;
                        }

                        break;
                }
            }
        }





        //Apply Any Structures in the chunk
        for (int i = 0; i < GenerationFeatures.Length; i++)
        {
            Vector2Int randpos = RandomPosInChunk();
            Vector2Int LastPos = randpos;
            for (int c = 0; c <= GenerationFeatures[i].ExpectedNumberPerChunk; c++)
            {
                if (HeightMap[randpos.x, randpos.y].y == GenerationFeatures[i].HousingBiome)
                {
                    if(WorldGenRandom.NextDouble(0, 1) <= GenerationFeatures[i].ChanceToGenerate)
                    {
                        if (Vector2Int.Distance(randpos, LastPos) > GenerationFeatures[i].GenDistance)
                        {
                            WorldStructureGenerator.StructuresToBuild.Enqueue(new Structure(GenerationFeatures[i].StructuralBlockPositions, new Vector3Int(randpos.x + Position.ToVector2Int().x * 16, HeightMap[randpos.x, randpos.y].x + GenerationFeatures[i].yOffset, randpos.y + Position.ToVector2Int().y * 16)));
                        }
                    }
                }

                LastPos = randpos;
                randpos = RandomPosInChunk();
            }
        }

        //Create Ores
        for (int x = 0; x < 16; x++)
        {
            for(int y = 0; y < 256; y++)
            {
                for(int z = 0; z < 16; z++)
                {
                    if(y < HeightMap[x,z].x - 5 && y > 1)
                    {
                        if(y < HeightMap[x, z].x - 10)
                        {
                            if(WorldGenRandom.NextDouble(0, 100) < CoalChance)
                            {
                                Data[x, y, z].BlockType = 16;
                            }
                        }

                        if (y < HeightMap[x, z].x - 20)
                        {
                            if (WorldGenRandom.NextDouble(0, 100) < IronChance)
                            {
                                Data[x, y, z].BlockType = 17;
                            }
                        }

                        if (y < HeightMap[x, z].x - 50)
                        {
                            if (WorldGenRandom.NextDouble(0, 100) < DiamondChance)
                            {
                                Data[x, y, z].BlockType = 18;
                            }
                        }
                    }
                }
            }
        }


        //Create Caves

        return Data;
    }

    private static Vector2Int RandomPosInChunk()
    {
        return new Vector2Int(WorldGenRandom.NextInt(0, 16), WorldGenRandom.NextInt(0, 16));
    }

    #endregion
}

// Im a fucking vampire