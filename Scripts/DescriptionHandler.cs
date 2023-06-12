using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DescriptionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate Item ItemGetter();
    public ItemGetter thisItem;
    public Action<Item> HoveringItemReference;

    private void Start()
    {
        HoveringItemReference = InventoryController.current.HoveringItem;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HoveringItemReference != null)
            HoveringItemReference.Invoke(thisItem());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HoveringItemReference != null)
            HoveringItemReference.Invoke(null);
    }
}
