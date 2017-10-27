﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using TMPro;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationDetails {
        string Title { get; set; }
        string Description { get; set; }

        void Initialise(IVerb verb,SituationController controller);

        void Show();
        void Hide();

        
        void DisplayAspects(IAspectsDictionary forAspects);
        void DisplayStartingRecipeFound(Recipe r);

        IRecipeSlot GetStartingSlotBySaveLocationInfoPath(string locationInfo);

        void DisplayAspects(IAspectsDictionary forAspects);
        IAspectsDictionary GetAspectsFromAllSlottedElements(bool showElementAspects = true);

        void SetOutput(List<IElementStack> stacks,INotification notification);
        void SetUnstarted();
        void SetOngoing(Recipe forRecipe);
        void UpdateTextForPrediction(RecipePrediction recipePrediction);

        void DumpAllStartingCardsToDesktop();

        IEnumerable<IElementStack> GetStartingStacks();
        IEnumerable<IElementStack> GetOngoingStacks();
        IEnumerable<IElementStack> GetStoredStacks();
        IEnumerable<IElementStack> GetOutputStacks();

        void StoreStacks(IEnumerable<IElementStack> stacksToStore);
        void DisplayStoredElements();

        IElementStacksManager GetStorageStacksManager();

        IEnumerable<ISituationOutputNote> GetOutputNotes();

        void SetSlotConsumptions();

        IRecipeSlot GetUnfilledGreedySlot();
        IRecipeSlot GetOngoingSlotBySaveLocationInfoPath(string locationInfo);

        void Retire();
        void SetComplete();
        bool ConsumeMarkedElements(bool withAnim);

        void ShowDestinationsForStack(IElementStack stack);

        void DisplayTimeRemaining(float duration, float timeRemaining);
        void DisplayNoRecipeFound();
    }
}
