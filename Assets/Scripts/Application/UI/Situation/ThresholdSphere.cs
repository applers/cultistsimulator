﻿using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.Spheres.Angels;
using SecretHistories.States.TokenStates;
using SecretHistories.UI;
using SecretHistories.UI.Scripts;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;

using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace SecretHistories.UI {

    
    public class ThresholdSphere : Sphere, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,IInteractsWithTokens {

        public override SphereCategory SphereCategory => SphereCategory.Threshold;

        // VISUAL ELEMENTS
        public RecipeSlotViz viz;

        public TextMeshProUGUI SlotLabel;
        public Graphic border;
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;
        private SpherePath _thresholdSpherePath;


        public override bool AllowStackMerge { get { return false; } }


        public override bool AllowDrag {
            get {
                return !GoverningSphereSpec.Greedy;
            }
        }

        public override bool IsGreedy
        {
            get { return GoverningSphereSpec != null && GoverningSphereSpec.Greedy; }
        }

        public bool IsConsuming
        {
            get { return GoverningSphereSpec.Consumes; }
        }
        

        public void Start() {
            slotGlow.Hide();
            
        }


        public void Initialise(SphereSpec sphereSpec,SpherePath pathForThisThreshold)
        {
            GoverningSphereSpec = sphereSpec;
            _thresholdSpherePath = pathForThisThreshold;
            gameObject.name = GetPath().ToString();

            SlotLabel.text = sphereSpec.Label;
            if (sphereSpec.Greedy)
            {
                GreedyAngel greedyAngel = new GreedyAngel();
                greedyAngel.SetThresholdToGrabTo(this);
                greedyAngel.SetWatch(Registry.Get<SphereCatalogue>().GetDefaultWorldSphere());
                greedyAngel.SetWatch(Registry.Get<SphereCatalogue>().GetDefaultEnRouteSphere());
                flock.AddAngel(greedyAngel);

            }

            var angelsToAdd = sphereSpec.MakeAngels();
            foreach(var a in angelsToAdd)
                AddAngel(a);

            //refactor to something more generic
            GreedyIcon.SetActive(sphereSpec.Greedy);
            ConsumingIcon.SetActive(sphereSpec.Consumes);

        }

        public override SpherePath GetPath()
        {
            return _thresholdSpherePath;
        }


        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (GoverningSphereSpec.Greedy) // never show glow for greedy slots
                return;

            //if we're not dragging anything, and the slot is empty, glow the slot.
            if (!eventData.dragging) {
                if (GetTokenInSlot() == null)
                    ShowHoverGlow();
            }
            else
            {
                var draggedToken = eventData.pointerDrag.GetComponent<Token>();

                if (CanInteractWithToken(draggedToken)) {
                    draggedToken.ShowPossibleInteractionWithToken(draggedToken);

                    if (GetTokenInSlot() == null) // Only glow if the slot is empty
                        ShowHoverGlow();
                }
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData) {
            if (GoverningSphereSpec.Greedy) // we're greedy? No interaction.
                return;

            if(eventData.dragging)
            {
                var potentialDragToken = eventData.pointerDrag.GetComponent<Token>();

                if ( potentialDragToken != null)
                    potentialDragToken.ShowPossibleInteractionWithToken(potentialDragToken);
            }

            HideHoverGlow();
        }

        public void SetGlowColor(UIStyle.TokenGlowColor colorType) {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
        }

        public void SetGlowColor(Color color) {
            slotGlow.SetColor(color);
        }


        private  void ShowHoverGlow() {

                SetGlowColor(UIStyle.TokenGlowColor.OnHover);
                SoundManager.PlaySfx("TokenHover");
                slotGlow.Show();

        }

        private void HideHoverGlow()
        {
            SetGlowColor(UIStyle.TokenGlowColor.Default);
                SoundManager.PlaySfx("TokenHoverOff");
        }


        public override void DisplayAndPositionHere(Token token, Context context) {
            base.DisplayAndPositionHere(token, context);
            Choreographer.PlaceTokenAtFreeLocalPosition(token,context);

            slotIconHolder.transform.SetAsLastSibling();
            
        }

        public Token GetTokenInSlot() {
            return GetComponentInChildren<Token>();
        }

        public Token GetElementTokenInSlot()
        {
            if (GetElementTokens().Count() > 1)
            {
                NoonUtility.Log("Something weird in slot " + GoverningSphereSpec.Id +
                                ": it has more than one stack, so we're just returning the first.");
                return GetElementTokens().First();

            }

            return GetElementTokens().SingleOrDefault();
        }


        public override void TryMoveAsideFor(Token potentialUsurper, Token incumbent, out bool incumbentMoved) {
            if (IsGreedy) { // We do not allow
                incumbentMoved = false;
                return;
            }

            //incomer is a token. Does it fit in the slot?
            if (GetMatchForStack(potentialUsurper.ElementStack).MatchType==SlotMatchForAspectsType.Okay && potentialUsurper.ElementQuantity == 1)
            {
                incumbentMoved = true;
                incumbent.GoAway(new Context(Context.ActionSource.PlayerDrag)); //do this first; AcceptToken will trigger an update on the displayed aspects
                AcceptToken(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
            }
            else
                incumbentMoved = false;
        }

        public override bool TryAcceptToken(Token token,Context context)
        {

            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForStack(token.ElementStack);

            if (match.MatchType != SlotMatchForAspectsType.Okay)
            {
                token.SetState(new RejectedBySphereState());
                token.GoAway(context);

                var notifier = Registry.Get<INotifier>();

                var compendium = Registry.Get<Compendium>();

                if (notifier != null)
                    notifier.ShowNotificationWindow(Registry.Get<ILocStringProvider>().Get("UI_CANTPUT"), match.GetProblemDescription(compendium), false);
            }
            else if (token.ElementQuantity != 1)
            {
                // We're dropping a stack of >1?
                // set main stack to be returned to start position
                token.SetState(new RejectedViaSplit());
                // And we split a new one that's 1 (leaving the returning card to be n-1)
                var newStack = token.CalveToken(1, new Context(Context.ActionSource.PlayerDrag));
                // And we put that into the slot
                AcceptToken(newStack, context);
            }
            else
            {
                //it matches. Now we check if there's a token already there, and replace it if so:
                var currentOccupant = GetElementTokens().FirstOrDefault();

                // if we drop in the same slot where we came from, do nothing.
                if (currentOccupant == token)
                {
                    token.SetState(new DroppedInSphereState());
                    return false;
                }

                if (currentOccupant != null)
                    NoonUtility.LogWarning("There's still a card in the slot when this reaches the slot; it wasn't intercepted by being dropped on the current occupant. Rework.");
                //currentOccupant.ReturnToTabletop();

                //now we put the token in the slot.
                token.SetState(new DroppedInSphereState());
                AcceptToken(token, context);
                SoundManager.PlaySfx("CardPutInSlot");
            }

            return true;
        }



        public override void ActivatePreRecipeExecutionBehaviour() {
            if (GoverningSphereSpec.Consumes) {
                var token = GetElementTokenInSlot();

                if (token != null)
                    token.ElementStack.MarkedForConsumption = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            bool highlightGreedy = GreedyIcon.gameObject.activeInHierarchy && eventData.hovered.Contains(GreedyIcon);
            bool highlightConsumes = ConsumingIcon.gameObject.activeInHierarchy && eventData.hovered.Contains(ConsumingIcon);

            Registry.Get<INotifier>().ShowSlotDetails(GoverningSphereSpec, highlightGreedy, highlightConsumes);

        }


        public bool CanInteractWithToken(Token token)
        {

            if (GetElementTokenInSlot() != null)
                return false; // Slot is filled? Don't highlight it as interactive
            if (IsGreedy)
                return false; // Slot is greedy? It can never take anything.


            if (!token.ElementStack.IsValidElementStack())
                return false; // we only accept stacks

            //does the token match the slot? Check that first
            ContainerMatchForStack match = GetMatchForStack(token.ElementStack);

            return match.MatchType == SlotMatchForAspectsType.Okay;
        }

        public void ShowPossibleInteractionWithToken(Token token)
        {
            slotGlow.Show(false);
        }

        public void StopShowingPossibleInteractionWithToken(Token token)
        {
            slotGlow.Hide(false);

        }
    }


    
}