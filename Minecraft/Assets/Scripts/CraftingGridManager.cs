using static ItemManager;

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

public class CraftingGridManager : MonoBehaviour
{

    [System.Serializable]
    public class CraftingCell
    {
        public InventoryCellClass HousingCell;
        public int CraftingGridIndex;
    }

    public List<CraftingCell> CraftingCells;
    
    public InventoryCellClass ResultCell;
    public InventoryCellClass TransportCell;
    public HotbarNavigator HotBarManager;

    private int[] Empty = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    private void Awake()
    {
        foreach (CraftingCell cell in CraftingCells)
        {
            cell.HousingCell.OnChanged += CellChangedInvoke;
        }

        ResultCell.OnClick += RemoveFromGrid;
    }

    public void RemoveFromGrid(bool QuickSlot = false)
    {
        if(QuickSlot)
        {
            if(HotBarManager.HotBarCellRoots[HotBarManager.PointedHotbarIndex].Stack != null)
            {
                if (HotBarManager.HotBarCellRoots[HotBarManager.PointedHotbarIndex].Stack.Count < 64)
                {
                    foreach (CraftingCell CellClass in CraftingCells)
                    {
                        if (CellClass.HousingCell.Stack != null)
                        {
                            CellClass.HousingCell.Stack.Count -= 1;
                            CellClass.HousingCell.UpdateCell();
                        }
                    }
                }
            }
            else
            {
                foreach (CraftingCell CellClass in CraftingCells)
                {
                    if (CellClass.HousingCell.Stack != null)
                    {
                        CellClass.HousingCell.Stack.Count -= 1;
                        CellClass.HousingCell.UpdateCell();
                    }
                }
            }
        }
        else
        {
            if (TransportCell.Stack.Count < 64)
            {
                foreach (CraftingCell CellClass in CraftingCells)
                {
                    if (CellClass.HousingCell.Stack != null)
                    {
                        CellClass.HousingCell.Stack.Count -= 1;
                        CellClass.HousingCell.UpdateCell();
                    }
                }
            }
        }
    }

    public async void CellChangedInvoke()
    {
        List<InventoryCellClass> CellsToUpdate = new List<InventoryCellClass>();
        Task t = Task.Factory.StartNew
        (
            delegate
            {

                int[] ItemIDs = new int[9];

                for(int i = 0; i < CraftingCells.Count; i++)
                {
                    if(CraftingCells[i].HousingCell.Stack != null)
                        ItemIDs[CraftingCells[i].CraftingGridIndex] = CraftingCells[i].HousingCell.Stack.CurrentStackedItem.GetID();
                }

                //Run the dictitonary lookup for a recipe
                StringBuilder builder = new StringBuilder();
                string BuilderResult;

                for (int i = 0; i < ItemIDs.Length; i++)
                {
                    builder.Append($"{ItemIDs[i]}");
                }

                BuilderResult = builder.ToString();

                if (Instance.CraftingRecipesDict.ContainsKey(BuilderResult))
                {
                    if(ResultCell.Stack == null)
                    {
                        RecipeResult result = Instance.CraftingRecipesDict[BuilderResult];
                        ResultCell.Stack = new ItemStack(Instance.ItemsDict[result.ItemResultID], result.Count);
                        CellsToUpdate.Add(ResultCell);
                    }
                }
                else
                {
                    ResultCell.Stack = null;
                    CellsToUpdate.Add(ResultCell);
                }
            }
        );

        await t;

        if(CellsToUpdate.Count > 0)
        {
            foreach(InventoryCellClass Cell in CellsToUpdate)
            {
                Cell.UpdateCell();
            }
        }

    }

    public bool GridEmpty()
    {
        foreach(CraftingCell cell in CraftingCells)
        {
            if(cell.HousingCell.Stack != null)
            {
                return false;
            }
        }

        return true;
    }

    //public Stack[] ()
    //{
    //    foreach (CraftingCell cell in CraftingCells)
    //    {
    //        if (cell.HousingCell.Stack != null)
    //        {
    //            return false;
    //        }
    //    }
    //
    //    return true;
    //}

}
