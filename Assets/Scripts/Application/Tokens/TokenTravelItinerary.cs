using System;
using System.Numerics;
using SecretHistories.Entities;
using SecretHistories.States.TokenStates;
using SecretHistories.Constants;

using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace SecretHistories.UI
{
    public class TokenTravelItinerary
    {
        public Sphere EnRouteSphere { get; set; }
        public Sphere DestinationSphere { get; set; }
        public float Duration { get; set; }
        public Vector3 Anchored3DStartPosition { get; set; }
        public Vector3 Anchored3DEndPosition { get; set; }
        public float StartScale { get; set; }
        public float EndScale { get; set; }
        //startscale   = 1f, float endScale = 1f)
        private const float DefaultStartScale = 1f;
        private const float DefaultEndScale = 1f;



        public TokenTravelItinerary(TokenLocation startLocation, TokenLocation endLocation)
        {
            Anchored3DStartPosition = startLocation.Anchored3DPosition;
            Anchored3DEndPosition = endLocation.Anchored3DPosition;
            EnRouteSphere = Registry.Get<SphereCatalogue>().GetDefaultEnRouteSphere();
            DestinationSphere = Registry.Get<SphereCatalogue>().GetSphereByPath(endLocation.AtSpherePath);
            StartScale = DefaultStartScale;
            EndScale = DefaultEndScale;
        }

        public TokenTravelItinerary(Vector3 anchored3DStartPosition, Vector3 anchored3DEndPosition)
        {
            //the most basic itinerary: don't change sphere, move in current sphere from point to point,s et default duration based on distance, keep current scale
            Anchored3DStartPosition = anchored3DStartPosition;
            Anchored3DEndPosition = anchored3DEndPosition;

            EnRouteSphere = Registry.Get<SphereCatalogue>().GetDefaultEnRouteSphere();
            DestinationSphere = Registry.Get<SphereCatalogue>().GetDefaultWorldSphere();
            StartScale = DefaultStartScale;
            EndScale = DefaultEndScale;
        }


        public void Depart(Token tokenToSend)
        {
            tokenToSend.Unshroud(true);
            tokenToSend.SetState(new TravellingState());

            var tokenAnimation = tokenToSend.gameObject.AddComponent<TokenTravelAnimation>();
            tokenAnimation.OnTokenArrival += Arrive;


            //this will cause hilarity if it's applied to a world sphere rather than a threshold
            //future AK: you'll need a smart way to apply this differently for non-CS-recipe-slot situations
            DestinationSphere.AddBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.InboundTravellingStack));

            
            //We convert to world positions before sending, because we'll be animating through an EnRouteSphere to a DestinationSphere,
            //and the local positions in those are unlikely to match.
            Vector3 startPositioninWorldSpace = tokenToSend.Sphere.GetRectTransform().TransformPoint(Anchored3DStartPosition);
            Vector3 endPositionInWorldSpace = DestinationSphere.GetRectTransform().TransformPoint(Anchored3DEndPosition);
            
            tokenAnimation.SetPositions(startPositioninWorldSpace, endPositionInWorldSpace);
            tokenAnimation.SetScaling(StartScale,EndScale,1f); //1f was the originally set default. I'm not clear atm about the difference between Duration and ScaleDuration 
                                                               //is it if scaling ends before travel duration?
//set a default duraation if we don't have a valid one
           if (Duration <= 0)
           {
               float distance = Vector3.Distance(Anchored3DStartPosition, Anchored3DEndPosition);
               Duration = Mathf.Max(0.3f, distance * 0.001f);
           }
            tokenAnimation.Begin(tokenToSend, Duration);
        }

        public void Arrive(Token token)
        {
            try
            {
                if (DestinationSphere.Equals(null) || DestinationSphere.Defunct)
                    TravelFailed(token);
                else
                {
                    token.SetState(new TravelledToSphere());
                    // Assign element to new slot
                    DestinationSphere.AcceptToken(token, new Context(Context.ActionSource.TravelArrived));
                }
                DestinationSphere.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                    BlockReason.InboundTravellingStack));
            }
            catch(Exception e)
            {
                NoonUtility.LogException(e); 
                TravelFailed(token);
            }
        }

        private void TravelFailed(Token token)
        {
            DestinationSphere.RemoveBlock(new ContainerBlock(BlockDirection.Inward,
                BlockReason.InboundTravellingStack));

            token.SetState(new TravellingState() );
            token.GoAway(new Context(Context.ActionSource.TravelFailed));

        }

        public static TokenTravelItinerary StayExactlyWhereYouAre(Token token)
        {
           var i=new TokenTravelItinerary(token.TokenRectTransform.anchoredPosition3D, token.TokenRectTransform.anchoredPosition3D);
           i.StartScale = token.TokenRectTransform.localScale.magnitude;
           i.EndScale= token.TokenRectTransform.localScale.magnitude;
           i.EnRouteSphere = token.Sphere;
           i.DestinationSphere = token.Sphere;
           return i;
        }

      

        /// <summary>
        /// if we want to move to another sphere, set the sphere to travel through and the sphere to end in
        /// </summary>
        /// <param name="enRouteSphere"></param>
        /// <param name="destinationSphere"></param>
        public TokenTravelItinerary WithSphereRoute(Sphere enRouteSphere, Sphere destinationSphere)
        {
            EnRouteSphere = enRouteSphere;
            DestinationSphere = destinationSphere;
            return this;
        }

        public TokenTravelItinerary WithDuration(float duration)
        {
            Duration = duration;
            return this;
        }

        public TokenTravelItinerary WithScaling(float startScale, float endScale)
        {
            StartScale = startScale;
            EndScale = endScale;
            return this;
        }


        
    }
}