﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.UI;
using SecretHistories.Infrastructure;
using SecretHistories.Interfaces;

namespace Assets.TabletopUi
{
    public class AnchorAndSlot
    {
        public Token Token { get; set; } 
        public Sphere Threshold { get; set; }
    }
}
