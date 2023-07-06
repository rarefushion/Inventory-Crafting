using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public static class InventorySaveSystem
{
    //Make sure all Storage is Close() and InventoryController UpdateItemsToSave()
    public static void Save()
    {
        List<List<ItemNameAndQuantity>> itemsToSave = new List<List<ItemNameAndQuantity>>();
        List<ItemNameAndQuantity> inventoryList = new List<ItemNameAndQuantity>();
        foreach (Transform slotOBJ in InventoryController.current.slotsParent.transform)
        {
            ItemSlot slot = slotOBJ.GetComponent<ItemSlot>();
            if (slot.Item == null) inventoryList.Add(new ItemNameAndQuantity(null, 0));
            else inventoryList.Add(new ItemNameAndQuantity(slot.ItemName, slot.Quantity));
        }
        itemsToSave.Add(inventoryList);

        XmlSerializer serializer = new XmlSerializer(typeof(List<List<ItemNameAndQuantity>>));
        using (StringWriter sw = new StringWriter())
        {
            serializer.Serialize(sw, itemsToSave);
            PlayerPrefs.SetString("Items", sw.ToString());
        }
    }

    public static void Load()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<List<ItemNameAndQuantity>>));
        string wholeFile = PlayerPrefs.GetString("Items");
        if (wholeFile.Length > 0)
            using (var reader = new StringReader(wholeFile))
            {
                InventoryController IC = InventoryController.current;
                List<List<ItemNameAndQuantity>> items = serializer.Deserialize(reader) as List<List<ItemNameAndQuantity>>;
                if (items[0].Count != IC.numberOfSlots)
                    Debug.Log("numberOfSlots in InventoryController changed this will not be corrected for");

                for (int i = 0; i < IC.numberOfSlots; i++)
                    if (items[0].Count > i)
                    {
                        ItemSlot slot = IC.slotsParent.transform.GetChild(i).GetComponent<ItemSlot>();
                        ItemNameAndQuantity IQ = items[0][i];
                        slot.Fill(IQ.name, IQ.quantity);
                    }
            }
    }

    [System.Serializable]
    public struct ItemNameAndQuantity
    {
        public string name;
        public int quantity;

        public ItemNameAndQuantity(string name, int quantity)
        {
            this.name = name;
            this.quantity = quantity;
        }
    }
}
