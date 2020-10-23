﻿#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.SlotsContainers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Services;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.CS.TabletopUI {
    [RequireComponent(typeof(SituationWindowPositioner))]
    public class SituationWindow : AbstractToken,ISituationSubscriber {

        string buttonDefault;
        string buttonBusy;

		[Header("Visuals")]
		[SerializeField] CanvasGroupFader canvasGroupFader;
        public SituationWindowPositioner positioner;

        [Space]
        [SerializeField] Image artwork;
        [SerializeField] TextMeshProUGUI title;
		public PaginatedText PaginatedNotes;

		[Space]
        [SerializeField] StartingSlotsManager startingSlots;

        [Space]
        [SerializeField] OngoingSlotManager ongoing;

        [Space]
        [SerializeField] SituationResults results;
		[SerializeField] Button dumpResultsButton;
        [SerializeField] TextMeshProUGUI dumpResultsButtonText;

        [Space]
        [SerializeField] SituationStorage storage;

        [Space]
        [SerializeField] AspectsDisplay aspectsDisplay;

		[SerializeField] Button startButton;
		[SerializeField] TextMeshProUGUI startButtonText;

        public UnityEvent OnStartButtonClicked;
        public UnityEvent OnCollectButtonClicked;
        public UnityEvent OnWindowClosed;

        public TokenLocation LastOpenLocation;

        private IVerb Verb;
        private bool windowIsWide = false;

        public bool IsOpen {
            get { return gameObject.activeInHierarchy; }
        }

		public string Title {
			get { return title.text; }
			set { title.text = value; }
        }

		public Vector3 Position
		{
			get { return positioner.GetPosition(); }
			set { positioner.SetPosition( value ); }
		}
        // INIT & LIFECYCLE

        void OnEnable()
        {

          
            buttonDefault = "VERB_START";
			buttonBusy = "VERB_RUNNING";
        }

        public void TryResizeWindow(int slotsCount)
        {
            SetWindowSize(slotsCount > 3);
        }
        public void DisplayIcon(string icon)
        {
            Sprite sprite = ResourcesManager.GetSpriteForVerbLarge(icon);
            artwork.sprite = sprite;
        }

        public void Populate(Situation situation) {
			Verb = situation.Verb;
            name = "Window_" + Verb.Id;
            DisplayIcon(Verb.Id);

            if (Verb.Startable)
            {
                startButton.gameObject.SetActive(true);
            }
            else
            {
                startButton.gameObject.SetActive(false);
            }



            startingSlots.Initialise(Verb,this);
            ongoing.Initialise(Verb,this);
            results.Initialise();
		}

        public override void StartArtAnimation()
        {
            throw new NotImplementedException();
        }

        public override bool CanAnimate()
        {
            throw new NotImplementedException();
        }

        public override string EntityId { get; }

        public override void OnDrop(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public override bool CanInteractWithTokenDroppedOn(VerbAnchor tokenDroppedOn)
        {
            throw new NotImplementedException();
        }

        public override bool CanInteractWithTokenDroppedOn(ElementStackToken stackDroppedOn)
        {
            throw new NotImplementedException();
        }

        public override void InteractWithTokenDroppedOn(VerbAnchor tokenDroppedOn)
        {
            throw new NotImplementedException();
        }

        public override void InteractWithTokenDroppedOn(ElementStackToken stackDroppedOn)
        {
            throw new NotImplementedException();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public override void ReturnToTabletop(Context context)
        {
            throw new NotImplementedException();
        }

        protected override void NotifyChroniclerPlacedOnTabletop()
        {
            throw new NotImplementedException();
        }

        public override bool Retire()
        {
            var startingStacks = new List<ElementStackToken>(GetStartingStacks());
            foreach (var s in startingStacks)
                s.Retire(CardVFX.None);
            var ongoingStacks=new List<ElementStackToken>(GetOngoingStacks());
           foreach (var o in ongoingStacks)
               o.Retire(CardVFX.None);


           storage.RemoveAllStacks();
            results.RemoveAllStacks();
            Destroy(gameObject);

            return true;
        }

        public override void ReactToDraggedToken(TokenInteractionEventArgs args)
        {
            throw new NotImplementedException();
        }

        public override void HighlightPotentialInteractionWithToken(bool show)
        {
            throw new NotImplementedException();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        
        public void Close() {
        OnWindowClosed.Invoke();
        }

        // BASIC DISPLAY

        public void Show( Vector3 targetPosOverride )
		{
			if (!gameObject.activeInHierarchy)
			{
				SoundManager.PlaySfx("SituationWindowShow");
			}

			canvasGroupFader.Show();
            positioner.Show(canvasGroupFader.durationTurnOn, targetPosOverride); // Animates the window (position allows optional change is position)
            results.UpdateDumpButtonText(); // ensures that we've updated the dump button accordingly
            startingSlots.ArrangeSlots(); //won't have been arranged if a card was dumped in while the window was closed
 			PaginatedNotes.Reset();
        }

		public void Hide() {
			if (gameObject.activeInHierarchy)
				SoundManager.PlaySfx("SituationWindowHide");

			canvasGroupFader.Hide();
        }


        public void DisplayInitialState()
        {
            Title = Verb.Label;
            PaginatedNotes.SetText(Verb.Description);

            DisplayButtonState(false);
        }


        public void DisplayNoRecipeFound() {
			Title = Verb.Label;
			PaginatedNotes.SetText(Verb.Description);
            
			DisplayButtonState(false);
        }

        public void DisplayStartingRecipeFound(Recipe r,AspectsDictionary aspectsInSituation) {


            Title = r.Label;
            //Check for possible text refinements based on the aspects in context
            TextRefiner tr = new TextRefiner(aspectsInSituation);
            PaginatedNotes.SetText(tr.RefineString(r.StartDescription));

            DisplayTimeRemaining(r.Warmup, r.Warmup, r.SignalEndingFlavour); //Ensures that the time bar is set to 0 to avoid a flicker
			DisplayButtonState(true);

            SoundManager.PlaySfx("SituationAvailable");
        }

        public void DisplayHintRecipeFound(Recipe r, AspectsDictionary aspectsInSituation)
        
            {
            Title = Registry.Get<ILocStringProvider>().Get("UI_HINT") + " " + r.Label;
            //Check for possible text refinements based on the aspects in context
        TextRefiner tr = new TextRefiner(aspectsInSituation);

            PaginatedNotes.SetText("<i>" + tr.RefineString(r.StartDescription) + "</i>");
            DisplayButtonState(false);
            

            SoundManager.PlaySfx("SituationAvailable");
        }

       public void SetWindowSize(bool wide) {
            RectTransform rectTrans = transform as RectTransform;

            if (wide)
                rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 900f);
            else
                rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 700f);

            if (wide != windowIsWide) {
                if (wide)
                    rectTrans.anchoredPosition = rectTrans.anchoredPosition + new Vector2(100f, 0f);
                else
                    rectTrans.anchoredPosition = rectTrans.anchoredPosition - new Vector2(100f, 0f);

                startingSlots.SetGridNumPerRow(); // Updates the grid row numbers
                ongoing.SetSlotToPos(); // Updates the ongoing slot position
            }

            windowIsWide = wide;

           startingSlots.ArrangeSlots();
        }

        public void DisplayAspects(IAspectsDictionary forAspects) {
			aspectsDisplay.DisplayAspects(forAspects);
		}

        public void DisplayStoredElements() {
            ongoing.ShowStoredAspects(GetStoredStacks());
        }

        public void DisplayTimeRemaining(float duration, float timeRemaining, EndingFlavour forEndingFlavour) {
            ongoing.UpdateTime(duration, timeRemaining, forEndingFlavour);
        }

        void DisplayButtonState(bool interactable, string text = null) {
			startButton.interactable = interactable;
            startButtonText.GetComponent<Babelfish>().UpdateLocLabel(string.IsNullOrEmpty(text) ? buttonDefault : text);
        }

        // ACTIONS

        
        public IEnumerable<ElementStackToken> GetStartingStacks() {
            return startingSlots.GetStacksInSlots();
        }

        public IEnumerable<ElementStackToken> GetOngoingStacks() {
            return ongoing.GetStacksInSlots();
        }

        public IEnumerable<ElementStackToken> GetStoredStacks() {
            return storage.GetStacks();
        }





        public void StoreStacks(IEnumerable<ElementStackToken> stacksToStore)
        {
            storage.AcceptStacks(stacksToStore, new Context(Context.ActionSource.SituationStoreStacks));
            // Now that we've stored stacks, make sure we update the starting slots
            startingSlots.RemoveAnyChildSlotsWithEmptyParent(new Context(Context.ActionSource.SituationStoreStacks)); 
        }

        
        public RecipeSlot GetPrimarySlot() {
            return startingSlots.GetAllSlots().FirstOrDefault();
        }

        public RecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo) {
            return startingSlots.GetSlotBySaveLocationInfoPath(locationInfo);
        }

        public RecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo) {
            return ongoing.GetSlotBySaveLocationInfoPath(locationInfo);
        }


        public IList<RecipeSlot> GetStartingSlots() {
            return startingSlots.GetAllSlots();
        }

        public IList<RecipeSlot> GetOngoingSlots() {
            return ongoing.GetAllSlots();
        }

        public SituationStorage GetStorageContainer()
        {
            return storage;
        }

        public SituationResults GetResultsContainer()
        {
            return results;
        }




        public IEnumerable<ISituationNote> GetNotes() {
            return PaginatedNotes.GetCurrentTexts();
        }

        public void SituationBeginning(SituationEventData e)
        {
         
            startingSlots.gameObject.SetActive(false);

         
            ongoing.SetupSlot(e.CurrentRecipe);
            ongoing.ShowDeckEffects(e.CurrentRecipe.DeckEffects);
            ongoing.gameObject.SetActive(true);

            results.gameObject.SetActive(false);
            DisplayButtonState(false, buttonBusy);

            SetWindowSize(false); //always collapse the window if we don't need to display multiple slots

        }

        public void SituationOngoing(SituationEventData e)
        {
            DisplayTimeRemaining(e.Warmup, e.TimeRemaining, e.CurrentRecipe.SignalEndingFlavour);
        }

        public void SituationExecutingRecipe(SituationEventData e)
        {
            
        }

        public void SituationComplete(SituationEventData e)
        {
            startingSlots.gameObject.SetActive(false);
            ongoing.gameObject.SetActive(false);
            results.gameObject.SetActive(true);
            aspectsDisplay.ClearCurrentlyDisplayedAspects();

            results.UpdateDumpButtonText();
        }

        public void ResetSituation()
        {
            startingSlots.DoReset();
            startingSlots.gameObject.SetActive(true);

            ongoing.DoReset();
            ongoing.gameObject.SetActive(false);

            results.DoReset();
            results.gameObject.SetActive(false);

            Title = Verb.Label;
            PaginatedNotes.SetText(Verb.Description);
            DisplayButtonState(false);
            SetWindowSize(false);
        }

        public void ContainerContentsUpdated(SituationEventData e)
        {
  

            var allAspectsInSituation = AspectsDictionary.GetFromStacks(e.StacksInEachStorage.SelectMany(s => s.Value), true);
            

            var tabletopManager = Registry.Get<TabletopManager>();
            var aspectsInContext = tabletopManager.GetAspectsInContext(allAspectsInSituation);

            // Get all aspects and find a recipe
            Recipe matchingRecipe = Registry.Get<ICompendium>().GetFirstMatchingRecipe(aspectsInContext, Verb.Id, Registry.Get<Character>(), false);

            var allAspectsToDisplay = AspectsDictionary.GetFromStacks(e.StacksInEachStorage.SelectMany(s => s.Value), false);

            // Update the aspects in the window
            
            DisplayAspects(allAspectsToDisplay);

            //if we found a recipe, display it, and get ready to activate
            if (matchingRecipe != null)
            {
                DisplayStartingRecipeFound(matchingRecipe,allAspectsInSituation);
                return;
            }

            //if we can't find a matching craftable recipe, check for matching hint recipes
            Recipe matchingHintRecipe = Registry.Get<ICompendium>().GetFirstMatchingRecipe(aspectsInContext, e.ActiveVerb.Id , Registry.Get<Character>(), true); ;

            //perhaps we didn't find an executable recipe, but we did find a hint recipe to display
            if (matchingHintRecipe != null)
                DisplayHintRecipeFound(matchingHintRecipe,allAspectsInSituation);
            //no recipe, no hint? If there are any elements in the mix, display 'try again' message
            else if (allAspectsInSituation.Count > 0)
                DisplayNoRecipeFound();
            //no recipe, no hint, no aspects. Just set back to unstarted
            else
                DisplayInitialState();
        }

        public void ReceiveNotification(SituationEventData e)
        {
            PaginatedNotes.AddText(e.Notification.Description);
        }

        public void RecipePredicted(RecipePrediction recipePrediction)
        {
            Title = recipePrediction.Title;
            PaginatedNotes.AddText(recipePrediction.DescriptiveText);
        }
    }
}
