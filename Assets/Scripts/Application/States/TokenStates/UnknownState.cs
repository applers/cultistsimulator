﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;

namespace SecretHistories.States
{
    public class UnknownState: AbstractTokenState
    {
        public override bool Docked(Token token)
        {
            return false;
        }

        public override bool InPlayerDrivenMotion(Token token)
        {
            return false;

        }

        public override bool InSystemDrivenMotion(Token token)
        {
            return false;

        }

        public override bool CanDecay(Token token)
        {
            return false;

        }
    }
}
