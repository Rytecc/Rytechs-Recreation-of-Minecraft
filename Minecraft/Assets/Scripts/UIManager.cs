using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public enum InventoryType
    {
        None = 0,
        CraftingTableUI = 1,
        FurnaceUI = 2
    }

    [SerializeField]
    private GameObject DebugMenu;
    [Space]
    [SerializeField]
    private HotbarNavigator HotBar;
    [SerializeField]
    private InventoryCellClass TransportCell;
    [Space]
    [SerializeField]
    private GameObject InventoryUI;
    [SerializeField]
    private Movement PlayerMovement;
    [SerializeField]
    private PlayerBlockEditor PlayerTerrainEditor;
    [Space]
    [SerializeField] private CraftingGridManager TwoByTwoManager;
    [SerializeField] private CraftingGridManager ThreeByThreeManager;
    [Space]
    [SerializeField]
    private GameObject[] Interfaces;

    private InventoryType CurrentInventoryOpen;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F3))
        {
            DebugMenu.SetActive(!DebugMenu.activeInHierarchy);
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            Toggle(InventoryType.None);
        }

    }

    public void Toggle(InventoryType TypeToEnable)
    {
        foreach (GameObject Interface in Interfaces)
        {
            if (Interface) Interface.SetActive(false);
        }

        InventoryUI.SetActive(!InventoryUI.activeInHierarchy);

        Interfaces[(int)TypeToEnable].SetActive(!Interfaces[(int)TypeToEnable].activeSelf);

        PlayerTerrainEditor.enabled = !PlayerTerrainEditor.enabled;
        PlayerMovement.enabled = !PlayerMovement.enabled;
        PlayerMovement.PlayerInput = Vector2.zero;
        Cursor.lockState = PlayerMovement.enabled ? CursorLockMode.Locked : CursorLockMode.None;

        if (!InventoryUI.activeSelf)
        {
            //Drop all items in crafting grids. and transport cells.
            switch(TypeToEnable)
            {
                case InventoryType.None:
                    foreach (CraftingGridManager.CraftingCell cell in TwoByTwoManager.CraftingCells)
                    {
                        HotBar.DropItemBatch(cell.HousingCell);
                    }

                    HotBar.DropItemBatch(TwoByTwoManager.ResultCell);

                    break;
                case InventoryType.CraftingTableUI:
                    foreach(CraftingGridManager.CraftingCell cell in ThreeByThreeManager.CraftingCells)
                    {
                        HotBar.DropItemBatch(cell.HousingCell);
                    }

                    HotBar.DropItemBatch(ThreeByThreeManager.ResultCell);
                    break;
            }
        }

        HotBar.DropItemBatch(TransportCell);

        CurrentInventoryOpen = TypeToEnable;
    }

    public void Toggle(InventoryType TypeToEnable, Vector3Int BlockPosition)
    {

        if(TypeToEnable == InventoryType.FurnaceUI)
        {
            Interfaces[(int)TypeToEnable].GetComponent<FurnaceInterface>().onFurnaceOpen(BlockPosition);
        }

        foreach (GameObject Interface in Interfaces)
        {
            if (Interface) Interface.SetActive(false);
        }

        InventoryUI.SetActive(!InventoryUI.activeInHierarchy);

        Interfaces[(int)TypeToEnable].SetActive(!Interfaces[(int)TypeToEnable].activeSelf);

        PlayerTerrainEditor.enabled = !PlayerTerrainEditor.enabled;
        PlayerMovement.enabled = !PlayerMovement.enabled;
        PlayerMovement.PlayerInput = Vector2.zero;
        Cursor.lockState = PlayerMovement.enabled ? CursorLockMode.Locked : CursorLockMode.None;

        if (!InventoryUI.activeSelf)
        {
            //Drop all items in crafting grids. and transport cells.
            switch (TypeToEnable)
            {
                case InventoryType.None:
                    foreach (CraftingGridManager.CraftingCell cell in TwoByTwoManager.CraftingCells)
                    {
                        HotBar.DropItemBatch(cell.HousingCell);
                    }

                    HotBar.DropItemBatch(TwoByTwoManager.ResultCell);

                    break;
                case InventoryType.CraftingTableUI:
                    foreach (CraftingGridManager.CraftingCell cell in ThreeByThreeManager.CraftingCells)
                    {
                        HotBar.DropItemBatch(cell.HousingCell);
                    }

                    HotBar.DropItemBatch(ThreeByThreeManager.ResultCell);
                    break;
            }
        }

        HotBar.DropItemBatch(TransportCell);

        CurrentInventoryOpen = TypeToEnable;
    }
}
