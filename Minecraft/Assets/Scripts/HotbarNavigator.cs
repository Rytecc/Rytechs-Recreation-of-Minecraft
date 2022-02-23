using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class InventoryCell
{
    public InventoryCell(RectTransform Root, Image ItemDisplay, TextMeshProUGUI ItemCount, InventoryCellClass Stack)
    {
        CellTransform = Root;
        CellItemDisplay = ItemDisplay;
        this.ItemCount = ItemCount;
        ClassContainer = Stack;
    }

    public InventoryCellClass ClassContainer;
    [Space]
    public RectTransform CellTransform;
    public TextMeshProUGUI ItemCount;
    public Image CellItemDisplay;
}

public class HotbarNavigator : MonoBehaviour
{
    public InventoryCellClass[] HotBarCellRoots;
    public InventoryCell[] HotBarItems;

    [SerializeField]
    private RectTransform PointerUI;

    [SerializeField]
    private PlayerBlockEditor BlockEditor;

    [SerializeField]
    private DroppedItemClass ItemClass;

    private float ScrollDir;
    [HideInInspector] public int PointedHotbarIndex;

    [SerializeField] private GameObject DroppedItemPrefab;
    [SerializeField] private Transform HandTip;
    [SerializeField] private Transform CameraForwardTransform;

    private void Start()
    {
        HotBarItems = new InventoryCell[HotBarCellRoots.Length];

        for(int i = 0; i < HotBarItems.Length; i++)
        {
            HotBarItems[i] = new InventoryCell((RectTransform)HotBarCellRoots[i].transform, HotBarCellRoots[i].ItemDisplay, HotBarCellRoots[i].ItemCountDisplay, HotBarCellRoots[i]);
        }

        PointerSwitch();
    }

    private void Update()
    {
        CheckNumInput();
        ScrollDir = Input.mouseScrollDelta.y * -1;

        if(ScrollDir != 0)
        {
            PointedHotbarIndex += (int)ScrollDir;

            if (PointedHotbarIndex < 0)
            {
                PointedHotbarIndex = 9;
            }
            else if (PointedHotbarIndex > 9)
            {
                PointedHotbarIndex = 0;
            }

            PointerSwitch();
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            DropItem(HotBarItems[PointedHotbarIndex].ClassContainer);
        }

    }

    public void DropItemBatch(InventoryCellClass Cell)
    {
        if (Cell.Stack != null)
        {
            while (Cell.Stack.Count > 0)
            {
                Cell.Stack.Count -= 1;
                GameObject droppedItem = Instantiate(DroppedItemPrefab, HandTip.transform.position, Quaternion.identity);
                droppedItem.GetComponent<Rigidbody>().AddForce(CameraForwardTransform.forward * 5f, ForceMode.Impulse);
                droppedItem.GetComponent<Rigidbody>().AddTorque(new Vector3(5, 5, 5), ForceMode.Impulse);
                droppedItem.GetComponent<DroppedItemClass>().SetIdentity(ItemManager.Instance.ItemsDict[Cell.Stack.CurrentStackedItem.GetID()]);
            }
        }

        Cell.UpdateCell();
    }

    public void DropItem(InventoryCellClass Cell)
    {
        if (Cell.Stack != null)
        {
            Cell.Stack.Count -= 1;
            GameObject droppedItem = Instantiate(DroppedItemPrefab, HandTip.transform.position, Quaternion.identity);
            droppedItem.GetComponent<Rigidbody>().AddForce(CameraForwardTransform.forward * 5f, ForceMode.Impulse);
            droppedItem.GetComponent<Rigidbody>().AddTorque(new Vector3(5, 5, 5), ForceMode.Impulse);
            droppedItem.GetComponent<DroppedItemClass>().SetIdentity(ItemManager.Instance.ItemsDict[Cell.Stack.CurrentStackedItem.GetID()]);
        }

        Cell.UpdateCell();
    }

    private void PointerSwitch()
    {
        PointerUI.position = HotBarItems[PointedHotbarIndex].CellTransform.position;
        BlockEditor.BlockToPlace = HotBarItems[PointedHotbarIndex].ClassContainer.Stack != null ? HotBarItems[PointedHotbarIndex].ClassContainer.Stack.CurrentStackedItem.GetID() : 0;

        Item SelectedItem = HotBarItems[PointedHotbarIndex].ClassContainer.Stack != null ?
            HotBarItems[PointedHotbarIndex].ClassContainer.Stack.CurrentStackedItem :
            null;

        ItemClass.SetIdentity(SelectedItem);
    }

    // Code below is hideous
    private void CheckNumInput()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            PointedHotbarIndex = 9;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PointedHotbarIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PointedHotbarIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PointedHotbarIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PointedHotbarIndex = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PointedHotbarIndex = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            PointedHotbarIndex = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            PointedHotbarIndex = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            PointedHotbarIndex = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            PointedHotbarIndex = 8;
        }

        PointerSwitch();
    }
}
