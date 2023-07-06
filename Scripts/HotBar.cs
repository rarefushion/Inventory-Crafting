using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotBar : MonoBehaviour
{
    public int numberOfReferencedSlots;

    private int currentReferencedSlot;
    public int CurrentSlotID
    {
        get { return currentReferencedSlot; }
        set
        {
            if (value < 0)
                value = numberOfReferencedSlots - 1;
            else if (value >= numberOfReferencedSlots)
                value = 0;
            currentReferencedSlot = value;
            if (currentSlotChanged != null)
                currentSlotChanged.Invoke(value);
        }
    }
    public Action<int> currentSlotChanged;

    public Transform referencesParent;
    public Transform defaultSelectorImage;
    public static HotBar current;
    private InventoryController IC;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        IC = InventoryController.current;
        for (int i = 0; i < numberOfReferencedSlots; i++)
            if (referencesParent.childCount - i <= 0)
                Instantiate(IC.slotPrefab, referencesParent).GetComponent<ItemSlot>();
        for (int i = 0; i < numberOfReferencedSlots; i++)
        {
            ItemSlot thisSlot = referencesParent.GetChild((referencesParent.childCount - 1) - i).GetComponent<ItemSlot>();
            ItemSlot INVSlot = IC.slotsParent.transform.GetChild((IC.slotsParent.transform.childCount - 1) - i).GetComponent<ItemSlot>();
            thisSlot.visualOnly = true;
            thisSlot.Fill(INVSlot.ItemName, INVSlot.Quantity);
            INVSlot.Updated += (item, quan) =>
            {
                thisSlot.Disable();
                if (item != null)
                    thisSlot.Fill(item.name, quan);
            };
        }
        //Input
        InventoryController.inputs.Navigation.HotBarSlotUp.performed += (info) => { CurrentSlotID++; };
        InventoryController.inputs.Navigation.HotBarSlotDown.performed += (info) => { CurrentSlotID--; };
        if (defaultSelectorImage != null)
            currentSlotChanged += DefaultSlotIndicator;
    }

    public void DefaultSlotIndicator(int value)
    {
        defaultSelectorImage.position = referencesParent.transform.GetChild(CurrentSlotID).transform.position + Vector3.up;
    }

    public ItemSlot SelectedSlot()
    {
        return IC.slotsParent.transform.GetChild((IC.slotsParent.transform.childCount - 1) - currentReferencedSlot).GetComponent<ItemSlot>();
    }
}
