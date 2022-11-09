using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Example : MonoBehaviour
{
    public InventoryController IC;
    public List<Storage> containers = new List<Storage>();
    //If you have more than three items you'll need to add more objects under item count obj
    public Transform itemCounter;

    public void Start()
    {
        IC = InventoryController.current;
        //Check if Storage needs to be initialized
        foreach (Storage c in containers)
        {
            if (IC.itemsToSave.Count - 1 < c.storageID) c.Initialize();
        }
        //Fill slot with random Item
        if (IC.itemByName[IC.items[0].name].quanity == 0)
        {
            foreach (Transform slotOBJ in IC.content)
            {
                ItemSlot slot = slotOBJ.GetComponent<ItemSlot>();
                if (Random.Range(0, 5) == 4)
                {
                    slot.item = null;
                    continue;
                }
                Item I = IC.items[Random.Range(0, 3)];
                int q = Random.Range(1, I.maxStack + 1);
                slot.item = null;
                slot.Fill(I, q);
            }
        }
    }

    public void FixedUpdate() 
    {
        //Item Counter
        for(int i = 4; i > 0; i--)
        {
            Transform counter = itemCounter.GetChild(i - 1);
            counter.GetChild(0).GetComponent<Image>().sprite = IC.items[i - 1].image;
            counter.GetChild(1).GetComponent<TMP_Text>().text = IC.items[i - 1].name + "s: " + IC.itemByName[IC.items[i - 1].name].quanity;
        }   
    }
    
    public void SaveButton()
    {
        //Preperation
        IC.UpdateItemsToSave();
        foreach (Storage S in containers)
        {
            if (S.transform.GetChild(0).gameObject.activeSelf) S.Close();
        }
        //Save
        SaveSystem.Save();
    }
}
