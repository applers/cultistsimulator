﻿using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public class Limbo : MonoBehaviour,ITokenContainer {
        private ElementStacksManager _elementStacksManager;

        public void Start()
        {
            _elementStacksManager=new ElementStacksManager(new TokenTransformWrapper(transform),"Limbo");
        }

        public void TokenPickedUp(DraggableToken draggableToken)
        {
            //do nothing right now
        }

        public void TokenDropped(DraggableToken draggableToken)
        {
            //do nothing right now
        }

        public void TryMoveAsideFor(DraggableToken potentialUsurper, DraggableToken incumbent, out bool incumbentMoved)
        {
            //do nothing, ever
            incumbentMoved = false;
        }

        public bool AllowDrag { get; private set; }
        public bool AllowStackMerge { get; private set; }
        public ElementStacksManager GetElementStacksManager()
        {
            return _elementStacksManager;
        }

        public string GetSaveLocationInfoForDraggable(DraggableToken draggable)
        {
            return "limbo";
        }
    }
}

