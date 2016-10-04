﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SlotReceiveElement : BoardMonoBehaviour, IDropHandler {

    public GameObject itemInSlot
    {
        get
        {
            if (transform.childCount > 0)
                return transform.GetChild(0).gameObject;
            else
            
                return null;

        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (BoardManager.itemBeingDragged.tag == "Element")
        { 
            
            if (itemInSlot && itemInSlot.GetComponent<DraggableToSlot>())
            {
                DraggableToSlot itemInSlotComponent = itemInSlot.GetComponent<DraggableToSlot>();
                itemInSlot.transform.SetParent(itemInSlotComponent.originSlot);
            }
            DraggableElementDisplay draggableElementDisplay = BoardManager.itemBeingDragged.GetComponent<DraggableElementDisplay>();
            draggableElementDisplay.transform.SetParent(transform);

            foreach (KeyValuePair<string,int> kvp in draggableElementDisplay.Element.Aspects)
            {
                Debug.Log(kvp.Key +": " + kvp.Value);
            }

        }
    }
}
