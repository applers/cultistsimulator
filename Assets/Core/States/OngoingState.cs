﻿using System;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.Core.States
{
    public class OngoingState : SituationState
    {

        public override bool Extinct => false;

        protected override void Enter(Situation situation)
        {
         situation.CurrentBeginningEffectCommand = new RecipeBeginningEffectCommand(situation.CurrentPrimaryRecipe.Slots, situation.CurrentRecipePrediction?.BurnImage);
        }

        protected override void Exit(Situation situation)
        {
            
        }

        public override bool IsActiveInThisState(Sphere s)
        {
            if (s.SphereCategory == SphereCategory.SituationStorage)
                return true;

            if (s.SphereCategory != SphereCategory.Threshold && s.GoverningSlotSpecification.IsActiveInState(StateEnum.Ongoing))
                return true;

            return false;
        }
        /// <summary>
        /// WARNING: this assumes ShouldAlwaysSucceed, which is greast for prediction but not for execution
        /// </summary>
        /// <param name="currentRecipe"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {

            //Situation is Ongoing. Recipe is in Alt list of current recipe - as Always Succeed and not as Additional. ActionId doesn't need to match.
            if(s.CurrentPrimaryRecipe.Alt.Exists(r => r.Id == recipeToCheck.Id && r.ShouldAlwaysSucceed() && !r.Additional))
                return true;

            return false;

        }

        protected override SituationState GetNextState(Situation situation)
        {
            if(situation.CurrentInterruptInputs.Contains(SituationInterruptInput.Halt))
            {
                situation.CurrentInterruptInputs.Remove(SituationInterruptInput.Halt);
            return new HaltingState();
            }

            if (situation.TimeRemaining <= 0)
                return new RequiresExecutionState();
            else
            {
                situation.TimeRemaining = situation.TimeRemaining - situation.IntervalForLastHeartbeat;
                return this;
            }
            
        }
    }
}