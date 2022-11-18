using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ItemSlot: MonoBehaviour
{
    //If you change quantity manually remember to update quantityText and InventoryController.itemByName.quantity
    public int quantity;
    public TMP_Text quantityText;
    public Item item;
    public Image image;
    public InventoryController inventory;
    //ItemSlot state or location
    public bool isInventory = false;
    public bool isCrafting = false;
    //Instantiate a new item and quantity for this itemSlot
    public void Fill(Item I, int quan)
    {
        image.gameObject.SetActive(true);
        quantityText.gameObject.SetActive(true);
        quantity = quan;
        quantityText.text = quantity.ToString();
        item = I;
        image.sprite = item.image;
        if (isInventory) inventory.LogItem(this, quan, true);
    }
    //Add a number and return the left overs
    public int Add(int quan)
    {
        int left;
        if (quan + quantity > item.maxStack)
        {
            left = (quan + quantity) - item.maxStack;
            if (isInventory) inventory.LogItem(this, quan - left, false);
            quantity = Mathf.Clamp(quan + quantity, 0, item.maxStack);
        }
        else
        {
            left = 0;
            if (isInventory) inventory.LogItem(this, quan, false);
            quantity += quan;
        }
        quantityText.text = quantity.ToString();
        return left;
    }
    //Request a quantity and return what can be given
    public int Split(int request)
    {
        int give = Mathf.Clamp(request, 0, quantity);
        if (isInventory) inventory.LogItem(this, -give, false);
        quantity = Mathf.Clamp(quantity - request, 0, item.maxStack);
        quantityText.text = quantity.ToString();
        if(quantity == 0) Clear();
        return give;
    }
    //Swap items and quantities of this slot and the input slot
    public void Swap(ItemSlot toSwap)
    {
        string itemName = item.name;
        int quan = quantity;
        //Set this
        quantity = toSwap.quantity;
        quantityText.text = quantity.ToString();
        item = toSwap.item;
        image.sprite = item.image;
        //Set toSwap
        toSwap.quantity = quan;
        toSwap.quantityText.text = toSwap.quantity.ToString();
        toSwap.item = inventory.itemByName[itemName].item;
        toSwap.image.sprite = toSwap.item.image;
        if (isInventory && !toSwap.isInventory) 
        {
            inventory.LogItem(this, quantity, false);
            inventory.LogItem(this, -toSwap.quantity, false);
        }
        else if (!isInventory && toSwap.isInventory) 
        {
            inventory.LogItem(toSwap, -quantity, false);
            inventory.LogItem(toSwap, toSwap.quantity, false);
        }
    }

    public void Clear()
    {
        if (isInventory) inventory.LogItem(this, -quantity, false);
        quantity = 0;
        quantityText.text = quantity.ToString();
        item = null;
        image.sprite = null;
        image.gameObject.SetActive(false);
        quantityText.gameObject.SetActive(false);
    }
}
