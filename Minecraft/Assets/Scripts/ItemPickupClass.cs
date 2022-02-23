using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickupClass : MonoBehaviour
{
    [SerializeField] private InventoryClass Inventory;
    [SerializeField] private Movement PlayerMovement;
    [SerializeField] private Transform PickupCenter;
    [SerializeField] private float PickupRange = 5;
    [SerializeField] private LayerMask PickupMask;

    private float PickupCooldown = 1f;
    float Delay = 0;

    private void Awake()
    {
        Delay = PickupCooldown;
    }

    private Collider[] ItemsInRange;
    private void Update()
    {

        ItemsInRange = Physics.OverlapSphere(PickupCenter.transform.position, PickupRange, PickupMask);

        if (ItemsInRange.Length > 0)
        {
            if (Delay >= 0)
            {
                Delay -= Time.deltaTime;
                return;
            }

            foreach (Collider c in ItemsInRange)
            {
                c.GetComponent<DroppedItemClass>().PickupItem(Inventory);
            }

            return;
        }

        Delay = PickupCooldown;
    }

}
