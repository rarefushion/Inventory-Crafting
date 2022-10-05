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
    public bool isInventory = false;
    //Click controlled by Slot prefab button component
    public void Click()
    {
        //First check if advanced split is open
        if (inventory.advancedSplitItemSlot != null)
        {
            inventory.advancedSplitItemSlot = null;
            inventory.advancedSplitObject.gameObject.SetActive(false);
        }

        if (Input.GetKey(KeyCode.LeftShift) && inventory.activeStorage != null && item != null)
        {
            ItemSlot firstOpen = null;
            if (isInventory)
            {
                foreach (Transform slotOBJ in inventory.activeStorage.content)
                {
                    ItemSlot storageSlot = slotOBJ.GetComponent<ItemSlot>();
                    if (storageSlot.item != null) 
                    {
                        if (storageSlot.item.name != item.name) continue;
                        storageSlot.Add(Split(Mathf.Clamp(quanity, 0, storageSlot.item.maxStack - storageSlot.quanity)));
                        if (item == null) break;
                        continue;
                    }
                    if (storageSlot.item == null && firstOpen == null) firstOpen = storageSlot;
                }
                if (item != null && firstOpen != null) firstOpen.Fill(item, Split(quanity));
            }
            else
            {
                foreach (Transform slotOBJ in inventory.content)
                {
                    ItemSlot invSlot = slotOBJ.GetComponent<ItemSlot>();
                    if (invSlot.item != null) 
                    {
                        if (invSlot.item.name != item.name) continue;
                        invSlot.Add(Split(Mathf.Clamp(quanity, 0, invSlot.item.maxStack - invSlot.quanity)));
                        if (item == null) break;
                        continue;
                    }
                    if (invSlot.item == null && firstOpen == null) firstOpen = invSlot;
                }
                if (item != null && firstOpen != null) firstOpen.Fill(item, Split(quanity));
            }
        }
        else if (Cursor.itemSlot.item == null)
        {
            if (item != null)
            {
                Cursor.itemSlot.Fill(item, quanity);
                Clear();
            }
        }
        else
        {
            if (item == null)
            {
                Fill(Cursor.itemSlot.item, Cursor.itemSlot.quanity);
                Cursor.itemSlot.Clear();
            }
            else if (Cursor.itemSlot.item.name == item.name)
            {
                if(quanity == item.maxStack)
                {    
                    Swap(Cursor.itemSlot);
                } 
                else 
                {
                    int quan = Cursor.itemSlot.Split(Mathf.Clamp(Cursor.itemSlot.quanity, 0, item.maxStack - quanity));
                    Add(quan);
                }
            }
            else 
            {    
                Swap(Cursor.itemSlot);
            } 
        }
    }
    //Right click triggered by InventoryController Update raycast as well as advanced item split
    public void RightClick()
    {
        //First check if advanced split is open
        if (inventory.advancedSplitItemSlot != null)
        {
            inventory.advancedSplitItemSlot = null;
            inventory.advancedSplitObject.gameObject.SetActive(false);
        }
        
        if (item != null)
        {
            if (Cursor.itemSlot.item == null) 
            {
                string itemName = item.name;
                int quan = Split(Mathf.Clamp((quanity + 1) / 2, 0, item.maxStack));
                Cursor.itemSlot.Fill(inventory.itemByName[itemName].item, quan);
            }
            else if (Cursor.itemSlot.item.name == item.name) 
            {
                int quan = Cursor.itemSlot.Split(Mathf.Clamp(1, 0, item.maxStack - quanity));
                Add(quan);
            }
        }
        else if (Cursor.itemSlot.item != null) 
        {
            string itemName = Cursor.itemSlot.item.name;
            int quan =  Cursor.itemSlot.Split(1);
            Fill(inventory.itemByName[itemName].item, quan);
        }
    }

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
        //No need to log new item since we're using fill
        if (isInventory && !toSwap.isInventory) inventory.LogItem(this, -quanity, false);
        else if (!isInventory && toSwap.isInventory) inventory.LogItem(toSwap, -toSwap.quanity, false);
        string itemName = item.name;
        int quan = quanity;
        Fill(toSwap.item, toSwap.quanity);
        toSwap.Fill(inventory.itemByName[itemName].item, quan);
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
