﻿#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// The contents of Output spheres can be picked up by the player, but not replaced. They become unavailable once empty.
/// </summary>
[IsEmulousEncaustable(typeof(Sphere))]
public class OutputSphere : Sphere,ISituationSubscriber{

    public CanvasGroupFader canvasGroupFader;
    [SerializeField] SituationResultsPositioning outputPositioning;
    [SerializeField] TextMeshProUGUI dumpResultsButtonText;

    [SerializeField] public UnityEvent AllTokenssCollected;

    public override SphereCategory SphereCategory => SphereCategory.Output;

    private string buttonClearResultsDefault = "VERB_COLLECT";
    private string buttonClearResultsNone = "VERB_ACCEPT";

    
    public override bool AllowDrag { get { return true; } }
    public override bool AllowStackMerge { get { return false; } }
    public override float TokenHeartbeatIntervalMultiplier => 1;

    

    public override void DisplayAndPositionHere(Token token, Context context) {
        base.DisplayAndPositionHere(token, context);

       outputPositioning.ArrangeTokens(GetElementTokens());
    }

    public override void RemoveToken(Token token,Context context) {
        // Did we just drop the last available token? 
        // Update the badge, then reorder cards?

        UpdateDumpButtonText();

        bool cardsRemaining = false;
        IEnumerable<Token> stacks = GetElementTokens();

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
            outputPositioning.ArrangeTokens(stacks);
        else
            AllTokenssCollected.Invoke();

        base.RemoveToken(token,context);
    }




    public void UpdateDumpButtonText() {
        if (GetElementTokens().Any())
            dumpResultsButtonText.GetComponent<Babelfish>().UpdateLocLabel(buttonClearResultsDefault);
        else
            dumpResultsButtonText.GetComponent<Babelfish>().UpdateLocLabel(buttonClearResultsNone);
    }

    public void SituationStateChanged(Situation situation)
    {
        if (situation.State.IsActiveInThisState(this))
        {
            canvasGroupFader.Show();
            outputPositioning.ArrangeTokens(GetElementTokens());
        }
        else
            canvasGroupFader.Hide();

        UpdateDumpButtonText();
    }

    public void TimerValuesChanged(Situation s)
    {
    }

    public void SituationSphereContentsUpdated(Situation s)
    {
    }

    public void ReceiveNotification(INotification n)
    {
    }

    public void ReceiveCommand(IAffectsTokenCommand command)
    {
        //can't make use of it
    }
}