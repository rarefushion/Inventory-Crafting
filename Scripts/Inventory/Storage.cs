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
    
    public GraphicRaycaster canvasRaycaster;
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
        InventoryController.current.itemsToSave.Add(new List<ItemNameAndQuantity>());
    }

    public void Open()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        foreach (ItemNameAndQuantity INQ in InventoryController.current.itemsToSave[storageID])
        {
            ItemSlot slot = Instantiate(prefabSlot, content).GetComponent<ItemSlot>();
            slot.inventory = InventoryController.current;
            if (INQ.name != null) slot.Fill(InventoryController.current.itemByName[INQ.name].item, INQ.quantity);
            else slot.item = null;
        }
        InventoryController.current.activeStorage = this;
    }

    public void Close()
    {
        List<Transform> destroy = new List<Transform>();
        List<ItemNameAndQuantity> itemsToAdd = new List<ItemNameAndQuantity>();
        foreach (Transform slotTransform in content)
        {
            ItemSlot slot = slotTransform.gameObject.GetComponent<ItemSlot>();
            ItemNameAndQuantity toAdd = new ItemNameAndQuantity();
            if (slot.item != null)
            {
                toAdd.name = slot.item.name;
                toAdd.quantity = slot.quantity;
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
