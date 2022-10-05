using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryController : MonoBehaviour
{
    public int numberOfSlots;
    public GameObject prefabSlot;
    public Transform content;

    public List<Item> items = new List<Item>();
    //The Dictionary contains 1 of every item, the slot locations of all the items and the total quanity of every item. If you remove or add items from the inventory remember to log it with LogItem(). Default ItemSlot functions do this already.
    public Dictionary<string, ItemQuanitySlots> itemByName = new Dictionary<string, ItemQuanitySlots>();
    public List<List<ItemNameAndQuanity>> itemsToSave = new List<List<ItemNameAndQuanity>>();
    public Storage activeStorage;

    public Transform cursorItem;
    public GraphicRaycaster canvasRacaster;
    PointerEventData pointerEventData;
    public EventSystem eventSystem;

    public Transform advancedSplitObject;
    public ItemSlot advancedSplitItemSlot;
    [HideInInspector] public int advancedSplitItemQuanity;

    public static InventoryController current;


    public void Start()
    {
        current = this;

        Cursor.itemSlot.image = cursorItem.GetChild(0).gameObject.GetComponent<Image>();
        Cursor.itemSlot.quanityText = cursorItem.GetChild(1).gameObject.GetComponent<TMP_Text>();
        Cursor.itemSlot.inventory = this;
        Cursor.itemSlot.isInventory = true;
        //Generate itemByName Dictionary
        foreach(Item i in items)
        {
            itemByName.Add(i.name, new ItemQuanitySlots(i));
        }

        SaveSystem.Load();
    }

    public void Update()
    {
        //move image for item held by cursor
        cursorItem.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward;
        //if (right click) raycast from mouse position
        if(Input.GetMouseButtonDown(1))
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            canvasRacaster.Raycast(pointerEventData, raycastResults);
            //if (slot under croshair)
            foreach(RaycastResult result in raycastResults)
            {
                if (result.gameObject.name == "Slot(Clone)")
                {
                    ItemSlot slot = result.gameObject.GetComponent<ItemSlot>();
                    if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        if (slot.item != null && Cursor.itemSlot.item == null)
                        {
                            advancedSplitObject.GetChild(0).GetComponent<Slider>().maxValue = slot.quanity;
                            advancedSplitItemSlot = slot;
                            advancedSplitObject.gameObject.SetActive(true);
                            advancedSplitObject.position = slot.transform.position + new Vector3(0, -slot.transform.gameObject.GetComponent<RectTransform>().localScale.y / 2, 1);
                        }
                    }
                    else slot.RightClick();
                    break;
                }
            }
        }
    }
    //Slot item must not be null. Default ItemSlot functions do this already
    public void LogItem(ItemSlot slot, int quanity, bool newItem)
    {
        itemByName[slot.item.name].quanity += quanity;
        if (newItem) itemByName[slot.item.name].locations.Add(slot);
        else if (quanity < 0) itemByName[slot.item.name].locations.Remove(slot);
    }

    public void AdvancedItemSplitQuanityUpdate(bool slider)
    {
        if (slider) advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text = advancedSplitObject.GetChild(0).GetComponent<Slider>().value.ToString();
        else 
        {
            float i = Mathf.Clamp(float.Parse(advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text), 0, advancedSplitItemSlot.quanity);
            advancedSplitObject.GetChild(0).GetComponent<Slider>().SetValueWithoutNotify(i);
            advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text = i.ToString();
        }
        advancedSplitItemQuanity = (int)advancedSplitObject.GetChild(0).GetComponent<Slider>().value;
    }

    public void AdvancedItemSplit()
    {
        string itemName = advancedSplitItemSlot.item.name;
        int quan = advancedSplitItemSlot.Split(advancedSplitItemQuanity);
        Cursor.itemSlot.Fill(itemByName[itemName].item, quan);
        advancedSplitItemSlot = null;
        advancedSplitObject.gameObject.SetActive(false);
    }

    public void InitializeSlots()
    {
        for(int i = numberOfSlots; i > 0; i--)
        {
            ItemSlot slot = Instantiate(prefabSlot, content).GetComponent<ItemSlot>();
            slot.inventory = this;
            slot.isInventory = true;
            slot.item = null;
        }
        itemsToSave.Add(new List<ItemNameAndQuanity>());
    }

    public void UpdateItemsToSave()
    {
        List<ItemNameAndQuanity> inventoryList = itemsToSave[0];
        inventoryList.Clear();
        foreach (Transform slotOBJ in content)
        {
            ItemSlot slot = slotOBJ.GetComponent<ItemSlot>();
            if (slot.item == null) inventoryList.Add(new ItemNameAndQuanity(null, 0));
            else inventoryList.Add(new ItemNameAndQuanity(slot.item.name, slot.quanity));
        }
    }

    public void LoadSlotsFromItemsToSave()
    {
        List<ItemNameAndQuanity> inventoryList = itemsToSave[0];
        if (inventoryList.Count != numberOfSlots)
        {
            Debug.Log("numberOfSlots in InventoryController changed this will not be corrected for");
        }

        foreach (ItemNameAndQuanity i in inventoryList)
        {
            ItemSlot slot = Instantiate(prefabSlot, content).GetComponent<ItemSlot>();
            slot.inventory = this;
            slot.isInventory = true;
            slot.item = null;
            if (i.name != null)
            {
                slot.Fill(itemByName[i.name].item, i.quanity);
                itemByName[i.name].quanity += i.quanity;
                itemByName[i.name].locations.Add(slot);
            }
        }
    }
}
//Cursor is like a item slot for the cursor
[System.Serializable]
public static class Cursor
{
    //Both quanityText and image need to be set at start.
    public static ItemSlot itemSlot = new ItemSlot();
}

public static class SaveSystem
{
    //Make sure all Storage is Close() and InventoryController UpdateItemsToSave()
    public static void Save()
    {
        List<List<ItemNameAndQuanity>> itemsToSave = InventoryController.current.itemsToSave;
        XmlSerializer serializer = new XmlSerializer (typeof(List<List<ItemNameAndQuanity>>));
        using (StringWriter sw = new StringWriter())
        {
            serializer.Serialize(sw, itemsToSave);
            PlayerPrefs.SetString("Items", sw.ToString());
        }
    }

    public static void Load()
    { 
        XmlSerializer serializer = new XmlSerializer (typeof(List<List<ItemNameAndQuanity>>));
        string wholeFile = PlayerPrefs.GetString("Items");
        if (wholeFile.Length == 0)
        {
            InventoryController.current.InitializeSlots();
            Debug.Log("Initialized Inventory");
        }
        else
        {
            using (var reader = new StringReader(wholeFile))
            {
                InventoryController.current.itemsToSave = serializer.Deserialize(reader) as List<List<ItemNameAndQuanity>>;
                InventoryController.current.LoadSlotsFromItemsToSave();
            }
        }
    }
}

[System.Serializable]
public class Item
{
    public string name;
    public Sprite image;
    public int maxStack;
}
//Used in itemByName to keep track of item amout and slot locations
[System.Serializable]
public class ItemQuanitySlots
{
    public Item item;
    public int quanity;
    public List<ItemSlot> locations = new List<ItemSlot>();

    public ItemQuanitySlots()
    {

    }

    public ItemQuanitySlots(Item I)
    {
        item = I;
        quanity = 0;
    }

    public ItemQuanitySlots(Item I, int Q)
    {
        item = I;
        quanity = Q;
    }
}

//Minimize the data needed to be stored and saved
[System.Serializable]
public class ItemNameAndQuanity
{
    public string name;
    public int quanity;

    public ItemNameAndQuanity ()
    {

    }

    public ItemNameAndQuanity (string N, int Q)
    {
        name = N;
        quanity = Q;
    }
}
