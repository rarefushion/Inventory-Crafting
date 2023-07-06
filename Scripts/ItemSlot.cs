using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ItemSlot : ItemContainer, IPointerClickHandler, IComparable<ItemSlot>
{
    [Header("--- ItemSlot ---")]
    public PrecisionSplit precisionSplit;
    public bool visualOnly;

    private InventoryController IC;
    private CursorSlot CS;

    private void Start()
    {
        if (Item == null)
            Quantity = 0;
        IC = InventoryController.current;
        CS = CursorSlot.current;
        gameObject.GetComponent<DescriptionHandler>().thisItem = new DescriptionHandler.ItemGetter(() => { return Item; });
        if (precisionSplit != null)
        {
            precisionSplit.gameObject.SetActive(false);
            precisionSplit.SplitCalled += (i) => CS.Fill(ItemName, Take(i));
            Action<InputAction.CallbackContext> disablePS = (info) =>
            {
                if (precisionSplit.gameObject.activeSelf && !precisionSplit.mouseInside)
                    precisionSplit.gameObject.SetActive(false);
            };
            InventoryController.inputs.Navigation.LeftClick.performed += disablePS;
            InventoryController.inputs.Navigation.RightClick.performed += disablePS;
            InventoryController.inputs.Navigation.MiddleClick.performed += disablePS;
            InventoryController.onDisable += () => disablePS(new InputAction.CallbackContext());
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (visualOnly || (precisionSplit.mouseInside && precisionSplit.gameObject.activeSelf))
            return;

        Item CItem = CS.Item;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if ((CItem == null || Item == null) || CItem != Item)
                CS.Swap(this);
            else if (CItem != null && (CItem == Item || Item == null))
            {
                Item = CItem;
                Quantity += CS.Take(Item.maxStack - Quantity);
            }
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (CItem == null && Item != null)
                CS.Fill(ItemName, Take(Mathf.Max(Quantity / 2, 1)));
            else if (CItem != null && ((CItem == Item && Quantity < Item.maxStack) || Item == null))
                Fill(CItem.name, CS.Take(1));
        }
        if (eventData.button == PointerEventData.InputButton.Middle && precisionSplit != null)
        {
            if (CItem == null || CItem == Item)
            {
                precisionSplit.gameObject.SetActive(true);
                Action<Item, int> slotU = null;
                CS.Updated += slotU = (i, q) =>
                {
                    precisionSplit.gameObject.SetActive(false);
                    CS.Updated -= slotU;
                };
                precisionSplit.slider.maxValue = Quantity;
                precisionSplit.slider.value = 1;
            }
        }
    }

    public int CompareTo(ItemSlot compareTo)
    {
        if (compareTo == null)
            return 1;
        return transform.GetSiblingIndex().CompareTo(compareTo.transform.GetSiblingIndex());
    }

}
