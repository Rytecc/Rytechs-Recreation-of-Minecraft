using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceInterface : MonoBehaviour
{

    public InventoryCellClass FuelCell;
    public InventoryCellClass ProduceCell;
    [Space]
    public InventoryCellClass ResultingCell;

    private OnTick ReferencedFurnace;

    private void Start()
    {
        FuelCell.OnChanged += SetFuel;
        ProduceCell.OnChanged += SetProduce;
    }

    public void onFurnaceOpen(Vector3Int ClickedBlockCoords)
    {
        if(TickManager.DynamicBlocksList.ContainsKey(ClickedBlockCoords))
        {
            ReferencedFurnace = TickManager.DynamicBlocksList[ClickedBlockCoords];
            FuelCell.Stack = (ItemStack)ReferencedFurnace.GetProperty(3);
            ProduceCell.Stack = (ItemStack)ReferencedFurnace.GetProperty(2);
        }
    }

    private void LateUpdate()
    {
        Debug.Log(ReferencedFurnace.GetProperty(9));
    }

    public void SetFuel()
    {
        ReferencedFurnace.SetProperty(FuelCell.Stack, 3);
    }

    public void SetProduce()
    {
        ReferencedFurnace.SetProperty(ProduceCell.Stack, 2);
    }
}
