using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingButton : MonoBehaviour
{
    public List<ItemAndQuantity> requirements;
    public List<ItemAndQuantity> product;

    public void TryCraft()
    {
        foreach (ItemAndQuantity IQ in requirements)
            if (!InventoryController.current.CouldTake(IQ.item.name, IQ.quantity))
                return;
        foreach (ItemAndQuantity IQ in product)
            if (!InventoryController.current.CouldGive(IQ.item.name, IQ.quantity))
                return;

        foreach (ItemAndQuantity IQ in requirements)
            InventoryController.current.TryTake(IQ.item.name, IQ.quantity);
        foreach (ItemAndQuantity IQ in product)
            InventoryController.current.TryGive(IQ.item.name, IQ.quantity);

    }
}

[System.Serializable]
public struct ItemAndQuantity
{
    public Item item;
    public int quantity;
}