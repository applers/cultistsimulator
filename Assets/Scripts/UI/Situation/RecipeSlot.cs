﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Scripts.UI;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Assets.CS.TabletopUI {



    public class RecipeSlot : Sphere, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,IInteractsWithTokens {

        public override SphereCategory SphereCategory => SphereCategory.Threshold;

        // DATA ACCESS

        public IList<RecipeSlot> childSlots { get; set; }
        public RecipeSlot ParentSlot { get; set; }

        // VISUAL ELEMENTS
        public RecipeSlotViz viz;

        public TextMeshProUGUI SlotLabel;
        public Graphic border;
        public GraphicFader slotGlow;
        public LayoutGroup slotIconHolder;

        public GameObject GreedyIcon;
        public GameObject ConsumingIcon;
        

        public override bool AllowStackMerge { get { return false; } }

        private SituationPath _situationPath;

        public override bool AllowDrag {
            get {
                return !GoverningSlotSpecification.Greedy;
            }
        }

        public override bool IsGreedy
        {
            get { return GoverningSlotSpecification != null && GoverningSlotSpecification.Greedy; }
        }

        public bool IsConsuming
        {
            get { return GoverningSlotSpecification.Consumes; }
        }

        public enum SlotModifier { Locked, Ongoing, Greedy, Consuming };

        public RecipeSlot() {
            childSlots = new List<RecipeSlot>();
        }

        public void Start() {
            slotGlow.Hide();
            
        }

        public void Initialise(SlotSpecification slotSpecification,SituationPath situationPath)
        {
            _situationPath = situationPath;
            GoverningSlotSpecification = slotSpecification;
            gameObject.name = GetPath().ToString();

            if (slotSpecification == null)
                return;

            SlotLabel.text = slotSpecification.Label;

            GreedyIcon.SetActive(slotSpecification.Greedy);
            ConsumingIcon.SetActive(slotSpecification.Consumes);

        }





        public virtual void OnPointerEnter(PointerEventData eventData) {
            if (GoverningSlotSpecification.Greedy) // never show glow for greedy slots
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
            if (GoverningSlotSpecification.Greedy) // we're greedy? No interaction.
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

        public bool HasChildSlots() {
            if (childSlots == null)
                return false;
            return childSlots.Count > 0;
        }



        
        

        public override void DisplayAndPositionHere(Token token, Context context) {
            base.DisplayAndPositionHere(token, context);
            Choreographer.PlaceTokenAtFreePosition(token,context);

            slotIconHolder.transform.SetAsLastSibling();
            
        }

        public Token GetTokenInSlot() {
            return GetComponentInChildren<Token>();
        }

        public Token GetElementTokenInSlot()
        {
            if (GetElementTokens().Count() > 1)
            {
                NoonUtility.Log("Something weird in slot " + GoverningSlotSpecification.Id +
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
                incumbent.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag)); //do this first; AcceptToken will trigger an update on the displayed aspects
                AcceptToken(potentialUsurper, new Context(Context.ActionSource.PlayerDrag));
            }
            else
                incumbentMoved = false;
        }



        /// <summary>
        /// path to slot expressed in underscore-separated slot specification labels: eg "work_sacrifice"
        /// </summary>
        public override SpherePath GetPath()
        {

            SpherePath path;
            if (ParentSlot != null)
                path = new SpherePath(ParentSlot.GetPath(), GoverningSlotSpecification.Id);
            else
                path = new SpherePath(_situationPath,GoverningSlotSpecification.Id);


            if (!string.IsNullOrEmpty(pathIdentifier))
                NoonUtility.Log($"We're trying to specify a spherepath ({pathIdentifier}) in a recipe slot / threshold ({path})");
            return path;
        }

        public override void ActivatePreRecipeExecutionBehaviour() {
            if (GoverningSlotSpecification.Consumes) {
                var token = GetElementTokenInSlot();

                if (token != null)
                    token.ElementStack.MarkedForConsumption = true;
            }
        }

        public bool IsPrimarySlot()
        {
            return ParentSlot == null;
        }

        public void OnPointerClick(PointerEventData eventData) {
            bool highlightGreedy = GreedyIcon.gameObject.activeInHierarchy && eventData.hovered.Contains(GreedyIcon);
            bool highlightConsumes = ConsumingIcon.gameObject.activeInHierarchy && eventData.hovered.Contains(ConsumingIcon);

            Registry.Get<INotifier>().ShowSlotDetails(GoverningSlotSpecification, highlightGreedy, highlightConsumes);

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