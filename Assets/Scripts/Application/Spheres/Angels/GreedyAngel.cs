﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;


using UnityEngine;
using Random = System.Random;

namespace SecretHistories.Spheres.Angels
{
    public class GreedyAngel:IAngel
    {
        private const int GRAB_QUANTITY_LIMIT = 1;
        private const int BEATS_BETWEEN_ANGELRY = 20; 
        private int _beatsTowardsAngelry = 0;
        private Sphere _thresholdSphereToGrabTo;
        private readonly HashSet<Sphere> _spheresToGrabFrom=new HashSet<Sphere>();

        public void SetThresholdToGrabTo(Sphere thresholdSphereToGrabTo)
        {
            _thresholdSphereToGrabTo = thresholdSphereToGrabTo;
            thresholdSphereToGrabTo.GreedyIcon.SetActive(true);
        }

        public int Authority => 10;

        public void Act(float interval)
        {
            _beatsTowardsAngelry++;

            if (_beatsTowardsAngelry >= BEATS_BETWEEN_ANGELRY)
           
                _beatsTowardsAngelry = 0;

            if (!_thresholdSphereToGrabTo.CurrentlyBlockedFor(BlockDirection.Inward) && _thresholdSphereToGrabTo.GetAllTokens().Count == 0)
                TryGrabStack(_thresholdSphereToGrabTo, interval);
            
        }

        public void SetWatch(Sphere sphere)
        {
            _spheresToGrabFrom.Add(sphere);
        }

        public bool MinisterToEvictedToken(Token token,Context context)
        {
            return false; // if  we want to grab evicted tokens, this would be a good place
        }

        private void TryGrabStack(Sphere destinationThresholdSphere, float interval)
        {
  
            foreach (var sphereToSearch in _spheresToGrabFrom)
            {
                var matchingToken = FindStackForSlotSpecificationInSphere(destinationThresholdSphere.GoverningSphereSpec, sphereToSearch);
                if (matchingToken != null)
                {
                    
                    if (matchingToken.CurrentlyBeingDragged())
                        matchingToken.FinishDrag();

                    if (matchingToken.ElementQuantity > GRAB_QUANTITY_LIMIT)
                        matchingToken.CalveToken(matchingToken.ElementQuantity - GRAB_QUANTITY_LIMIT,
                            new Context(Context.ActionSource.GreedyGrab));

                    var enRouteSphere = Watchman.Get<SphereCatalogue>().GetDefaultEnRouteSphere();

                    var targetPosition = GetTargetPositionForDestinationSphere(destinationThresholdSphere);

                    TokenTravelItinerary itinerary = new TokenTravelItinerary(matchingToken.Location.Anchored3DPosition,
                            targetPosition)
                        .WithScaling(1f,0.35f)
                        .WithSphereRoute(enRouteSphere, destinationThresholdSphere);

                    itinerary.Depart(matchingToken,new Context(Context.ActionSource.GreedyGrab));

                    return;
                }
            }
        }

        private Vector3 GetTargetPositionForDestinationSphere(Sphere destinationThresholdSphere)
        {
            //if we're using this for non-classic-CS-recipe slots, we'll need to rewrite it. We'll also need not to hardcode the final scale
            var targetPath = destinationThresholdSphere.GetPath();
            var targetSituationPath = targetPath.GetBaseSituationPath();
            var targetSituation = Watchman.Get<SituationsCatalogue>().GetSituationByPath(targetSituationPath);
            var targetPosition = targetSituation.GetAnchorLocation().Anchored3DPosition;
            return targetPosition;
        }


        private Token FindStackForSlotSpecificationInSphere(SphereSpec slotSpec, Sphere sphereToSearch)
        {
            var rnd = new Random();
            var tokens = sphereToSearch.GetElementTokens().OrderBy(x => rnd.Next());

            foreach (var token in tokens)
                if (token.CanBePulled() && slotSpec.GetSlotMatchForAspects(token.ElementStack.GetAspects()).MatchType == SlotMatchForAspectsType.Okay)
                    return token;

            return null;
        }

    }
}
