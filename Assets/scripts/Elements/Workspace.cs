﻿using UnityEngine;
using System.Collections;

public class Workspace : BoardMonoBehaviour
{

    [SerializeField] private SlotReceiveVerb VerbSlot;
    private GameObject RootElementSlot;

    public bool IsRootElementPresent { get { return RootElementSlot != null; } }
    public string GetCurrentVerbId()
    {
        return VerbSlot.GetCurrentVerbId();
    }

    public DraggableElementToken[] GetCurrentElements()
    {
        return GetComponentsInChildren<DraggableElementToken>();
    }

    public void MakeFirstSlotAvailable(Vector3 governorPosition,GameObject prefabEmptyElementSlot)
    {
        if(!IsRootElementPresent)
        { 
        int governedStepRight = 50;
        int nudgeDown = -10;
       RootElementSlot = Instantiate(prefabEmptyElementSlot, transform, false) as GameObject;
        Vector3 newSlotPosition = new Vector3(governorPosition.x + governedStepRight, governorPosition.y + nudgeDown);
        RootElementSlot.transform.localPosition = newSlotPosition;
        }
    }

    public void ReturnEverythingToOrigin()
    {
        

        SlotReceiveElement[] slots = this.GetComponentsInChildren<SlotReceiveElement>();
        foreach(SlotReceiveElement slot in slots)
        { 
            slot.ClearThisSlot();
            BM.ExileToLimboThenDestroy(slot.gameObject);
        }
        SlotReceiveVerb verbSlot = this.GetComponentInChildren<SlotReceiveVerb>();
        if (verbSlot != null)
            verbSlot.ClearThisSlot();
            

    }

    public void ConsumeElements()
    {
        DraggableElementToken[] elements = this.GetComponentsInChildren<DraggableElementToken>();

        foreach (DraggableElementToken element in elements)
            if(!element.HasChildSlots())
                element.SetQuantity(0);

        BM.ClearWorkspace();
          
    }
}
