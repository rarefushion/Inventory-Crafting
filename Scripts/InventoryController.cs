using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public int numberOfSlots;
    public bool saveLoad = true;

    [Header("Object References")]
    public GameObject slotsParent;
    public GameObject slotPrefab;
    public TMP_Text defaultDescriptionOBJ;

    public Action<Item> HoveringItem = (i) => 
    {
        if (i != null)
            current.defaultDescriptionOBJ.text = i.description; 
    };
    public static Action onDisable;
    public static Action onEnable;
    public static InventoryInputs inputs;
    public Dictionary<string, Item> itemByName = new Dictionary<string, Item>();
    public Dictionary<string, List<ItemSlot>> itemSlotsByItemName = new Dictionary<string, List<ItemSlot>>();
    public List<ItemSlot> emptySlots;

    public static InventoryController current;

    private void Awake()
    {
        current = this;
        inputs = new InventoryInputs();
    }

    private void Start()
    {
        UnityEngine.Object[] items = Resources.LoadAll("Items", typeof(ScriptableObject));
        for (int i = 0; i < items.Length; i++)
            itemByName.Add(((Item)items[i]).name, ((Item)items[i]));
        for (int i = 0; i < numberOfSlots; i++)
        {
            ItemSlot IS = Instantiate(slotPrefab, slotsParent.transform).GetComponent<ItemSlot>();
            string previousItem = null;
            IS.Updated += (i, q) =>
            {
                if (previousItem != null && itemSlotsByItemName.ContainsKey(previousItem))
                {
                    itemSlotsByItemName[previousItem].Remove(IS);
                    if (itemSlotsByItemName[previousItem].Count == 0)
                        itemSlotsByItemName.Remove(previousItem);
                    else
                        itemSlotsByItemName[previousItem].Sort();
                }
                emptySlots.Remove(IS);
                previousItem = null;
                if (i != null)
                {
                    if (!itemSlotsByItemName.ContainsKey(i.name))
                        itemSlotsByItemName.Add(i.name, new List<ItemSlot>());
                    itemSlotsByItemName[i.name].Add(IS);
                    itemSlotsByItemName[i.name].Sort();
                    previousItem = i.name;
                }
                else
                    emptySlots.Add(IS);
                emptySlots.Sort();

            };
        }
        if (saveLoad)
            InventorySaveSystem.Load();
    }
    /// <summary>
    ///   Returns true if there are enough items
    /// </summary>
    public bool CouldTake(string itemName, int quantity)
    {
        int total = 0;
        if (itemSlotsByItemName.ContainsKey(itemName))
            foreach (ItemSlot IS in itemSlotsByItemName[itemName])
                total += IS.Quantity;
        if (total >= quantity)
            return true;
        return false;
    }
    /// <summary>
    ///   Checks if there are enough items then removes them. Returns true if the items have been removed
    /// </summary>
    public bool TryTake(string itemName, int quantity)
    {
        if (CouldTake(itemName, quantity))
        {
            int qua = itemSlotsByItemName[itemName].Count;
            for (int i = 0; i < qua; i++)
                if (quantity > 0)
                    quantity -= itemSlotsByItemName[itemName][0].Take(quantity);
            return true;
        }
        return false;
    }
    /// <summary>
    ///   Returns true if there are enough spaces to place
    /// </summary>
    public bool CouldGive(string itemName, int quantity)
    {
        if (itemSlotsByItemName.ContainsKey(itemName))
            foreach (ItemSlot IS in itemSlotsByItemName[itemName])
            {
                if (IS.Item == null)
                    quantity -= itemByName[itemName].maxStack;
                if (IS.Item.name == itemName)
                    quantity -= itemByName[itemName].maxStack - IS.Quantity;
            }
        foreach (ItemSlot IS in emptySlots)
            quantity -= itemByName[itemName].maxStack;

        if (quantity <= 0)
            return true;
        return false;
    }
    /// <summary>
    ///   Checks if there are enough spaces then adds the items. Returns true if the items have been Added
    /// </summary>
    public bool TryGive(string itemName, int quantity, bool toEnd = false)
    {
        if (CouldGive(itemName, quantity))
        {
            if (itemSlotsByItemName.ContainsKey(itemName))
                for (int i = 0; i < itemSlotsByItemName[itemName].Count; i++)
                    if (quantity > 0)
                        quantity = itemSlotsByItemName[itemName][i].Give(quantity);
            if (!toEnd)
            {

                for (int i = 0; i < emptySlots.Count; i++)
                    if (quantity > 0)
                    {
                        int q = Mathf.Min(quantity, itemByName[itemName].maxStack);
                        quantity -= q;
                        emptySlots[i].Fill(itemName, q);
                    }
            }
            else
                for (int i = emptySlots.Count - 1; i >= 0; i--)
                    if (quantity > 0)
                    {
                        int q = Mathf.Min(quantity, itemByName[itemName].maxStack);
                        quantity -= q;
                        emptySlots[i].Fill(itemName, q);
                    }
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        inputs.Enable();
        if (onDisable != null)
            onDisable.Invoke();
    }

    private void OnDisable()
    {
        inputs.Disable();
        if (onEnable != null)
            onEnable.Invoke();
    }

    private void OnDestroy()
    {
        if (saveLoad)
            InventorySaveSystem.Save();
    }
}