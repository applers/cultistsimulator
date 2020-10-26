﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Enums;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.TokenContainers
{
    public class EnRouteTokenContainer : TokenContainer, IDraggableHolder
    {
        public override ContainerCategory ContainerCategory { get; }

        public TabletopTokenContainer StartingContainer;

        public override string GetSaveLocationForToken(AbstractToken token)
        {
            throw new NotImplementedException();
        }

        public RectTransform RectTransform
        {
            get { return GetComponent<RectTransform>(); }
        }


        public void PrepareElementForSendAnim(ElementStackToken stack, TokenLocation destination) // "this reparents the card so it can animate properly" - okay, let's roll with that for now. But the line below is commented, so do we need it?
        {
            StartingContainer.AcceptStack(stack, new Context(Context.ActionSource.DoubleClickSend)); // this reparents, sets container
            //stack.transform.position = ownerSituation.transform.position;
            stack.Unshroud(true);
        }

        public void PrepareElementForGreedyAnim(ElementStackToken stack, TokenLocation destination)
        {
            StartingContainer.AcceptStack(stack, new Context(Context.ActionSource.GreedySlot)); // "this reparents, sets container" - okay, let's roll with that for now
            stack.transform.position = destination.Position;
            stack.Unshroud(true);
        }

        public void MoveElementToSituationSlot(ElementStackToken stack, TokenLocation destination, TokenContainer destinationSlot, float durationOverride = -1.0f)
        {
            var startPos = stack.rectTransform.anchoredPosition3D;
            var endPos = destination.Position;

            float distance = Vector3.Distance(startPos, endPos);
            float duration = durationOverride > 0.0f ? durationOverride : Mathf.Max(0.3f, distance * 0.001f);

            var stackAnim = stack.gameObject.AddComponent<TokenAnimationToSlot>();
            stackAnim.onElementSlotAnimDone += ElementSendAnimDone;
            stackAnim.SetPositions(startPos, endPos);
            stackAnim.SetScaling(1f, 0.35f);
            stackAnim.SetDestinationSlot(destinationSlot);

            destinationSlot.AddBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.StackEnRouteToContainer));

            stackAnim.StartAnim(duration);
        }

        public void ElementSendAnimDone(ElementStackToken element, TokenLocation destination,TokenContainer destinationSlot)
        {
            try
            {
                if (destinationSlot.Equals(null) || destinationSlot.Defunct)
                    element.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
                else
                    // Assign element to new slot
                    destinationSlot.AcceptStack(element, new Context(Context.ActionSource.AnimEnd));
                // Clear this whether the card arrived successfully or not, otherwise slot is locked for rest of session - CP
                destinationSlot.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                    BlockReason.StackEnRouteToContainer));
            }
            catch
            {
                // If anything goes wrong just dump the card back on the desk
                element.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }
        }

        public void ElementGreedyAnimDone(ElementStackToken element, AnchorAndSlot anchorSlotPair)
        {
            if (anchorSlotPair.Threshold.Equals(null) || anchorSlotPair.Threshold.Defunct)
                return;

            anchorSlotPair.Threshold.AcceptStack(element, new Context(Context.ActionSource.AnimEnd));
            anchorSlotPair.Threshold.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.StackEnRouteToContainer));
        }


    }
}
