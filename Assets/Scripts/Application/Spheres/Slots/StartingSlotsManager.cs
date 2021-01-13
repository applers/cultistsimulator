﻿#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using UnityEngine;
using SecretHistories.Services;
using SecretHistories.UI.Scripts;
using SecretHistories.Constants;

namespace SecretHistories.UI.SlotsContainers {
    public class StartingSlotsManager : MonoBehaviour {

        [SerializeField] SlotGridManager gridManager;
        public CanvasGroupFader canvasGroupFader;
        protected List<Threshold> validSlots;

        protected Threshold primarySlot;
        private IVerb _verb;
        private SituationWindow _window;
        private SituationPath _situationPath;




        public IEnumerable<ElementStack> GetStacksInSlots()
        {
            IList<ElementStack> stacks = new List<ElementStack>();
            ElementStack stack;

            foreach (Threshold slot in GetAllSlots())
            {
                stack = slot.GetElementTokenInSlot().ElementStack;

                if (stack != null)
                    stacks.Add(stack);
            }

            return stacks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeElementAspects">true to return aspects for the elements themselves as well; false to include only their aspects</param>
        /// <returns></returns>
        public AspectsDictionary GetAspectsFromSlottedCards(bool includeElementAspects)
        {
            AspectsDictionary currentAspects = new AspectsDictionary();
            ElementStack stack;

            foreach (Threshold slot in GetAllSlots())
            {
                stack = slot.GetElementTokenInSlot().ElementStack;

                if (stack != null)
                    currentAspects.CombineAspects(stack.GetAspects(includeElementAspects));
            }

            return currentAspects;
        }

        public virtual IList<Threshold> GetAllSlots()
        {

            return validSlots;
        }

        public Threshold GetSlotBySaveLocationInfoPath(string saveLocationInfoPath)
        {
            var candidateSlots = GetAllSlots();
            Threshold slotToReturn = candidateSlots.SingleOrDefault(s => s.GetPath().ToString() == saveLocationInfoPath);
            return slotToReturn;
        }


        public void Initialise(IVerb verb,SituationWindow window,SituationPath situationPath) {
            
            
            var children = GetComponentsInChildren<Threshold>();
            var allSlots = new List<Threshold>(children);
            validSlots = new List<Threshold>(allSlots.Where(rs => rs.Defunct == false && rs.GoverningSphereSpec != null));

            _verb = verb;
            _window= window;
            _situationPath = situationPath;
            

        var primarySlotSpecification = verb.Slot;
            if(primarySlotSpecification!=null)
                primarySlot = BuildSlot(primarySlotSpecification.Label, primarySlotSpecification, null,false);
            else
                primarySlot = BuildSlot("Primary recipe slot", new SphereSpec(), null);


            var otherslots = verb.Slots;
            if(otherslots!=null)
                foreach (var s in otherslots)
                    BuildSlot(s.Label, s, null);;

        }

        public void UpdateDisplay(Situation situation)
        {

            if(situation.CurrentState.IsActiveInThisState(primarySlot))
                    canvasGroupFader.Show();
            else
                    canvasGroupFader.Hide();
        }

        
        public void RespondToStackAdded(Threshold slot, ElementStack stack, Context context) {
            //currently, nothing calls this - it used to be OnCardAdded. I hope we can feed it through the primary event flow

            _window.TryResizeWindow(GetAllSlots().Count);
           

            if (slot.IsPrimarySlot() && stack.HasChildSlotsForVerb(_verb.Id))
                AddSlotsForStack(stack, slot);

            ArrangeSlots();


        }

        protected void AddSlotsForStack(ElementStack stack, Threshold parentSlot) {

            foreach (var childSlotSpecification in stack.GetChildSlotSpecificationsForVerb(_verb.Id))
            {
                var slot = BuildSlot("childslot of " + stack.Element.Id, childSlotSpecification, parentSlot);
                parentSlot.childSlots.Add(slot);
            }
        }

        public void RespondToStackRemoved(ElementStack stack, Context context) {
            //currently, nothing calls this - it used to be OnCardAdded. I hope we can feed it through the primary event flow
            // startingSlots updated may resize window


            // Only update the slots if we're doing this manually, otherwise don't
            // Addendum: We also do this when retiring a card - Martin
            if (context.IsManualAction() || context.actionSource == Context.ActionSource.Retire)
                RemoveAnyChildSlotsWithEmptyParent(context);

            ArrangeSlots();

        }

        public void RemoveAnyChildSlotsWithEmptyParent(Context context) {
            // We get a copy of the list, since it modifies itself when slots are removed
            List<Threshold> currentSlots = new List<Threshold>(GetAllSlots());

            foreach (Threshold s in currentSlots) {
                if (s != null && s.GetElementTokenInSlot() == null && s.childSlots.Count > 0) {
                    List<Threshold> currentChildSlots = new List<Threshold>(s.childSlots);
                    s.childSlots.Clear();

                    foreach (Threshold cs in currentChildSlots)
                        ClearAndDestroySlot(cs, context);
                }
            }

        }


        protected virtual Threshold BuildSlot(string slotName, SphereSpec sphereSpec, Threshold parentSlot, bool wideLabel = false)
        {
            var slot = Registry.Get<PrefabFactory>().CreateLocally<Threshold>(transform);

            slot.name = slotName + (sphereSpec != null ? " - " + sphereSpec.Id : "");
            slot.ParentSlot = parentSlot;
            sphereSpec.MakeActiveInState(StateEnum.Unstarted);
            slot.Initialise(sphereSpec,_situationPath);
            
            if (wideLabel)
            {
                var slotTransform = slot.SlotLabel.GetComponent<RectTransform>();
                var originalSize = slotTransform.sizeDelta;
                slotTransform.sizeDelta = new Vector2(originalSize.x * 1.5f, originalSize.y * 0.75f);
            }

            validSlots.Add(slot);

            gridManager.AddSlot(slot);
            return slot;
        }


        protected  void ClearAndDestroySlot(Threshold slot, Context context) {
            if (slot == null)
                return;
            if (slot.Defunct)
                return;

            validSlots.Remove(slot);

            // This is all copy & paste from the parent class except for the last line
            if (slot.childSlots.Count > 0) {
                List<Threshold> childSlots = new List<Threshold>(slot.childSlots);
                foreach (var cs in childSlots)
                    ClearAndDestroySlot(cs, context);

                slot.childSlots.Clear();
            }

            //Destroy the slot *before* returning the token to the tabletop
            //otherwise, the slot will fire OnCardRemoved again, and we get an infinte loop
            gridManager.RetireSlot(slot);

            if (context != null && context.actionSource == Context.ActionSource.SituationStoreStacks)
                return; // Don't return the tokens to tabletop if we

            Token tokenContained = slot.GetTokenInSlot();

            if (tokenContained != null)
                tokenContained.GoAway(context);
        }

        public void ArrangeSlots() {
            gridManager.ReorderSlots();
        }

        public void SetGridNumPerRow() {
            gridManager.SetNumPerRow();
        }
    }

}
