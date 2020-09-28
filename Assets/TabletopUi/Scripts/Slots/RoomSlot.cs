﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Slots
{
    public class RoomSlot: MonoBehaviour, IRecipeSlot
    {
        public IElementStack GetElementStackInSlot()
        {
            throw new NotImplementedException();
        }

        public DraggableToken GetTokenInSlot()
        {
            throw new NotImplementedException();
        }

        public SlotMatchForAspects GetSlotMatchForStack(IElementStack stack)
        {
            throw new NotImplementedException();
        }

        public SlotSpecification GoverningSlotSpecification { get; set; }
        public void AcceptStack(IElementStack s, Context context)
        {
            throw new NotImplementedException();
        }

        public string AnimationTag { get; set; }
        public IRecipeSlot ParentSlot { get; set; }
        public string SaveLocationInfoPath { get; }
        public bool Defunct { get; set; }
        public bool Retire()
        {
            throw new NotImplementedException();
        }

        public bool IsPrimarySlot()
        {
            throw new NotImplementedException();
        }
    }
}
