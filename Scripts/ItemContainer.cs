using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemContainer : MonoBehaviour
{
    private string itemName;
    public string ItemName
    {
        get { return itemName; }
        set
        {
            if (value == null)
                Disable();
            else
            {
                Item I;
                if (InventoryController.current.itemByName.TryGetValue(value, out I))
                {
                    item = I;
                    itemName = I.name;
                    Image = I.image;
                }
                else
                    Debug.Log(value + " is not an Item");
            }
            if (Updated != null)
                Updated.Invoke(item, quan);
        }
    }

    private Item item;
    public Item Item
    {
        get { return item; }
        set
        {
            if (value == null)
                Disable();
            else
            {
                item = value;
                itemName = value.name;
                Image = value.image;
            }
            if (Updated != null)
                Updated.Invoke(item, quan);
        }
    }

    private int quan;
    public int Quantity
    {
        get { return quan; }
        set
        {
            if (value <= 0)
                Disable();
            else
            {
                quan = value;
                if (quanText != null)
                    quanText.text = "X" + quan;
            }
            if (Updated != null)
                Updated.Invoke(item, quan);
        }
    }

    [Header("--- ItemContainer ---")]
    public Image image;
    public Sprite Image
    {
        get { return image.sprite; }
        private set
        {
            image.sprite = value;
            image.color = new Color(1, 1, 1, 1);
            if (quanBackground != null)
                quanBackground.enabled = true;
            if (value == null)
            {
                image.color = new Color(0, 0, 0, 0);
                if (quanBackground != null)
                    quanBackground.enabled = false;
            }
        }
    }
    public TMP_Text quanText;
    public Image quanBackground;

    public Action<Item, int> Updated;

    public void Disable()
    {
        quan = 0;
        item = null;
        Image = null;
        itemName = null;
        quanText.text = "";
    }
    /// <summary>
    ///   Tries to fill or add if names match else return false
    /// </summary>
    public bool Fill(string itemName, int quantity)
    {
        if (quan <= 0)
        {
            ItemName = itemName;
            Quantity = quantity;
        }
        else if (itemName == this.itemName && quan + quantity <= Item.maxStack)
            Quantity += quantity;
        else 
            return false;
        return true;
    }
    /// <summary>
    ///   Returns what was taken
    /// </summary>
    public int Take(int amount)
    {
        int take = Mathf.Clamp(amount, 0, quan);
        Quantity -= take;
        return take;
    }
    /// <summary>
    ///   Fills with what it can take and returns the rest
    /// </summary>
    public int Give(int amount)
    {
        int toTake = Mathf.Min(amount, Item.maxStack - quan);
        Quantity += toTake;
        return amount - toTake;
    }
    /// <summary>
    ///   Swaps the Item and quantity with the provided ItemContainer
    /// </summary>
    public void Swap(ItemContainer itemC)
    {
        int q = quan;
        Item i = item;
        int qSwap = itemC.Quantity;
        Item iSwap = itemC.Item;
        Item = iSwap;
        Quantity = qSwap;
        itemC.Item = i;
        itemC.Quantity = q;
    }

}
