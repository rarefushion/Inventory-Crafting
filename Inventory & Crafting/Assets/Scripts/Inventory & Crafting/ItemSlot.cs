using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ItemSlot: MonoBehaviour
{
    //If you change quanity manually remember to update quanityText and InventoryController.itemByName.quanity
    public int quanity;
    public TMP_Text quanityText;
    public Item item;
    public Image image;
    public InventoryController inventory;
    //ItemSlot state or location
    public bool isInventory = false;
    public bool isCrafting = false;

    public void Fill(Item I, int quan)
    {
        image.gameObject.SetActive(true);
        quanityText.gameObject.SetActive(true);
        quanity = quan;
        quanityText.text = quanity.ToString();
        item = I;
        image.sprite = item.image;
        if (isInventory) inventory.LogItem(this, quan, true);
    }

    public int Add(int quan)
    {
        int left;
        if (quan + quanity > item.maxStack)
        {
            left = (quan + quanity) - item.maxStack;
            if (isInventory) inventory.LogItem(this, quan - left, false);
            quanity = Mathf.Clamp(quan + quanity, 0, item.maxStack);
        }
        else
        {
            left = 0;
            if (isInventory) inventory.LogItem(this, quan, false);
            quanity += quan;
        }
        quanityText.text = quanity.ToString();
        return left;
    }

    public int Split(int request)
    {
        int give = Mathf.Clamp(request, 0, quanity);
        if (isInventory) inventory.LogItem(this, -give, false);
        quanity = Mathf.Clamp(quanity - request, 0, item.maxStack);
        quanityText.text = quanity.ToString();
        if(quanity == 0) Clear();
        return give;
    }

    public void Swap(ItemSlot toSwap)
    {
        if (isInventory && !toSwap.isInventory) inventory.LogItem(this, -quanity, false);
        else if (!isInventory && toSwap.isInventory) inventory.LogItem(toSwap, -toSwap.quanity, false);
        string itemName = item.name;
        int quan = quanity;
        //Set this
        quanity = toSwap.quanity;
        quanityText.text = quanity.ToString();
        item = toSwap.item;
        image.sprite = item.image;
        //Set toSwap
        toSwap.quanity = quan;
        toSwap.quanityText.text = toSwap.quanity.ToString();
        toSwap.item = inventory.itemByName[itemName].item;
        toSwap.image.sprite = toSwap.item.image;
    }

    public void Clear()
    {
        inventory.LogItem(this, -quanity, false);
        quanity = 0;
        quanityText.text = quanity.ToString();
        item = null;
        image.sprite = null;
        image.gameObject.SetActive(false);
        quanityText.gameObject.SetActive(false);
    }
}
