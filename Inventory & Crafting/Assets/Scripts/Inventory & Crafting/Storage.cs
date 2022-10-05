using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Storage : MonoBehaviour
{
    public int storageID;
    public int numberOfSlots;
    public GameObject prefabSlot;
    public Transform content;
    
    public GraphicRaycaster canvasRacaster;
    PointerEventData pointerEventData;
    public EventSystem eventSystem;

    public void Initialize()
    {
        for(int i = numberOfSlots; i > 0; i--)
        {
            ItemSlot slot = Instantiate(prefabSlot, content).GetComponent<ItemSlot>();
            slot.inventory = InventoryController.current;
            slot.item = null;
        }
        InventoryController.current.itemsToSave.Add(new List<ItemNameAndQuanity>());
    }

    public void Open()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        foreach (ItemNameAndQuanity INQ in InventoryController.current.itemsToSave[storageID])
        {
            ItemSlot slot = Instantiate(prefabSlot, content).GetComponent<ItemSlot>();
            slot.inventory = InventoryController.current;
            if (INQ.name != null) slot.Fill(InventoryController.current.itemByName[INQ.name].item, INQ.quanity);
            else slot.item = null;
        }
        InventoryController.current.activeStorage = this;
    }

    public void Close()
    {
        List<Transform> destroy = new List<Transform>();
        List<ItemNameAndQuanity> itemsToAdd = new List<ItemNameAndQuanity>();
        foreach (Transform slotTransform in content)
        {
            ItemSlot slot = slotTransform.gameObject.GetComponent<ItemSlot>();
            ItemNameAndQuanity toAdd = new ItemNameAndQuanity();
            if (slot.item != null)
            {
                toAdd.name = slot.item.name;
                toAdd.quanity = slot.quanity;
            }
            itemsToAdd.Add(toAdd);
            destroy.Add(slotTransform);
        }
        InventoryController.current.itemsToSave[storageID] = itemsToAdd;
        foreach (Transform slot in destroy)
        {
            Destroy(slot.gameObject);
        }
        InventoryController.current.activeStorage = null;
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
