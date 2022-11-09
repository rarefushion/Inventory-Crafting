using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingButton : MonoBehaviour
{
    public List<ItemNameAndQuanity> requirements = new List<ItemNameAndQuanity>();
    public List<ItemNameAndQuanity> product = new List<ItemNameAndQuanity>();

    private InventoryController IC;

    public void Start()
    {
        IC = InventoryController.current;
        if (transform.GetChild(0).GetComponent<Image>().sprite == null) transform.GetChild(0).GetComponent<Image>().sprite = IC.itemByName[product[0].name].item.image;
    }

    public void Clicked()
    {
        //Make sure we have the items before we remove them
        foreach (ItemNameAndQuanity req in requirements)
        {
            if (IC.itemByName[req.name].quanity < req.quanity) return;
        }
        //Make sure there's room in inventory
        List<ItemSlot> openSlot = new List<ItemSlot>();
        foreach (Transform slotT in IC.content)
        {
            if (openSlot.Count >= product.Count) continue;

            if (slotT.GetComponent<ItemSlot>().item == null) openSlot.Add(slotT.GetComponent<ItemSlot>());
            else
            {
                {
                    foreach (ItemNameAndQuanity INAQ in product)
                    {
                        if (slotT.GetComponent<ItemSlot>().item.name == IC.itemByName[INAQ.name].item.name && slotT.GetComponent<ItemSlot>().quanity + INAQ.quanity <= IC.itemByName[INAQ.name].item.maxStack) openSlot.Add(slotT.GetComponent<ItemSlot>());
                    }
                }
            }
        }
        if (openSlot.Count != product.Count) return;
        //Use items to craft
        foreach (ItemNameAndQuanity req in requirements)
        {
            int requested = req.quanity;
            List<ItemSlot> copy = new List<ItemSlot>();
            foreach (ItemSlot slot in IC.itemByName[req.name].locations) copy.Add(slot);
            //Remove all emptied slots from itemByName locations
            foreach (ItemSlot slot in copy) requested -= slot.Split(Mathf.Clamp(requested, 0, slot.quanity));
        }
        //Fill first open slots with products
        for (int i = openSlot.Count - 1; i >= 0; i--)
        {
            if (openSlot[i].item == null) openSlot[i].Fill(IC.itemByName[product[i].name].item, product[i].quanity);
            else if (openSlot[i].item.name == product[i].name) openSlot[i].Add(product[i].quanity);
        }
    }
}
