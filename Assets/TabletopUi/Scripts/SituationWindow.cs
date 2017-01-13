﻿using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Assets.TabletopUi.SlotsContainers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Should inherit from a "TabletopTokenWindow" base class, same as ElementDetailsWindow
namespace Assets.CS.TabletopUI
{
    public class SituationWindow : MonoBehaviour,ISituationDetails, IDropHandler
    {

        [SerializeField] CanvasGroupFader canvasGroupFader;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI description;

        [SerializeField] StartingSlotsContainer startingSlotsContainer;
        [SerializeField] OngoingSlotsContainer ongoingSlotsContainer;
        [SerializeField] SituationStorage situationStorage;
        [SerializeField] Results outputContainer;

        [SerializeField] AspectsDisplay aspectsDisplay;
        [SerializeField] Button button;
        [SerializeField] private TextMeshProUGUI ButtonBarText;
        [SerializeField] private TabletopContainer tabletopContainer;
        public IList<INotification> queuedNotifications = new List<INotification>();
        private SituationController situationController;
        private IVerb Verb;

        void OnEnable()
        {
            button.onClick.AddListener(HandleOnButtonClicked);
        }
        void OnDisable()
        {
            button.onClick.RemoveListener(HandleOnButtonClicked);
        }


        public void Initialise(IVerb verb, SituationController sc)
        {
            situationController = sc;
            Verb = verb;

            startingSlotsContainer.Initialise(sc);
            ongoingSlotsContainer.Initialise(sc);
            
        }

        public void Show()
        {
            canvasGroupFader.Show();

        }

        public void Hide()
        {
            canvasGroupFader.Hide();
        }

        public void DisplayStarting()
        {
            startingSlotsContainer.Reset();

            startingSlotsContainer.gameObject.SetActive(true);
            ongoingSlotsContainer.gameObject.SetActive(false);
            outputContainer.gameObject.SetActive(false);
            
            title.text = Verb.Label;
            description.text = Verb.Description;
            ButtonBarText.text = "";

            ButtonBarText.gameObject.SetActive(false);
        }

        public void DisplayOngoing(Recipe forRecipe) {

            startingSlotsContainer.gameObject.SetActive(false);
            ongoingSlotsContainer.gameObject.SetActive(true);
            outputContainer.gameObject.SetActive(false);

            ongoingSlotsContainer.SetUpSlots(forRecipe.SlotSpecifications);
           

            button.gameObject.SetActive(false);
            ButtonBarText.gameObject.SetActive(true);
        }

        public void DisplayComplete()
        {
            startingSlotsContainer.gameObject.SetActive(false);
            ongoingSlotsContainer.gameObject.SetActive(false);
            outputContainer.gameObject.SetActive(true);

            aspectsDisplay.ClearAspects();
            
        }

        public void DisplayAspects(IAspectsDictionary forAspects)
        {
            aspectsDisplay.DisplayAspects(forAspects);
        }

        public void UpdateTextForCandidateRecipe(Recipe r)
        {
            if (r != null)
            {
                title.text = r.Label;
                description.text = r.StartDescription;
                button.gameObject.SetActive(true);
            }
            else
            {
                title.text = "";
                description.text = "[If I experiment further, I may find another combination.]";
                button.gameObject.SetActive(false);
            }
        }


        public void UpdateTextForPrediction(RecipePrediction recipePrediction)
        {
            title.text = recipePrediction.Title;
            description.text = recipePrediction.DescriptiveText;
            ButtonBarText.text = recipePrediction.Commentary;

        }

        void HandleOnButtonClicked()
        {

            situationController.AttemptActivateRecipe();
 
        }

        public IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo)
        {
            return
                startingSlotsContainer.GetSlotBySaveLocationInfoPath(locationInfo);

        }

        public IEnumerable<IElementStack> GetStacksInStartingSlots()
        {
            return startingSlotsContainer.GetStacksInSlots();
        }

        public IEnumerable<IElementStack> GetStacksInOngoingSlots()
        {
            return ongoingSlotsContainer.GetStacksInSlots();
        }

        public AspectsDictionary GetAspectsFromAllSlottedElements()
        {
            var slottedAspects=new AspectsDictionary();
            slottedAspects.CombineAspects(startingSlotsContainer.GetAspectsFromSlottedCards());
            slottedAspects.CombineAspects(ongoingSlotsContainer.GetAspectsFromSlottedCards());

            return slottedAspects;
        }

        public IEnumerable<ISituationOutput> GetCurrentOutputs()
        {
            return outputContainer.GetCurrentOutputs();
        }

        public IRecipeSlot GetUnfilledGreedySlot()
        {

            return ongoingSlotsContainer.GetUnfilledGreedySlot();
        }


        public IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo)
        {
            return ongoingSlotsContainer.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public void RunSlotConsumptions()
        {
            foreach (var s in startingSlotsContainer.GetAllSlots())
                s.RunConsumption();

        }

        public void SetOutput(IEnumerable<IElementStack> stacks,INotification notification) {
            outputContainer.SetOutput(stacks,notification);
        }


        public IEnumerable<IElementStack> GetStoredStacks()
        {
            return GetSituationStorageStacksManager().GetStacks();
        }



        public void StoreStacks(IEnumerable<IElementStack> stacksToStore)
        {
            GetSituationStorageStacksManager().AcceptStacks(stacksToStore);
        }

        public IAspectsDictionary GetAspectsFromStoredElements()
        {
            return GetSituationStorageStacksManager().GetTotalAspects();
        }

        public ElementStacksManager GetSituationStorageStacksManager()
        {
            return situationStorage.GetElementStacksManager();
        }

        public void AllOutputsGone() {
            outputContainer.gameObject.SetActive(false);
            situationController.AllOutputsGone();
        }

        public void Retire()
        {
            Destroy(gameObject);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("on window");
        }
    }
}
