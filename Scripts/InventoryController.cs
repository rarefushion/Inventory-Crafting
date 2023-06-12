using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public int numberOfSlots;

    [Header("Object References")]
    public GameObject slotsParent;
    public GameObject slotPrefab;
    public TMP_Text defaultDescriptionOBJ;
    public Canvas canvas;
    public ItemContainer cursorSlot;
    public delegate Vector3 CursorSlotPosition();
    public CursorSlotPosition cursorSlotPosition = new CursorSlotPosition(DefaultCursorSlotPosition);
    public Action<Item> HoveringItem = (i) => 
    {
        if (i != null)
            current.defaultDescriptionOBJ.text = i.description; 
    };
    public Dictionary<string, Item> itemByName = new Dictionary<string, Item>();
    public static InventoryController current;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        UnityEngine.Object[] items = Resources.LoadAll("Items", typeof(ScriptableObject));
        for (int i = 0; i < items.Length; i++)
            itemByName.Add(((Item)items[i]).name, ((Item)items[i]));
        for (int i = 0; i < numberOfSlots; i++)
        {
            ItemSlot IS = Instantiate(slotPrefab, slotsParent.transform).GetComponent<ItemSlot>();
            IS.Item = itemByName[itemByName.Keys.ToArray()[UnityEngine.Random.Range(0, items.Length)]];
            IS.Quantity = UnityEngine.Random.Range(0, 10);
        }
    }

    private void Update()
    {
        if (cursorSlotPosition != null)
            cursorSlot.transform.position = cursorSlotPosition();
    }

    static Vector3 DefaultCursorSlotPosition()
    {
        Vector3 v3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        v3.z = current.canvas.transform.position.z;
        return v3;
    }
}

public struct NameAndQaunity
{
    public string name;
    public int quanity;
}