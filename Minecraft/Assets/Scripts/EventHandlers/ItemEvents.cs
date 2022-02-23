using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEvents : MonoBehaviour
{

    public static ItemEvents Instance;

    public delegate void BlockBreak(int BlockID, Vector3Int BlockPosition);
    public event BlockBreak BlockBreakEvent;

    [SerializeField] private GameObject DroppedItemPrefab;

    private void Awake()
    {
        Instance = this;
        BlockBreakEvent += DropItem;
    }

    public void InvokeBreakEvent(int ID, Vector3Int BlockPosition)
    {
        BlockBreakEvent.Invoke(ID, BlockPosition);
    }

    public void DropItem(int ItemID, Vector3Int BlockPosition)
    {
        DroppedItemClass droppeditem = Instantiate(DroppedItemPrefab, BlockPosition, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360))).GetComponent<DroppedItemClass>();
        droppeditem.SetIdentity(ItemManager.Instance.ItemsDict[ItemID]);
    }
}
