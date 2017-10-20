using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using Noon;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.TabletopUi {
    public class SituationController : ISituationSubscriber {
        public ISituationAnchor situationToken;
        private ISituationDetails situationWindow;
        public ISituation Situation;
        private readonly ICompendium compendium;
        private readonly Character currentCharacter;

        public SituationController(ICompendium co, Character ch) {
            compendium = co;
            currentCharacter = ch;
        }

        public void Initialise(SituationCreationCommand command, ISituationAnchor t, ISituationDetails w) {
            situationToken = t;
            situationToken.Initialise(command.GetBasicOrCreatedVerb(), this);

            situationWindow = w;
            situationWindow.Initialise(command.GetBasicOrCreatedVerb(), this);

            Situation = new Situation(command.TimeRemaining, command.State, command.Recipe, this);

            if (command.State == SituationState.Unstarted) {
                situationWindow.SetUnstarted();
            }
            else if (command.State == SituationState.FreshlyStarted || command.State == SituationState.Ongoing || command.State == SituationState.RequiringExecution) {
                if (command.State == SituationState.FreshlyStarted)
                    Situation.Start(command.Recipe);
                //ugly subclause here. Situation.Start largely duplicates the constructor. I'm trying to use the same code path for recreating situations from a save file as for beginning a new situation
                //possibly just separating out FreshlyStarted would solve it

                situationWindow.SetOngoing(command.Recipe);

                situationToken.DisplayMiniSlotDisplay(command.Recipe.SlotSpecifications);
                situationToken.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining);

                //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
                if (command.OverrideTitle != null)
                    situationWindow.Title = command.OverrideTitle;
                if (command.OverrideDescription != null)
                    situationWindow.Description = command.OverrideDescription;

            }
            else if (command.State == SituationState.Complete) {
                Situation = new Situation(this);
                situationWindow.SetComplete();

                //this is a little ugly here; but it makes the intent clear. The best way to deal with it is probably to pass the whole Command down to the situationwindow for processing.
                if (command.OverrideTitle != null)
                    situationWindow.Title = command.OverrideTitle;
                if (command.OverrideDescription != null)
                    situationWindow.Description = command.OverrideDescription;
                //NOTE: only on Complete state. Completioncount shouldn't show on other states. This is fragile tho.
                if (command.CompletionCount > 0)
                    situationToken.SetCompletionCount(command.CompletionCount);

            }
            else {
                throw new ApplicationException("Tried to create situation for " + command.Verb.Label +
                                               " with unknown state");
            }
        }

        public void OpenSituation() {
            situationToken.OpenToken();
            situationWindow.Show();
        }

        public void CloseSituation() {
            // This comes first so the token doesn't show a glow when it's being closed
            if (Situation.State == SituationState.Unstarted) 
                situationWindow.DumpAllStartingCardsToDesktop();

            situationToken.CloseToken();
            situationWindow.Hide();
        }

        public string GetCurrentRecipeId() {
            return Situation == null ? null : Situation.RecipeId;
        }

        public void StartingSlotsUpdated() {
            IAspectsDictionary allAspects = situationWindow.GetAspectsFromAllSlottedElements();
            Recipe recipeMatchingStartingAspects = compendium.GetFirstRecipeForAspectsWithVerb(allAspects, situationToken.Id, currentCharacter);

            IAspectsDictionary aspectsNoElementsSelf = situationWindow.GetAspectsFromAllSlottedElements(false);

            situationWindow.DisplayAspects(aspectsNoElementsSelf);

            if (recipeMatchingStartingAspects != null)
                situationWindow.DisplayStartingRecipeFound(recipeMatchingStartingAspects);
            else if (allAspects.Count > 0)
                situationWindow.DisplayNoRecipeFound();
            else
                situationWindow.SetUnstarted();
        }

        public void OngoingSlotsUpdated() {
            var allAspects = GetAspectsAvailableToSituation(false);
            situationWindow.DisplayAspects(allAspects);
            var rp = getNextRecipePrediction(allAspects);

            if (rp != null && rp.BurnImage != null)
                BurnImageHere(rp.BurnImage);

            situationWindow.UpdateTextForPrediction(rp);
            situationToken.UpdateMiniSlotDisplay(situationWindow.GetOngoingStacks());
        }

        private RecipePrediction getNextRecipePrediction(IAspectsDictionary aspects) {
            RecipeConductor rc = new RecipeConductor(compendium, aspects,
                new DefaultDice(), currentCharacter); //nb the use of default dice: we don't want to display any recipes without a 100% chance of executing
            return Situation.GetPrediction(rc);
        }

        private IAspectsDictionary GetAspectsAvailableToSituation(bool showElementAspects = true) {
            return situationWindow.GetAspectsFromAllSlottedElements(showElementAspects);
        }

        public HeartbeatResponse ExecuteHeartbeat(float interval) {
            HeartbeatResponse response = new HeartbeatResponse();

            RecipeConductor rc = new RecipeConductor(compendium,
                GetAspectsAvailableToSituation(), new Dice(), currentCharacter);

            Situation.Continue(rc, interval);

            if (Situation.State == SituationState.Ongoing) {
                var tokenAndSlot = new TokenAndSlot() {
                    Token = situationToken as SituationToken,
                    RecipeSlot = situationWindow.GetUnfilledGreedySlot() as RecipeSlot
                };

                if (tokenAndSlot.RecipeSlot != null)
                    response.SlotsToFill.Add(tokenAndSlot);
            }

            return response;
        }

        public void UpdateSituationDisplayForDescription() {
            RecipeConductor rc = new RecipeConductor(compendium, situationWindow.GetAspectsFromAllSlottedElements(), new Dice(), currentCharacter);

            var nextRecipePrediction = Situation.GetPrediction(rc);

            situationWindow.UpdateTextForPrediction(nextRecipePrediction);
        }

        public void SituationBeginning(Recipe withRecipe) {
            situationToken.UpdateMiniSlotDisplay(null); // Hide content of miniSlotDisplay - looping recipes never go by complete which would do that
            situationToken.DisplayMiniSlotDisplay(withRecipe.SlotSpecifications);
            situationWindow.SetOngoing(withRecipe);
            StoreStacks(situationWindow.GetStartingStacks());

            UpdateSituationDisplayForDescription();
        }

        public void SituationOngoing() {
            situationToken.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining);
            situationWindow.DisplayTimeRemaining(Situation.Warmup, Situation.TimeRemaining);
        }

        void StoreStacks(IEnumerable<IElementStack> stacks) {
            var inputStacks = situationWindow.GetOngoingStacks();
            var storageStackManager = situationWindow.GetStorageStacksManager();
            storageStackManager.AcceptStacks(inputStacks);
        }

        public void SituationExecutingRecipe(ISituationEffectCommand command) {
            //move any elements currently in OngoingSlots to situation storage
            //NB we're doing this *before* we execute the command - the command may affect these elements too
            StoreStacks(situationWindow.GetOngoingStacks());

            if (command.AsNewSituation) {
                IVerb verbForNewSituation = compendium.GetOrCreateVerbForCommand(command);
                SituationCreationCommand scc = new SituationCreationCommand(verbForNewSituation, command.Recipe, SituationState.FreshlyStarted, situationToken as DraggableToken);
                Registry.Retrieve<TabletopManager>().BeginNewSituation(scc);
                return;
            }

            currentCharacter.AddExecutionToHistory(command.Recipe.Id);
            var executor = new SituationEffectExecutor();
            executor.RunEffects(command, situationWindow.GetStorageStacksManager());

            if (command.Recipe.EndingFlag != null) {
                var ending = compendium.GetEndingById(command.Recipe.EndingFlag);
                Registry.Retrieve<TabletopManager>().EndGame(ending);
            }
        }

        public void SituationComplete() {
            var stacksToRetrieve = situationWindow.GetStoredStacks();
            INotification notification = new Notification(Situation.GetTitle(), Situation.GetDescription());
            SetOutput(stacksToRetrieve.ToList(), notification);

            //This must be run here: it disables (and destroys) any card tokens that have not been moved to outputs
            situationWindow.SetComplete();

            // Now update the token based on the current stacks in the window
            situationToken.DisplayComplete();
            situationToken.UpdateMiniSlotDisplay(situationWindow.GetOngoingStacks());
        }

        public void SituationHasBeenReset() {
            situationWindow.SetUnstarted();
            ResetToStartingState();
        }

        public void SetOutput(List<IElementStack> stacksForOutput, INotification notification) {
            situationWindow.SetOutput(stacksForOutput, notification);
        }

        public void AttemptActivateRecipe() {
            var aspects = situationWindow.GetAspectsFromAllSlottedElements();
            var recipe = compendium.GetFirstRecipeForAspectsWithVerb(aspects, situationToken.Id, currentCharacter);

            if (recipe == null)
                return;

            situationWindow.SetSlotConsumptions();
            situationWindow.StoreStacks(situationWindow.GetStartingStacks());
            situationWindow.DisplayStoredElements();
            Situation.Start(recipe);

            if (recipe.BurnImage != null)
                BurnImageHere(recipe.BurnImage);
        }

        private void BurnImageHere(string burnImage) {
            Registry.Retrieve<INotifier>()
                .ShowImageBurn(burnImage, situationToken as DraggableToken, 20f, 2f,
                    TabletopImageBurner.ImageLayoutConfig.CenterOnToken);
        }

        public void ModifyStoredElementStack(string elementId, int quantity) {
            situationWindow.GetStorageStacksManager().ModifyElementQuantity(elementId, quantity);
            situationWindow.DisplayStoredElements();
        }

        public void ResetToStartingState() {
            Situation.ResetIfComplete();
            //if this was a transient verb, clean up everything and finish.
            //otherwise, prep the window for the next recipe
            if (situationToken.IsTransient) {
                situationToken.Retire();
                situationWindow.Retire();
                //at the moment, the controller is accessed through the token
                //if we attach the controller to a third object, we'd need to retire that too
            }
            else {
                situationWindow.SetUnstarted();
                situationToken.SetCompletionCount(0);
            }
        }

        public Hashtable GetSaveDataForSituation() {
            var situationSaveData = new Hashtable();
            IGameDataExporter exporter = new GameDataExporter();

            situationSaveData.Add(SaveConstants.SAVE_VERBID, situationToken.Id);
            if (Situation != null) {
                situationSaveData.Add(SaveConstants.SAVE_TITLE, situationWindow.Title);
                situationSaveData.Add(SaveConstants.SAVE_DESCRIPTION, situationWindow.Description);
                situationSaveData.Add(SaveConstants.SAVE_RECIPEID, Situation.RecipeId);
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTATE, Situation.State);
                situationSaveData.Add(SaveConstants.SAVE_TIMEREMAINING, Situation.TimeRemaining);

                situationSaveData.Add(SaveConstants.SAVE_COMPLETIONCOUNT, situationToken.GetCompletionCount());
            }

            //save stacks in window (starting) slots
            if (situationWindow.GetStartingStacks().Any()) {
                var htStartingSlots = exporter.GetHashTableForStacks(situationWindow.GetStartingStacks());
                situationSaveData.Add(SaveConstants.SAVE_STARTINGSLOTELEMENTS, htStartingSlots);
            }

            //save stacks in ongoing slots
            if (situationWindow.GetOngoingStacks().Any()) {
                var htOngoingSlots = exporter.GetHashTableForStacks(situationWindow.GetOngoingStacks());
                situationSaveData.Add(SaveConstants.SAVE_ONGOINGSLOTELEMENTS, htOngoingSlots);
            }

            //save stacks in storage
            if (situationWindow.GetStoredStacks().Any()) {
                var htStacksInStorage = exporter.GetHashTableForStacks(situationWindow.GetStoredStacks());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONSTOREDELEMENTS, htStacksInStorage);
            }

            //save stacks in output
            if (situationWindow.GetOutputStacks().Any()) {
                var htStacksInOutput = exporter.GetHashTableForStacks(situationWindow.GetOutputStacks());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONOUTPUTSTACKS, htStacksInOutput);
            }

            //save output notes

            //save notes, and their contents
            if (situationWindow.GetOutputNotes().Any()) {
                var htOutputs = exporter.GetHashtableForOutputNotes(situationWindow.GetOutputNotes());
                situationSaveData.Add(SaveConstants.SAVE_SITUATIONOUTPUTNOTES, htOutputs);
            }
            return situationSaveData;
        }

        public IRecipeSlot GetSlotBySaveLocationInfoPath(string locationInfo, string slotType) {
            if (slotType == SaveConstants.SAVE_STARTINGSLOTELEMENTS) //hacky! this should  be an enum or something OOier
                return situationWindow.GetStartingSlotBySaveLocationInfoPath(locationInfo);
            else
                return situationWindow.GetOngoingSlotBySaveLocationInfoPath(locationInfo);
        }

        public bool IsSituationOccupied() {
            // This should return true when the situation is occupied:
            /*
            - if the situation isn't currently executing and the primary slot contains an element, it's occupied
            - if the situation is currently executing, or the primary slot doesn't contain an element, it's occupied
            */

            var stacks = situationWindow.GetStartingStacks();

            foreach (var stack in stacks) {
                if (!stack.Defunct)
                    return true;
            }

            return false; 
        }

        public void ShowDestinationsForStack(IElementStack stack) {
            situationWindow.ShowDestinationsForStack(stack);
        }
    }
}

