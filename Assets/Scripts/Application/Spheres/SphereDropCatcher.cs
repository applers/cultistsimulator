﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using SecretHistories.States.TokenStates;
using SecretHistories.Constants;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Spheres
{
    public class SphereDropCatcher: MonoBehaviour,IDropHandler
    {
        public Sphere Sphere;


        public void OnDrop(PointerEventData eventData)
        {
            if (Sphere.GoverningSphereSpec.Greedy) // we're greedy? No interaction.
                return;

            if (Sphere.CurrentlyBlockedFor(BlockDirection.Inward))
                return;

            var token = eventData.pointerDrag.GetComponent<Token>();

            if (token != null)
            {
                if (Sphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag)))
                    token.Stabilise();
            }
              

        }


        public bool TryShowPredictedInteractionIfDropped(Token forToken)
        {
            return Sphere.TryDisplayGhost(forToken);
            
        }
    }
}
