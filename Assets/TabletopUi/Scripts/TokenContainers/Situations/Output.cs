﻿#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.TabletopUi;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.Scripts.Infrastructure;
using Noon;
using TMPro;

/// <summary>
/// The contents of Output spheres can be picked up by the player, but not replaced. They become unavailable once empty.
/// </summary>
public class Output : Sphere {

    public CanvasGroupFader canvasGroupFader;
    [SerializeField] SituationResultsPositioning cardPos;
    [SerializeField] TextMeshProUGUI dumpResultsButtonText;

    public override SphereCategory SphereCategory => SphereCategory.Output;

    private string buttonClearResultsDefault;
    private string buttonClearResultsNone;

    
    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return false; } }


    public void Initialise() {
        buttonClearResultsDefault = "VERB_COLLECT";
        buttonClearResultsNone = "VERB_ACCEPT";
    }

    public void UpdateDisplay(Situation situation)
    {
        switch (situation.State)
        {
            case SituationState.Complete:
                canvasGroupFader.Show();
                break; 
            default:
                canvasGroupFader.Hide();
                break;
        }

    }


    public void SetOutput(List<ElementStack> allStacksToOutput) {
        if (allStacksToOutput.Any() == false)
            return;

        AcceptStacks(allStacksToOutput, new Context(Context.ActionSource.SituationResults));

        //currently, if the first stack is fresh, we'll turn it over anyway. I think that's OK for now.
        //cardPos.ReorderCards(allStacksToOutput);
        // we noew reorder on DisplayHere
    }



    public override void AcceptToken(Token token, Context context)
    {
        base.AcceptToken(token, context);
        token.Shroud(true);
    }
        //stack.Shroud(true);)

    public override void DisplayHere(ElementStack stack, Context context) {
        base.DisplayHere(stack, context);
        cardPos.ReorderCards(GetStackTokens());
    }

    public override void RemoveStack(ElementStack elementStack) {
        // Did we just drop the last available token? 
        // Update the badge, then reorder cards?

        UpdateDumpButtonText();

        bool cardsRemaining = false;
        IEnumerable<ElementStack> stacks = GetOutputStacks();

        // Window is open? Check if it was the last card, then reset automatically
        if (gameObject.activeInHierarchy) {
            foreach (var item in stacks) {
                if (item != null && item.Defunct == false) {
                    cardsRemaining = true;
                    break;
                }
            }
        }
        else {
            // Window is closed? ensure we never reset only reorder
            cardsRemaining = true;
        }

        //if (!cardsRemaining)
        //    controller.ResetSituation();
        //else
        //    cardPos.ReorderCards(stacks);

        if (cardsRemaining)
            cardPos.ReorderCards(stacks);
    }

    public IEnumerable<ElementStack> GetOutputStacks() {
        return GetStackTokens();
    }

    public override SpherePath GetPath()
    {
        if (!string.IsNullOrEmpty(pathIdentifier))
            NoonUtility.Log($"We're trying to specify a spherepath ({pathIdentifier}) in an output sphere");
        return new SpherePath("output");
        //   return (token.RectTransform.localPosition.x.ToString() + SaveConstants.SEPARATOR + token.RectTransform.localPosition.y).ToString();
    }

    public void UpdateDumpButtonText() {
        if (GetOutputStacks().Any())
            dumpResultsButtonText.GetComponent<Babelfish>().UpdateLocLabel(buttonClearResultsDefault);
        else
            dumpResultsButtonText.GetComponent<Babelfish>().UpdateLocLabel(buttonClearResultsNone);
    }
    
}
