﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationSubscriber
    {
        void SituationBeginning(Recipe withRecipe);
        void SituationOngoing();
        void SituationExecutingRecipe(ISituationEffectCommand situationEffectCommand);
        void SituationComplete();
        void SituationHasBeenReset();
        void Halt();
    }
}
