﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using Assets.Scripts.Application.Interfaces;
using Newtonsoft.Json;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Tokens;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class TokenCreationCommand:IEncaustment
    {
        public TokenLocation Location { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public ITokenPayloadCreationCommand Payload { get; set; }
        public bool Defunct { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public AbstractTokenState CurrentState { get; set; }
        private Token _sourceToken;
        private TokenLocation _destination;
        private float _travelDuration;

        public TokenCreationCommand()
        {
            Location=TokenLocation.Default(FucinePath.Root()); // we expect the sphere to be overwritten in Execute.The SpherePath bit of Location here is redundant
        }

        public TokenCreationCommand(ITokenPayloadCreationCommand payload,TokenLocation location)
        {
            Payload = payload;
            Location = location; // we expect the sphere to be overwritten in Execute.The SpherePath bit of Location here is redundant

        }

        public TokenCreationCommand(ElementStack elementStack, TokenLocation location)
        {
            var elementStackEncaustery = new Encaustery<ElementStackCreationCommand>();
            Payload = elementStackEncaustery.Encaust(elementStack);

        }

        public TokenCreationCommand WithSourceToken(Token sourceToken)
        {
            _sourceToken = sourceToken;
            return this;
        }

        public TokenCreationCommand WithDestination(TokenLocation destination,float travelDuration)
        {
            _destination = destination;
            _travelDuration = travelDuration;
            return this;
        }

        public TokenCreationCommand WithElementStack(string elementId,int quantity)
        {
            Payload = new ElementStackCreationCommand(elementId, quantity);
            return this;
        }


        public Token Execute(Context context,Sphere sphere)
        {

            var payloadForToken = Payload.Execute(context); //do this first, so we can decide not to instantiate the token if the payload turns out to be invalid (eg, an attempt to create a unique verb twice)
            if(!payloadForToken.IsValid())
                return NullToken.Create();

            Token newToken;

            Sphere sphereForThisToken;

            if (Location != null && sphere == null && Location.HasValidSpherePath())
            {
                //if we have a valid location sphere path and no direct reference to a valid sphere, use the location sphere path.
                sphereForThisToken = Watchman.Get<HornedAxe>().GetSphereByAbsolutePath(Location.AtSpherePath);
            }

            //if we have a valid location that is not the same as the sphere in which this is being executed, execute at the location instead
            else if (Location != null && Location.HasValidSpherePath() 
                     && !Location.AtSpherePath.IsRoot() //what? why? Because back when the pathing system was embryonic, 
                     //root meant 'ugh I dont know what should go here and/or maybe this means current.' So it's often the default for CS when it should actually be current.
                     && !sphere.GetAbsolutePath().Conforms(Location.AtSpherePath))
                sphereForThisToken = Watchman.Get<HornedAxe>().GetSphereByAbsolutePath(Location.AtSpherePath);
            else
                sphereForThisToken = sphere;

            if (!payloadForToken.IsPermanent())
                newToken = InstantiateTokenInSphere(payloadForToken,context, sphereForThisToken);
            else
            {
                //permanent tokens, like terrain features, are already instantiated with the token component already attached.
                //So we don't instantiate the token: we just find the existing token and then populate it with relevant payload data.
                newToken = sphereForThisToken.GetTokens().SingleOrDefault(t => t.PayloadId == payloadForToken.Id);
                if(newToken==null || !newToken.IsValid())
                {
                    NoonUtility.LogWarning($"Couldn't populate a permanent token with payload id {payloadForToken.Id} in {sphereForThisToken.GetAbsolutePath()}");
                }
                else
                  newToken.SetPayload(payloadForToken); //for a permanent token, we're replacing the starter payload with the populated one.
    
            }
       

            payloadForToken.FirstHeartbeat();

            
            if (_sourceToken != null && _destination != null)
            {
                SetTokenTravellingFromSourceToken(newToken,_sourceToken,_destination,_travelDuration);
            }

            else if (_destination != null)
            {
                SetTokenTravellingToDestination(newToken, _destination);
            }

 
            return newToken;
        }



        private Token InstantiateTokenInSphere(ITokenPayload payload, Context context, Sphere sphere)
        {
            //only use the location sphere if for some reason we don't have a valid sphere to accept the token
            if (sphere == null && Location.HasValidSpherePath())
                sphere = Watchman.Get<HornedAxe>().GetSphereByAbsolutePath(Location.AtSpherePath);

            var token = Watchman.Get<PrefabFactory>().CreateLocally<Token>(sphere.GetRectTransform());
            token.TokenRectTransform.anchoredPosition3D = Location.Anchored3DPosition;
            token.SetPayload(payload); //We need to do this before placing the token in the sphere, if we want the correct manifestations to appear on entry to the sphere
            sphere.AcceptToken(token, context);
            return token;
        }

        private void SetTokenTravellingFromSourceToken(Token newToken,Token fromSourceToken, TokenLocation destination, float duration)
        {
            

            var spawnedTravelItinerary = new TokenTravelItinerary(fromSourceToken.TokenRectTransform.anchoredPosition3D,
                    newToken.Sphere.Choreographer.GetClosestFreeLocalPosition(newToken,
                        fromSourceToken.ManifestationRectTransform.anchoredPosition))
                .WithDuration(duration)
                .WithDestinationSpherePath(destination.AtSpherePath)
                .WithScaling(0f, 1f);

            newToken.TravelTo(spawnedTravelItinerary, new Context(Context.ActionSource.JustSpawned));
        }

        private void SetTokenTravellingToDestination(Token newToken, TokenLocation destination)
        {

            var itineraryForNewToken =
                new TokenTravelItinerary(newToken.TokenRectTransform.anchoredPosition3D,
                        newToken.Sphere.Choreographer.GetClosestFreeLocalPosition(newToken, destination.Anchored3DPosition))
                    .WithDuration(_travelDuration).
                    WithDestinationSpherePath(destination.AtSpherePath);
            newToken.TravelTo(itineraryForNewToken, new Context(Context.ActionSource.JustSpawned));

        }
    }
}
