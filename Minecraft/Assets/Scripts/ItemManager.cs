using static ItemProperties;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class ItemManager : MonoBehaviour
{

    [System.Serializable]
    public class BlockProperties
    {
        public bool Explodable = true;
        public bool Breakable = true;
        public TOOLTYPE HarvestingTool;
        public int HarvestLevel = 0;
        public float NonHarvestToolBreakSpeed = 1;
        public float HarvestToolBreakSpeed = 2;
    }

    [System.Serializable]
    public class RecipeResult
    {
        public int ItemResultID;
        public int Count;
    }

    [System.Serializable]
    public class ItemDictValue
    {
        public int Key;
        public Item Value;
    }

    [System.Serializable]
    public class BlockDictValue
    {
        public int Key;
        public BlockProperties Value;
    }

    [System.Serializable]
    public class SmeltingResult
    {
        public string RecipeName = "Recipe";
        public int Itemresult;
    }

    [System.Serializable]
    public class SmeltingDict
    {
        public SmeltingResult Value;
        public int Key;
    }

    [System.Serializable]
    internal class CraftingDict
    {
        public string RecipeName;
        public List<int> Key;
        public RecipeResult Value;
    }

    [System.Serializable]
    internal class ImageDisplayDictValue
    {
        public int Key;
        public Sprite Value;
    }


    public static ItemManager Instance;
    [SerializeField] private SmeltingDict[] SmeltingDictionary;
    [SerializeField] private ImageDisplayDictValue[] ItemImagesDictionary;
    [SerializeField] private BlockDictValue[] BlockPropertiesDictionary;
    [SerializeField] private ItemDictValue[] ItemsDictionary;
    [SerializeField] private CraftingDict[] CraftingDictionary;

    public Dictionary<int, SmeltingResult> SmeltingRecipesDict;
    public Dictionary<string, RecipeResult> CraftingRecipesDict;
    public Dictionary<int, BlockProperties> BlockPropertiesDict;
    public Dictionary<int, Item> ItemsDict;
    public Dictionary<int, Sprite> ItemImages;

    private void Awake()
    {
        Instance = this;

        CraftingRecipesDict = new Dictionary<string, RecipeResult>();
        ItemsDict = new Dictionary<int, Item>();
        ItemImages = new Dictionary<int, Sprite>();
        BlockPropertiesDict = new Dictionary<int, BlockProperties>();

        foreach(ImageDisplayDictValue temp in ItemImagesDictionary)
        {
            ItemImages.Add(temp.Key, temp.Value);
        }

        foreach(BlockDictValue temp in BlockPropertiesDictionary)
        {
            BlockPropertiesDict.Add(temp.Key, temp.Value);
        }

        foreach(ItemDictValue temp in ItemsDictionary)
        {
            ItemsDict.Add(temp.Key, temp.Value);
        }

        //foreach (SmeltingDict temp in SmeltingDictionary)
        //{
        //    SmeltingRecipesDict.Add(temp.Key, temp.Value);
        //}

        StringBuilder builder = new StringBuilder();

        foreach(CraftingDict temp in CraftingDictionary)
        {

            for(int i = 0; i < 9; i++)
            {
                builder.Append($"{temp.Key[i]}");
            }

            CraftingRecipesDict.Add(builder.ToString(), temp.Value);
            builder.Clear();
        }

    }
}
