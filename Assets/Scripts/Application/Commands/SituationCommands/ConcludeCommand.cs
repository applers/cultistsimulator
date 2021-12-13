﻿using System;

using System.Linq;

using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.States;


namespace SecretHistories.Commands.SituationCommands
{
   public class ConcludeCommand: ISituationCommand
   {
       
       public bool IsValidForState(StateEnum forState)
       {
           return forState == StateEnum.Complete;
       }

       public bool IsObsoleteInState(StateEnum forState)
       {
           return false;
           // return forState == StateEnum.Unstarted;
       }

       public bool Execute(Situation situation)
        {
 
            var results = situation.GetElementTokens(SphereCategory.Output);
            foreach (var item in results)
            {
                item.Unshroud(true);
                item.GoAway(new Context(Context.ActionSource.PlayerDumpAll));
            }
            // Only play collect all if there's actually something to collect 
            // Only play collect all if it's not transient - cause that will retire it and play the retire sound
            // Note: If we collect all from the window we also get the default button sound in any case.
            if (results.Any())
                SoundManager.PlaySfx("SituationCollectAll");
            else if (situation.Verb.Spontaneous)
                SoundManager.PlaySfx("SituationTokenRetire");
            else
                SoundManager.PlaySfx("UIButtonClick");
            
            if(situation.Verb.Spontaneous)
                situation.Retire(RetirementVFX.Default);
            else
                situation.TransitionToState(new UnstartedState());

            return true;
        }
    }
}
