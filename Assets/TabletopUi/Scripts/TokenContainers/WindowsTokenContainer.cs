﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.TabletopUi.Scripts.TokenContainers
{
    public class WindowsTokenContainer: TokenContainer
    {
        public override ContainerCategory ContainerCategory
        {
            get { return ContainerCategory.World; }
        }
        public override string GetSaveLocationForToken(AbstractToken token)
        {
            throw new NotImplementedException();
        }
    }
}