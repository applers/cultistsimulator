﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Elements;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;

public class Tabletop : MonoBehaviour, IContainsTokensView {

    private ElementStacksManager _stacksManager;


    public void SignalElementStackRemovedFromContainer(ElementStackToken elementStackToken)
    {

    }


    public void TryMoveAsideFor(SituationToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
    {
        //we're starting with the assumption that we don't want to attempt a merge if both tokens are elementstacks; that should be catered for elsewhere

        var freePos = Registry.Retrieve<Choreographer>().GetFreeTokenPositionWithDebug(incumbent, incumbent.GetRectTransform().anchoredPosition, 
            new Rect[1] { incumbent.GetRectTransform().rect }); // this is to ignore the current pos. Let's see what that does.

        incumbent.GetRectTransform().anchoredPosition = freePos;
        incumbentMoved = true;
        DisplaySituationTokenOnTable(potentialUsurper);
    }

    public void TryMoveAsideFor(ElementStackToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
    {
        //we're starting with the assumption that we don't want to attempt a merge if both tokens are elementstacks; that should be catered for elsewhere

        var freePos = Registry.Retrieve<Choreographer>().GetFreeTokenPositionWithDebug(incumbent, incumbent.GetRectTransform().anchoredPosition,
            new Rect[1] { incumbent.GetRectTransform().rect }); // this is to ignore the current pos. Let's see what that does.

        incumbent.GetRectTransform().anchoredPosition = freePos;
        incumbentMoved = true;
        _stacksManager.AcceptStack(potentialUsurper);
    }


    public ISituationAnchor CreateSituation(SituationCreationCommand creationCommand, string locatorInfo = null) {
        return Registry.Retrieve<SituationBuilder>().CreateTokenWithAttachedControllerAndSituation(creationCommand, locatorInfo);
    }

    public void DisplaySituationTokenOnTable(SituationToken token) {

        GetTokenTransformWrapper().DisplayHere(token);

        token.DisplayAtTableLevel();
    }


    public bool AllowDrag { get { return true; } }
    public bool AllowStackMerge { get { return true; } }


    public ElementStacksManager GetElementStacksManager() {
        //In some places we've done it Initialise. Here, we're testing if it's null and then assigning on the fly
        //This is because I'm going through and refactoring. Perhaps it should be consistent YOU TELL ME it's likely to get refactored further anyhoo
        if (_stacksManager == null)
        {
            _stacksManager = new ElementStacksManager(GetTokenTransformWrapper(),"tabletop");
            _stacksManager.EnforceUniqueStacks = true; // Martin: This ensures that this stackManager kills other copies when a unique is dropped in 
        }
        return _stacksManager;
    }

    public ITokenPhysicalLocation GetTokenTransformWrapper() {
        return new TabletopTokenTransformWrapper(transform);
    }

    public string GetSaveLocationInfoForDraggable(DraggableToken draggable) {
        return (draggable.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + draggable.RectTransform.localPosition.y).ToString();
    }

    // Returns a rect for use by the Choreographer
    public Rect GetRect() {
        var rectTrans = transform as RectTransform;
        return rectTrans.rect;
    }

    public void OnDestroy()
    {
        if (_stacksManager != null)
            _stacksManager.Deregister();
    }
}
