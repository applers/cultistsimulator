﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.Core.States
{
    public class RequiresExecutionState : SituationState
    {

        public override bool Extinct => false;

        protected override void Enter(Situation situation)
        {

            situation.ExecuteCurrentRecipe();
        }

        protected override void Exit(Situation situation)
        {
            throw new NotImplementedException();
        }

        public override bool IsActiveInThisState(Sphere s)
        {
            throw new NotImplementedException();
        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {
            //Situation is RequiringExecution, and recipe is in Linked list of current recipe.  ActionId doesn't need to match.
            if (s.CurrentPrimaryRecipe.Linked.Exists(r => r.Id == recipeToCheck.Id))
                return true;

            return false;
        }

        protected override SituationState GetNextState(Situation situation)
        {
            if (situation.CurrentInterruptInputs.Contains(SituationInterruptInput.Halt))
            {
                situation.CurrentInterruptInputs.Remove(SituationInterruptInput.Halt);
                return new HaltingState();
            }

            var tc = Registry.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(situation.GetAspectsAvailableToSituation(true));

            var rc = new RecipeConductor(aspectsInContext, Registry.Get<Character>());

            var linkedRecipe = rc.GetLinkedRecipe(situation.CurrentPrimaryRecipe);

            if (linkedRecipe != null)
            {
                //send the completion description before we move on
                INotification notification = new Notification(situation.CurrentPrimaryRecipe.Label, situation.CurrentPrimaryRecipe.Description);
                situation.SendNotificationToSubscribers(notification);

                //I think this code duplicates ActivateRecipe, below
                situation.CurrentPrimaryRecipe = linkedRecipe;
                situation.TimeRemaining = situation.CurrentPrimaryRecipe.Warmup;
                if (situation.TimeRemaining > 0) //don't play a sound if we loop through multiple linked ones
                {
                    if (situation.CurrentPrimaryRecipe.SignalImportantLoop)
                        SoundManager.PlaySfx("SituationLoopImportant");
                    else
                        SoundManager.PlaySfx("SituationLoop");

                }

                return new UnstartedState();
            }
            else
            {
                return new CompleteState();
            }


        }
    }
}