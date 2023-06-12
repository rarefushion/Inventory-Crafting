using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : ItemContainer, IPointerClickHandler
{
    [Header("--- ItemSlot ---")]
    public PrecisionSplit precisionSplit;

    private InventoryController IC;

    private void Start()
    {
        if (Item == null)
            Quantity = 0;
        IC = InventoryController.current;
        gameObject.GetComponent<DescriptionHandler>().thisItem = new DescriptionHandler.ItemGetter(() => { return Item; });
        if (precisionSplit != null)
            precisionSplit.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Item CItem = IC.cursorSlot.Item;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if ((CItem == null || Item == null) || CItem != Item)
                IC.cursorSlot.Swap(this);
            else if (CItem != null && (CItem == Item || Item == null))
            {
                Item = CItem;
                Quantity += IC.cursorSlot.Take(Item.maxStack - Quantity);
            }
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (CItem == null && Item != null)
                IC.cursorSlot.Fill(ItemName, Take(Mathf.Max(Quantity / 2, 1)));
            else if (CItem != null && ((CItem == Item && Quantity < Item.maxStack) || Item == null))
                Fill(CItem.name, IC.cursorSlot.Take(1));
        }
        if (eventData.button == PointerEventData.InputButton.Middle && precisionSplit != null)
        {
            //if (CItem == null || CItem == Item)
            //{
            //    precisionSplit.gameObject.SetActive(true);
            //    Action<Item, int> slotU = null;
            //    IC.cursorSlot.Updated += slotU = (i, q) =>
            //    {
            //        precisionSplit.gameObject.SetActive(false);
            //        IC.cursorSlot.Updated -= slotU;
            //    };
            //    precisionSplit.slider.maxValue = Quantity;
            //    Action<int> split = null;
            //    precisionSplit.SplitCalled += split = (i) =>
            //    {
            //        IC.cursorSlot.Fill(ItemName, Take(i));
            //        precisionSplit.SplitCalled -= split;
            //    };
            //}
        }
    }
}
