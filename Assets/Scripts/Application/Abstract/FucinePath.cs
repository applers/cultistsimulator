﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace SecretHistories.Interfaces
{
    public abstract class FucinePath
    {
        public abstract SituationPath GetBaseSituationPath();
    }
}