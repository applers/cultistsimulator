﻿using System;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Constants;

namespace SecretHistories.States
{
    public class CompleteState : SituationState
    {
        public override bool AllowDuplicateVerbIfTransient => true;
        public override void Enter(Situation situation)
        {
            
        }

        public override void Exit(Situation situation)
        {
       
        }

        public override bool IsActiveInThisState(Sphere s)
        {
            if (s.SphereCategory == SphereCategory.Output)
                return true;

            return false;
        }

        public override bool IsValidPredictionForState(Recipe recipeToCheck, Situation s)
        {
            return false;
        }

        public override void Continue(Situation situation)
        {
            situation.CommandQueue.ExecuteCommandsFor(CommandCategory.Output, situation);

        }
    }
}