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
    public int hotbarReferences;
    private List<ItemSlot> hotbarSlots = new List<ItemSlot>();
    private List<ItemSlot> referencedSlots = new List<ItemSlot>();
    private int hotbarSelectedSlot = 1;
    public ItemSlot HotbarSelectedSlot{get {return referencedSlots[-hotbarSelectedSlot + hotbarReferences];}}
    private Transform hotbarSelectedST;
    public Transform hotbar;
    public GameObject prefabSlot;
    public Transform content;

    public List<Item> items = new List<Item>();
    //The Dictionary contains 1 of every item, the slot locations of all the items and the total quantity of every item. If you remove or add items from the inventory remember to log it with LogItem(). Default ItemSlot functions do this already.
    public Dictionary<string, ItemQuantitySlots> itemByName = new Dictionary<string, ItemQuantitySlots>();
    public List<List<ItemNameAndQuantity>> itemsToSave = new List<List<ItemNameAndQuantity>>();
    public Storage activeStorage;

    public Transform cursorItem;
    public GraphicRaycaster canvasRaycaster;
    PointerEventData pointerEventData;
    public EventSystem eventSystem;

    public Transform advancedSplitObject;
    public ItemSlot advancedSplitItemSlot;
    [HideInInspector] public int advancedSplitItemQuantity;

    public static InventoryController current;


    public void Start()
    {
        current = this;

        InventoryCursor.itemSlot.image = cursorItem.GetChild(0).gameObject.GetComponent<Image>();
        InventoryCursor.itemSlot.quantityText = cursorItem.GetChild(1).gameObject.GetComponent<TMP_Text>();
        InventoryCursor.itemSlot.inventory = this;
        InventoryCursor.itemSlot.isInventory = true;
        //Generate itemByName Dictionary
        foreach(Item i in items)
        {
            itemByName.Add(i.name, new ItemQuantitySlots(i));
        }

        SaveSystem.Load();

        hotbarSelectedSlot = 1;
        hotbarSelectedST = hotbar.GetChild(0);
        for (int i = 0; i < hotbarReferences; i++) 
        {
            GameObject slot = Instantiate(prefabSlot, hotbar);
            slot.GetComponent<Image>().raycastTarget = false;
            hotbarSlots.Add(slot.GetComponent<ItemSlot>());
            referencedSlots.Add(content.GetChild(numberOfSlots - 1  - i).gameObject.GetComponent<ItemSlot>());
        }
        hotbarSelectedST.SetSiblingIndex(hotbarReferences);
    }

    public void Update()
    {
        //move image for item held by cursor
        cursorItem.position = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward);
        //if not clicking reset state
        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            if (InventoryCursor.state == InventoryCursor.CursorState.Transitioning || InventoryCursor.state == InventoryCursor.CursorState.LDrag || InventoryCursor.state == InventoryCursor.CursorState.RDrag || InventoryCursor.state == InventoryCursor.CursorState.QuickTransferring)
            {
                if (InventoryCursor.state == InventoryCursor.CursorState.LDrag) InventoryCursor.LeftClickSplit();

                if (InventoryCursor.itemSlot.item != null) InventoryCursor.state = InventoryCursor.CursorState.Filled;
                else InventoryCursor.state = InventoryCursor.CursorState.Neutral;
                InventoryCursor.draggingSlots.Clear();
            }
        }
        //if (left else right click) raycast from mouse position for slot, check if hotkeys are held else signal Cursor.Click() or Cursor.RightClick(). 
        else if(Input.GetMouseButton(0) && InventoryCursor.state != InventoryCursor.CursorState.Transitioning)
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            canvasRaycaster.Raycast(pointerEventData, raycastResults);
            ItemSlot slot = null;
            //if (slot under crosshair)
            foreach(RaycastResult result in raycastResults)
            {
                if (result.gameObject.name == "$Slot(Clone)") slot = result.gameObject.GetComponent<ItemSlot>();
            }
            //Check if advanced split is open. For left lick we don't do anything else if it's open otherwise it would close when moving slider.
            if (advancedSplitItemSlot != null || slot == null) return;
            //Check AdvancedSplit Key
            if (Input.GetKey(KeyCode.LeftAlt) && slot.item != null)
            {
                if (slot.item != null && InventoryCursor.itemSlot.item == null)
                {
                    advancedSplitObject.GetChild(0).GetComponent<Slider>().maxValue = slot.quantity;
                    advancedSplitItemSlot = slot;
                    advancedSplitObject.gameObject.SetActive(true);
                    advancedSplitObject.position = slot.transform.position + new Vector3(0, -slot.transform.gameObject.GetComponent<RectTransform>().localScale.y / 2, 1);
                }
            }
            else if (slot != null) InventoryCursor.Click(slot);
        }
        else if(Input.GetMouseButton(1) && InventoryCursor.state != InventoryCursor.CursorState.Transitioning)
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            canvasRaycaster.Raycast(pointerEventData, raycastResults);
            ItemSlot slot = null;
            //if (slot under crosshair)
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
                advancedSplitItemQuantity = (int)advancedSplitObject.GetChild(0).GetComponent<Slider>().value;
                advancedSplitItemSlot = null;
                advancedSplitObject.gameObject.SetActive(false);
            }
            if (slot == null) return;
            //Check AdvancedSplit Key
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (slot.item != null && InventoryCursor.itemSlot.item == null)
                {
                    advancedSplitObject.GetChild(0).GetComponent<Slider>().maxValue = slot.quantity;
                    advancedSplitItemSlot = slot;
                    advancedSplitObject.gameObject.SetActive(true);
                    advancedSplitObject.position = slot.transform.position + new Vector3(0, -slot.transform.gameObject.GetComponent<RectTransform>().localScale.y / 2, 1);
                }
            }
            else if (slot != null) InventoryCursor.RightClick(slot);
        }
        //Update hotbar
        for (int i = hotbarReferences - 1; i >= 0; i--)
        {
            ItemSlot reference = referencedSlots[i], HB = hotbarSlots[-i + hotbarReferences - 1];
            if (reference.item != HB.item || reference.quantity != HB.quantity)
            {
                HB.Clear();
                if (reference.item != null) HB.Fill(reference.item, reference.quantity);
            }  
        }
        //get mouseWheel
        if (Input.mouseScrollDelta.y != 0)
        {
            hotbarSelectedSlot += (int)Input.mouseScrollDelta.y;
            if (hotbarSelectedSlot > hotbarReferences) hotbarSelectedSlot = 1;
            if (hotbarSelectedSlot <= 0) hotbarSelectedSlot = hotbarReferences;
            hotbarSelectedST.position = hotbarSlots[hotbarSelectedSlot - 1].transform.position;    
        }
        //update ui
    }
    //Slot item must not be null/Clear() Before Log. Default Cursor functions do this already
    public void LogItem(ItemSlot slot, int quantity, bool newItem)
    {
        itemByName[slot.item.name].quantity += quantity;
        if (newItem) itemByName[slot.item.name].locations.Add(slot);
        else if (slot.quantity - quantity <= 0) itemByName[slot.item.name].locations.Remove(slot);
    }

    public void AdvancedItemSplitQuantityUpdate(bool slider)
    {
        if (slider) advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text = advancedSplitObject.GetChild(0).GetComponent<Slider>().value.ToString();
        else 
        {
            float i = Mathf.Clamp(float.Parse(advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text), 0, advancedSplitItemSlot.quantity);
            advancedSplitObject.GetChild(0).GetComponent<Slider>().SetValueWithoutNotify(i);
            advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text = i.ToString();
        }
        advancedSplitItemQuantity = (int)advancedSplitObject.GetChild(0).GetComponent<Slider>().value;
    }

    public void AdvancedItemSplit()
    {
        if (advancedSplitItemQuantity <= 0) goto Finnish;
        string itemName = advancedSplitItemSlot.item.name;
        int quan = advancedSplitItemSlot.Split(advancedSplitItemQuantity);
        InventoryCursor.itemSlot.Fill(itemByName[itemName].item, quan);
        InventoryCursor.state = InventoryCursor.CursorState.Filled;
        Finnish:  
        advancedSplitObject.GetChild(0).GetComponent<Slider>().SetValueWithoutNotify(0);
        advancedSplitObject.GetChild(1).GetComponent<TMP_InputField>().text = "0";
        advancedSplitItemQuantity = (int)advancedSplitObject.GetChild(0).GetComponent<Slider>().value;
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
        itemsToSave.Add(new List<ItemNameAndQuantity>());
    }

    public void UpdateItemsToSave()
    {
        List<ItemNameAndQuantity> inventoryList = itemsToSave[0];
        inventoryList.Clear();
        foreach (Transform slotOBJ in content)
        {
            ItemSlot slot = slotOBJ.GetComponent<ItemSlot>();
            if (slot.item == null) inventoryList.Add(new ItemNameAndQuantity(null, 0));
            else inventoryList.Add(new ItemNameAndQuantity(slot.item.name, slot.quantity));
        }
    }

    public void LoadSlotsFromItemsToSave()
    {
        List<ItemNameAndQuantity> inventoryList = itemsToSave[0];
        if (inventoryList.Count != numberOfSlots)
        {
            Debug.Log("numberOfSlots in InventoryController changed this will not be corrected for");
        }

        foreach (ItemNameAndQuantity i in inventoryList)
        {
            ItemSlot slot = Instantiate(prefabSlot, content).GetComponent<ItemSlot>();
            slot.inventory = this;
            slot.isInventory = true;
            slot.item = null;
            if (i.name != null)
            {
                slot.Fill(itemByName[i.name].item, i.quantity);
            }
        }
    }
}

[System.Serializable]
public static class InventoryCursor
{
    //Both quantityText and image need to be set at start. 
    public static ItemSlot itemSlot = new ItemSlot();
    //Cursor state. Neutral no item, pickup item exists, LDrag and RDrag item exists and currently dragging.
    public enum CursorState {Neutral, Transitioning, Filled, LDrag, RDrag, QuickTransferring};
    public static CursorState state = CursorState.Neutral;
    public static List<ItemSlot> draggingSlots = new List<ItemSlot>();
    //Triggered by InventoryController.Update() RayCast.
    public static void Click(ItemSlot hoverSlot)
    {
        //Check modifiers
        if ((state == CursorState.Neutral || state == CursorState.Filled) && Input.GetKey(KeyCode.LeftShift) && itemSlot.inventory.activeStorage != null) state = CursorState.QuickTransferring;
        
        if (draggingSlots.Contains(hoverSlot)) return;
        switch (state)
        {
            case CursorState.Neutral:
                if (hoverSlot.item != null)
                {
                    itemSlot.Fill(hoverSlot.item, hoverSlot.quantity);
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
                    if(hoverSlot.quantity == hoverSlot.item.maxStack)
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
                    if (draggingSlots.Count >= itemSlot.quantity) return;
                    hoverSlot.transform.GetComponent<Image>().color = new Color(0.588f, 0.588f, 0.588f, 1.000f);
                    draggingSlots.Add(hoverSlot);
                    state = CursorState.LDrag;
                }
                else if (itemSlot.item.name == hoverSlot.item.name)
                {
                    if (hoverSlot.quantity == hoverSlot.item.maxStack || draggingSlots.Count >= itemSlot.quantity) return;
                    hoverSlot.transform.GetComponent<Image>().color = new Color(0.588f, 0.588f, 0.588f, 1.000f);
                    draggingSlots.Add(hoverSlot);
                    state = CursorState.LDrag;
                }
                break;
            case CursorState.RDrag:
                break;
            case CursorState.QuickTransferring:
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
                                storageSlot.Add(hoverSlot.Split(Mathf.Clamp(hoverSlot.quantity, 0, storageSlot.item.maxStack - storageSlot.quantity)));
                                if (hoverSlot.item == null) break;
                                continue;
                            }
                            if (storageSlot.item == null && firstOpen == null) firstOpen = storageSlot;
                        }
                        if (hoverSlot.item != null && firstOpen != null) firstOpen.Fill(hoverSlot.item, hoverSlot.Split(hoverSlot.quantity));
                    }
                    else
                    {
                        foreach (Transform slotOBJ in InventoryController.current.content)
                        {
                            ItemSlot invSlot = slotOBJ.GetComponent<ItemSlot>();
                            if (invSlot.item != null) 
                            {
                                if (invSlot.item.name != hoverSlot.item.name) continue;
                                invSlot.Add(hoverSlot.Split(Mathf.Clamp(hoverSlot.quantity, 0, invSlot.item.maxStack - invSlot.quantity)));
                                if (hoverSlot.item == null) break;
                                continue;
                            }
                            if (invSlot.item == null && firstOpen == null) firstOpen = invSlot;
                        }
                        if (hoverSlot.item != null && firstOpen != null) firstOpen.Fill(hoverSlot.item, hoverSlot.Split(hoverSlot.quantity));
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
                    int quan = hoverSlot.Split(Mathf.Clamp((hoverSlot.quantity + 1) / 2, 0, hoverSlot.item.maxStack));
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
                    int quan = itemSlot.Split(Mathf.Clamp(1, 0, hoverSlot.item.maxStack - hoverSlot.quantity));
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
                    int quan = itemSlot.Split(Mathf.Clamp(1, 0, hoverSlot.item.maxStack - hoverSlot.quantity));
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
        int baseQuan = itemSlot.quantity;
        foreach (ItemSlot dragged in draggingSlots)
        {
            if (dragged.item == null) dragged.Fill(itemSlot.item, itemSlot.Split(Mathf.Clamp(baseQuan / draggingSlots.Count, 1, itemSlot.item.maxStack)));
            else dragged.Add(itemSlot.Split(Mathf.Clamp(baseQuan / draggingSlots.Count, 1, dragged.item.maxStack - dragged.quantity)));
            dragged.transform.GetComponent<Image>().color = new Color(0.392f, 0.392f, 0.392f, 1.000f);
        }
    }
}

public static class SaveSystem
{
    //Make sure all Storage is Close() and InventoryController UpdateItemsToSave()
    public static void Save()
    {
        List<List<ItemNameAndQuantity>> itemsToSave = InventoryController.current.itemsToSave;
        XmlSerializer serializer = new XmlSerializer (typeof(List<List<ItemNameAndQuantity>>));
        using (StringWriter sw = new StringWriter())
        {
            serializer.Serialize(sw, itemsToSave);
            PlayerPrefs.SetString("Items", sw.ToString());
        }
    }

    public static void Load()
    { 
        XmlSerializer serializer = new XmlSerializer (typeof(List<List<ItemNameAndQuantity>>));
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
                InventoryController.current.itemsToSave = serializer.Deserialize(reader) as List<List<ItemNameAndQuantity>>;
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
//Used in itemByName to keep track of item amount and slot locations
[System.Serializable]
public class ItemQuantitySlots
{
    public Item item;
    public int quantity;
    //When looping thew all locations either make sure you aren't changing this list by splitting items or by making a new list of this one and looping threw the new one. Splitting items will call log item and if there are no items left it will remove it from this list.
    public List<ItemSlot> locations = new List<ItemSlot>();

    public ItemQuantitySlots()
    {

    }

    public ItemQuantitySlots(Item I)
    {
        item = I;
        quantity = 0;
    }

    public ItemQuantitySlots(Item I, int Q)
    {
        item = I;
        quantity = Q;
    }
}

//Minimize the data needed to be stored and saved
[System.Serializable]
public class ItemNameAndQuantity
{
    public string name;
    public int quantity;

    public ItemNameAndQuantity ()
    {

    }

    public ItemNameAndQuantity (string N, int Q)
    {
        name = N;
        quantity = Q;
    }
}
