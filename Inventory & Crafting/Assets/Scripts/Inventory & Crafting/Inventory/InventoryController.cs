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
        //if not clicking reset state
        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            if (Cursor.state == Cursor.CursorState.Transitioning || Cursor.state == Cursor.CursorState.LDrag || Cursor.state == Cursor.CursorState.RDrag || Cursor.state == Cursor.CursorState.QucikTransfering)
            {
                if (Cursor.state == Cursor.CursorState.LDrag) Cursor.LeftClickSplit();

                if (Cursor.itemSlot.item != null) Cursor.state = Cursor.CursorState.Filled;
                else Cursor.state = Cursor.CursorState.Neutral;
                Cursor.draggingSlots.Clear();
            }
        }
        //if (left else right click) raycast from mouse position for slot, check if hotkeys are held else signal Cursor.Click() or Cursor.RightClick(). 
        else if(Input.GetMouseButton(0) && Cursor.state != Cursor.CursorState.Transitioning)
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            canvasRacaster.Raycast(pointerEventData, raycastResults);
            ItemSlot slot = null;
            //if (slot under croshair)
            foreach(RaycastResult result in raycastResults)
            {
                if (result.gameObject.name == "$Slot(Clone)") slot = result.gameObject.GetComponent<ItemSlot>();
            }
            //Check if advanced split is open. For left lick we don't do anything else if it's open otherwise it would close when moving slider.
            if (advancedSplitItemSlot != null || slot == null) return;
            //Check AdvancedSplit Key
            if (Input.GetKey(KeyCode.LeftAlt) && slot.item != null)
            {
                if (slot.item != null && Cursor.itemSlot.item == null)
                {
                    advancedSplitObject.GetChild(0).GetComponent<Slider>().maxValue = slot.quanity;
                    advancedSplitItemSlot = slot;
                    advancedSplitObject.gameObject.SetActive(true);
                    advancedSplitObject.position = slot.transform.position + new Vector3(0, -slot.transform.gameObject.GetComponent<RectTransform>().localScale.y / 2, 1);
                }
            }
            else if (slot != null) Cursor.Click(slot);
        }
        else if(Input.GetMouseButton(1) && Cursor.state != Cursor.CursorState.Transitioning)
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            canvasRacaster.Raycast(pointerEventData, raycastResults);
            ItemSlot slot = null;
            //if (slot under croshair)
            foreach(RaycastResult result in raycastResults)
            {
                if (result.gameObject.name == "$Slot(Clone)") slot = result.gameObject.GetComponent<ItemSlot>();
                else if (result.gameObject.name == "$Advanced Split") return;
            }
            //Check if advanced split is open. Right click is required to close advanced split
            if (advancedSplitItemSlot != null)
            {
                advancedSplitObject.GetChild(0).GetComponent<Slider>().SetValueWithoutNotify(0);
                advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text = "0";
                advancedSplitItemQuanity = (int)advancedSplitObject.GetChild(0).GetComponent<Slider>().value;
                advancedSplitItemSlot = null;
                advancedSplitObject.gameObject.SetActive(false);
            }
            if (slot == null) return;
            //Check AdvancedSplit Key
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
            else if (slot != null) Cursor.RightClick(slot);
        }
    }
    //Slot item must not be null/Clear() Befor Log. Default Cursor functions do this already
    public void LogItem(ItemSlot slot, int quanity, bool newItem)
    {
        itemByName[slot.item.name].quanity += quanity;
        if (newItem) itemByName[slot.item.name].locations.Add(slot);
        else if (slot.quanity - quanity <= 0) itemByName[slot.item.name].locations.Remove(slot);
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
        if (advancedSplitItemQuanity <= 0) goto Finnish;
        string itemName = advancedSplitItemSlot.item.name;
        int quan = advancedSplitItemSlot.Split(advancedSplitItemQuanity);
        Cursor.itemSlot.Fill(itemByName[itemName].item, quan);
        Cursor.state = Cursor.CursorState.Filled;
        Finnish:  
        advancedSplitObject.GetChild(0).GetComponent<Slider>().SetValueWithoutNotify(0);
        advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text = "0";
        advancedSplitItemQuanity = (int)advancedSplitObject.GetChild(0).GetComponent<Slider>().value;
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
            }
        }
    }
}

[System.Serializable]
public static class Cursor
{
    //Both quanityText and image need to be set at start. 
    public static ItemSlot itemSlot = new ItemSlot();
    //Cursor state. Neutral no item, pickup item exists, LDrag and RDrag item exists and currently draging.
    public enum CursorState {Neutral, Transitioning, Filled, LDrag, RDrag, QucikTransfering};
    public static CursorState state = CursorState.Neutral;
    public static List<ItemSlot> draggingSlots = new List<ItemSlot>();
    //Triggered by InventoryController.Update() RayCast.
    public static void Click(ItemSlot hoverSlot)
    {
        //Check modifiers
        if ((state == CursorState.Neutral || state == CursorState.Filled) && Input.GetKey(KeyCode.LeftShift) && itemSlot.inventory.activeStorage != null) state = CursorState.QucikTransfering;
        
        if (draggingSlots.Contains(hoverSlot)) return;
        switch (state)
        {
            case CursorState.Neutral:
                if (hoverSlot.item != null)
                {
                    itemSlot.Fill(hoverSlot.item, hoverSlot.quanity);
                    hoverSlot.Clear();
                    state = CursorState.Transitioning;
                }
                break;
            case CursorState.Filled:
                if (hoverSlot.item == null)
                {
                    draggingSlots.Add(hoverSlot);
                    hoverSlot.transform.GetComponent<Image>().color = new Color(0.588f, 0.588f, 0.588f, 1.000f);
                    state = CursorState.LDrag;
                }
                else if (itemSlot.item.name == hoverSlot.item.name)
                {
                    if(hoverSlot.quanity == hoverSlot.item.maxStack)
                    {    
                        hoverSlot.Swap(itemSlot);
                        state = CursorState.Transitioning;
                    } 
                    else 
                    {
                        draggingSlots.Add(hoverSlot);
                        hoverSlot.transform.GetComponent<Image>().color = new Color(0.588f, 0.588f, 0.588f, 1.000f);
                        state = CursorState.LDrag;
                    }
                }
                else 
                {    
                    hoverSlot.Swap(itemSlot);
                    state = CursorState.Transitioning;
                } 
                break;
            case CursorState.LDrag:
                if (hoverSlot.item == null)
                {
                    if (draggingSlots.Count >= itemSlot.quanity) return;
                    hoverSlot.transform.GetComponent<Image>().color = new Color(0.588f, 0.588f, 0.588f, 1.000f);
                    draggingSlots.Add(hoverSlot);
                    state = CursorState.LDrag;
                }
                else if (itemSlot.item.name == hoverSlot.item.name)
                {
                    if (hoverSlot.quanity == hoverSlot.item.maxStack || draggingSlots.Count >= itemSlot.quanity) return;
                    hoverSlot.transform.GetComponent<Image>().color = new Color(0.588f, 0.588f, 0.588f, 1.000f);
                    draggingSlots.Add(hoverSlot);
                    state = CursorState.LDrag;
                }
                break;
            case CursorState.RDrag:
                break;
            case CursorState.QucikTransfering:
                if (hoverSlot.item != null)
                {
                    ItemSlot firstOpen = null;
                    if (hoverSlot.isInventory)
                    {
                        foreach (Transform slotOBJ in InventoryController.current.activeStorage.content)
                        {
                            ItemSlot storageSlot = slotOBJ.GetComponent<ItemSlot>();
                            if (storageSlot.item != null) 
                            {
                                if (storageSlot.item.name != hoverSlot.item.name) continue;
                                storageSlot.Add(hoverSlot.Split(Mathf.Clamp(hoverSlot.quanity, 0, storageSlot.item.maxStack - storageSlot.quanity)));
                                if (hoverSlot.item == null) break;
                                continue;
                            }
                            if (storageSlot.item == null && firstOpen == null) firstOpen = storageSlot;
                        }
                        if (hoverSlot.item != null && firstOpen != null) firstOpen.Fill(hoverSlot.item, hoverSlot.Split(hoverSlot.quanity));
                    }
                    else
                    {
                        foreach (Transform slotOBJ in InventoryController.current.content)
                        {
                            ItemSlot invSlot = slotOBJ.GetComponent<ItemSlot>();
                            if (invSlot.item != null) 
                            {
                                if (invSlot.item.name != hoverSlot.item.name) continue;
                                invSlot.Add(hoverSlot.Split(Mathf.Clamp(hoverSlot.quanity, 0, invSlot.item.maxStack - invSlot.quanity)));
                                if (hoverSlot.item == null) break;
                                continue;
                            }
                            if (invSlot.item == null && firstOpen == null) firstOpen = invSlot;
                        }
                        if (hoverSlot.item != null && firstOpen != null) firstOpen.Fill(hoverSlot.item, hoverSlot.Split(hoverSlot.quanity));
                    }
                }
                break;
            default:
                Debug.Log("Error: Cursor.State exception. Current State: " + state);
                break;
        }
    }
    //Triggered by InventoryController.Update() RayCast.
    public static void RightClick(ItemSlot hoverSlot)
    {
        if (draggingSlots.Contains(hoverSlot)) return;
        switch (state)
        {
            case CursorState.Neutral:
                if (hoverSlot.item != null)
                {
                    string itemName = hoverSlot.item.name;
                    int quan = hoverSlot.Split(Mathf.Clamp((hoverSlot.quanity + 1) / 2, 0, hoverSlot.item.maxStack));
                    itemSlot.Fill(InventoryController.current.itemByName[itemName].item, quan);
                    state = CursorState.Transitioning;
                }
                break;
            case CursorState.Filled:
                if (hoverSlot.item == null)
                {
                    string itemName = itemSlot.item.name;
                    int quan = itemSlot.Split(1);
                    hoverSlot.Fill(InventoryController.current.itemByName[itemName].item, quan);
                    draggingSlots.Add(hoverSlot);
                    if (itemSlot.item != null) state = CursorState.RDrag;
                    else state = CursorState.Transitioning;
                }
                else if (itemSlot.item.name == hoverSlot.item.name) 
                {
                    int quan = itemSlot.Split(Mathf.Clamp(1, 0, hoverSlot.item.maxStack - hoverSlot.quanity));
                    hoverSlot.Add(quan);
                    draggingSlots.Add(hoverSlot);
                    if (itemSlot.item != null) state = CursorState.RDrag;
                    else state = CursorState.Transitioning;
                }
                break;
            case CursorState.LDrag:
                break;
            case CursorState.RDrag:
                if (hoverSlot.item == null)
                {
                    string itemName = itemSlot.item.name;
                    int quan = itemSlot.Split(1);
                    hoverSlot.Fill(InventoryController.current.itemByName[itemName].item, quan);
                    draggingSlots.Add(hoverSlot);
                    if (itemSlot.item != null) state = CursorState.RDrag;
                    else state = CursorState.Transitioning;
                }
                else if (itemSlot.item.name == hoverSlot.item.name) 
                {
                    int quan = itemSlot.Split(Mathf.Clamp(1, 0, hoverSlot.item.maxStack - hoverSlot.quanity));
                    hoverSlot.Add(quan);
                    draggingSlots.Add(hoverSlot);
                    if (itemSlot.item != null) state = CursorState.RDrag;
                    else state = CursorState.Transitioning;
                }
                break;
            default:
                Debug.Log("Error: Cursor.State exception. Current State: " + state);
                break;
        }
    }

    public static void LeftClickSplit()
    {
        int baseQuan = itemSlot.quanity;
        foreach (ItemSlot dragged in draggingSlots)
        {
            if (dragged.item == null) dragged.Fill(itemSlot.item, itemSlot.Split(Mathf.Clamp(baseQuan / draggingSlots.Count, 1, itemSlot.item.maxStack)));
            else dragged.Add(itemSlot.Split(Mathf.Clamp(baseQuan / draggingSlots.Count, 1, dragged.item.maxStack - dragged.quanity)));
            dragged.transform.GetComponent<Image>().color = new Color(0.392f, 0.392f, 0.392f, 1.000f);
        }
    }
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
    //When looping thew all locations either make sure you aren't changing this list by spliting items or by making a new list of this one and looping threw the new one. Spliting items will call log item and if there are no items left it will remove it from this list.
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
