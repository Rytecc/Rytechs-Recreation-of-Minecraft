using System.Reflection;
using UnityEngine;
using System;

public class Furnace : OnTick
{
    public TerrainChunk HousingChunk;
    public Vector3Int BlockPosition;

    public ItemStack SmeltProduce; // 3
    public ItemStack Fuel; // 4
    public ItemStack OutputStack; // 5
    public Item CurrentCookingProduce; // 6
    public Item CurrentBurningFuel; // 7
    public float SmeltProgress; // 8
    public int BurnTime; // 9
    long Ticks; // 10

    public bool IsInitialized()
    {
        return Initialized;
    }

    private bool Initialized = false;

    private FieldInfo[] Fields;

    Vector3Int LocalPos;
    public void Init(object Params)
    {

        Fields = typeof(Furnace).GetFields(BindingFlags.Public);

        BlockPosition = (Vector3Int)Params;

        HousingChunk = WorldGenerator.GetChunk(BlockPosition);
        WorldGenerator.GetLocalBlockPosition(BlockPosition, out _, out LocalPos);
        Initialized = true;
    }

    public TerrainChunk Tick()
    {
        Ticks++;
        if (BurnTime > 0)
        {
            //Check the FuelCell and ProduceCell to see if there is fuel,
            //to see if the furnace dictionary contains the produceCell's ItemID, if it does,
            //If the burntime is 0, set it to the burntime based off of the FuelCell's Fuel Properties

            //Does the furnace recipe exist?

            if (Ticks % 10 == 0)
            {
                SmeltProgress += 1f;
                BurnTime--;
            }

            if(SmeltProgress >= 100f)
            {
                if (OutputStack == null)
                {
                    OutputStack = new ItemStack(ItemManager.Instance.ItemsDict[ItemManager.Instance.SmeltingRecipesDict[SmeltProduce.CurrentStackedItem.GetID()].Itemresult], 1);
                }
                else
                {
                    if (OutputStack.CurrentStackedItem.GetID() == CurrentCookingProduce.GetID())
                    {
                        if (OutputStack.Count < 64)
                        {
                            OutputStack.Count++;
                        }
                        else
                        {
                            //Freeze the furnace
                        }
                    }
                    else
                    {
                        //Freeze the furnace
                    }
                }

                SmeltProgress = 0;
            }
        }
        else
        {
            if (Fuel != null && Fuel.CurrentStackedItem.GetProperties().FuelProperties != null &&
                ItemManager.Instance.SmeltingRecipesDict.ContainsKey(SmeltProduce.CurrentStackedItem.GetID()))
            {
                BurnTime = Fuel.CurrentStackedItem.GetProperties().FuelProperties.BurnTime;

                CurrentCookingProduce = SmeltProduce.CurrentStackedItem;
                CurrentBurningFuel = Fuel.CurrentStackedItem;

                SmeltProduce.Count--;
                Fuel.Count--;

                if(Fuel.Count == 0)
                {
                    Fuel = null;
                }

                if (SmeltProduce.Count == 0)
                {
                    SmeltProduce = null;
                }
            }
        }

        return HousingChunk;
    }

    public object GetProperty(int PropertyIndex)
    {
        try
        {
            return Fields[PropertyIndex].GetValue(this);
        }
        catch (Exception)
        {
            Debug.Log("Failed to get property.");
            return null;
        }
    }

    public void SetProperty(object Value, int PropertyIndex)
    {
        try {
            Fields[PropertyIndex].SetValue(this, Value);
        } catch(Exception) {
            Debug.Log("Failed to set property.");
        }
    }
}
