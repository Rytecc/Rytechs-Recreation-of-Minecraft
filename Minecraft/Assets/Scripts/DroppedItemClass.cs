using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DroppedItemClass : MonoBehaviour
{

    private Item StoredItem;

    [SerializeField] private MeshRenderer Pickaxe;
    [SerializeField] private MeshRenderer Axe;
    [SerializeField] private MeshRenderer Sword;
    [SerializeField] private MeshRenderer Shovel;
    [Space]
    [SerializeField] private SpriteRenderer Top;
    [SerializeField] private SpriteRenderer Bottom;
    [SerializeField] private SpriteRenderer Left;
    [SerializeField] private SpriteRenderer Right;
    [SerializeField] private SpriteRenderer Forward;
    [SerializeField] private SpriteRenderer Back;
    [Space]
    [SerializeField] private SpriteRenderer FlatForward;
    [SerializeField] private SpriteRenderer FlatBack;
    [Space]
    [SerializeField] private BoxCollider ItemCollider;
    [SerializeField] private Material[] ToolTiers;

    public async void PickupItem(InventoryClass ChosenInventory)
    {
        InventoryCellClass ClassToUpdate = null;
        Task PickupItemTask = Task.Factory.StartNew
        (
            delegate
            {
                // Look for a cell in the hotbar with the same itemstack
                foreach (InventoryCellClass cell in ChosenInventory.HotbarManager.HotBarCellRoots)
                {
                    if (cell.Stack != null)
                    {
                        if (cell.Stack.CurrentStackedItem.GetID() == StoredItem.GetID())
                        {
                            if (cell.Stack.CurrentStackedItem.GetProperties().Stackable && cell.Stack.Count < 64)
                            {
                                lock(cell)
                                {
                                    if (!cell.Accessed)
                                    {
                                        cell.Stack.AddToStack(1, out _);
                                        ClassToUpdate = cell;
                                        cell.Accessed = true;
                                    }
                                }


                                return;
                            }
                        }
                    }
                }

                // Look for the first empty cell
                foreach (InventoryCellClass cell in ChosenInventory.HotbarManager.HotBarCellRoots)
                {
                    if (cell.Stack == null)
                    {
                        lock(cell)
                        {
                            if (!cell.Accessed)
                            {
                                cell.Stack = new ItemStack(StoredItem, 1);
                                cell.Accessed = true;
                                ClassToUpdate = cell;
                            }
                        }


                        return;
                    }
                }

                // Repeat the same process for the inventory instead of the hotbar

                // Look for a cell in the hotbar with the same itemstack
                foreach (InventoryCellClass cell in ChosenInventory.InventoryCells)
                {
                    if (cell.Stack != null)
                    {
                        if (cell.Stack.CurrentStackedItem.GetID() == StoredItem.GetID())
                        {
                            if (cell.Stack.CurrentStackedItem.GetProperties().Stackable && cell.Stack.Count < 64)
                            {
                                lock(cell)
                                {
                                    if (!cell.Accessed)
                                    {
                                        cell.Stack.AddToStack(1, out _);
                                        cell.Accessed = true;
                                        ClassToUpdate = cell;
                                    }
                                }


                                return;
                            }
                        }
                    }
                }

                // Look for the first empty cell
                foreach (InventoryCellClass cell in ChosenInventory.InventoryCells)
                {
                    if (cell.Stack == null)
                    {
                        if(!cell.Accessed)
                        {
                            lock(cell)
                            {
                                cell.Stack = new ItemStack(StoredItem, 1);
                                cell.Accessed = true;
                                ClassToUpdate = cell;
                            }
                        }

                        return;
                    }
                }
            }
        );

        await PickupItemTask;

        try
        {
            if (ClassToUpdate)
            {
                ClassToUpdate.UpdateCell();
                ClassToUpdate.Accessed = false;
                if (gameObject) Destroy(gameObject);
            }
        }
        catch (System.NullReferenceException) { }


    }

    public void SetIdentity(Item item)
    {
        if(item != null)
        {
            if (item.GetID() > 0)
            {
                StoredItem = item;
                ItemCollider.size = new Vector3(1, 1, 1);

                if (item.GetProperties().type == ItemProperties.ITEMTYPE.Block)
                {
                    SetBlockItemDisplay(item.GetID());

                    Pickaxe.enabled = false;
                    Axe.enabled = false;
                    Sword.enabled = false;
                    Shovel.enabled = false;
                    ToggleBlockDisplay(true);
                    ToggleItemDisplay(false);
                }
                else if (item.GetProperties().type == ItemProperties.ITEMTYPE.Tool)
                {
                    ToggleBlockDisplay(false);
                    Pickaxe.enabled = false;
                    Axe.enabled = false;
                    Sword.enabled = false;
                    Shovel.enabled = false;
                    ToggleItemDisplay(false);

                    MeshRenderer TargetRenderer = null;
                    switch (item.GetProperties().tooltype)
                    {
                        case ItemProperties.TOOLTYPE.Axe:
                            Axe.enabled = true;
                            TargetRenderer = Axe;
                            break;

                        case ItemProperties.TOOLTYPE.Pickaxe:
                            Pickaxe.enabled = true;
                            TargetRenderer = Pickaxe;
                            break;

                        case ItemProperties.TOOLTYPE.Shovel:
                            Shovel.enabled = true;
                            TargetRenderer = Shovel;
                            break;

                        case ItemProperties.TOOLTYPE.Offense:
                            Sword.enabled = true;
                            TargetRenderer = Sword;
                            break;
                    }

                    TargetRenderer.material = ToolTiers[item.GetProperties().MaterialLevel - 1];
                }
                else if (item.GetProperties().type == ItemProperties.ITEMTYPE.Material)
                {
                    ToggleBlockDisplay(false);
                    Pickaxe.enabled = false;
                    Axe.enabled = false;
                    Sword.enabled = false;
                    Shovel.enabled = false;

                    ItemCollider.size = new Vector3(0.1f, 1, 1);
                    ToggleItemDisplay(true);
                    SetItemDisplay(item.GetID());
                }
            }
        }
        else if (item == null || item.GetID() <= 0)
        {
            Pickaxe.enabled = false;
            Axe.enabled = false;
            Sword.enabled = false;
            Shovel.enabled = false;
            ToggleBlockDisplay(false);
            ToggleItemDisplay(false);
        }

    }

    private void SetBlockItemDisplay(int ItemID)
    {
        try
        {
            if (WorldGenerator.BlockUVs.ContainsKey(ItemID))
            {
                BlockMaterial Material = WorldGenerator.BlockUVs[ItemID];

                if (Material.BaseTexture == null)
                {
                    Top.sprite = Material.Top;
                    Bottom.sprite = Material.Bottom;
                    Left.sprite = Material.Left;
                    Right.sprite = Material.Right;
                    Forward.sprite = Material.Forward;
                    Back.sprite = Material.Back;
                }
                else
                {
                    Top.sprite = Material.BaseTexture;
                    Bottom.sprite = Material.BaseTexture;
                    Left.sprite = Material.BaseTexture;
                    Right.sprite = Material.BaseTexture;
                    Forward.sprite = Material.BaseTexture;
                    Back.sprite = Material.BaseTexture;
                }
            }
        }
        catch(System.Exception)
        {
            Debug.Log("Couldn't get Material");
        }

    }

    private void SetItemDisplay(int ItemID)
    {
        try
        {
            FlatForward.sprite = ItemManager.Instance.ItemImages[ItemID];
            FlatBack.sprite = ItemManager.Instance.ItemImages[ItemID];
        }
        catch(System.Exception)
        {
            Debug.Log("Couldn't get material");
        }
    }

    private void ToggleBlockDisplay(bool state)
    {
        Top.enabled = state;
        Bottom.enabled = state;
        Forward.enabled = state;
        Back.enabled = state;
        Left.enabled = state;
        Right.enabled = state;
    }

    private void ToggleItemDisplay(bool state)
    {
        FlatForward.enabled = state;
        FlatBack.enabled = state;

        if(state)
        {
            FlatForward.color = new Color(255, 255, 255, 255);
            FlatBack.color = new Color(255, 255, 255, 255);
        }
        else
        {
            FlatForward.color = new Color(0, 0, 0, 0);
            FlatBack.color = new Color(0, 0, 0, 0);
        }

    }

}
