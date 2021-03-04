﻿
using SecretHistories.Assets.Scripts.Application.Commands.SituationCommands;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Spheres;

namespace SecretHistories.States
{
    public class UnstartedState : SituationState
    {
        public override StateEnum RehydrationValue => StateEnum.Unstarted;

        public override void Enter(Situation situation)
        {
            situation.CommandQueue.AddCommand(new ResetDominionCommand("VerbThresholdsDominion"));
            situation.CommandQueue.AddCommand(new ResetDominionCommand("RecipeThresholdsDominion"));
            situation.CommandQueue.AddCommand(new ResetDominionCommand("StorageDominion"));
            situation.CommandQueue.AddCommand(new ResetDominionCommand("OutputDominion"));

            situation.Reset();

            var verbThresholdsCommand= new PopulateDominionCommand("VerbThresholdsDominion",situation.Verb.Thresholds);
            situation.CommandQueue.AddCommand(verbThresholdsCommand);
        }

        public override void Exit(Situation situation)
        {
            }

        public override bool IsActiveInThisState(Sphere s)
        {
            if (s.SphereCategory != SphereCategory.Threshold)

                return false;

            return s.GoverningSphereSpec.IsActiveInState(StateEnum.Unstarted);

        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck,Situation s)
        {
            //return true if:
            //Situation is Unstarted; verb matches; and the recipe is either craftable or hintable
            if ((recipeToCheck.Craftable || recipeToCheck.HintOnly) && recipeToCheck.ActionId == s.Verb.Id)
                return true;

            return false;
        }

        public override bool AllowDuplicateVerbIfVerbSpontaneous => false;


        public override void Continue (Situation situation)
        {
        
        }
    }
}