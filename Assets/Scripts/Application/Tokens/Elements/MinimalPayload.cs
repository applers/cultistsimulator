﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Logic;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Entities
{
    
    public class MinimalPayload : ITokenPayload
    {
        public string Id { get; protected set; }
        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public Dictionary<string, int> Mutations { get; }
        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
      //
        }

        public string GetSignature()
        {
            return Id;
        }

        public string Label { get; }
        public string Description { get; }
        public int Quantity { get; }
        public string UniquenessGroup { get; }
        public bool Unique { get; }
        public string Icon { get; }
        public string GetIllumination(string key)
        {
            return String.Empty;
        }

        public void SetIllumination(string key, string value)
        {
            //
        }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
        }

        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;


        public MinimalPayload(string id)
        {
            Id = id;
        }
        
        
        public bool IsOpen { get; }
        public FucinePath AbsolutePath { get; }
        public List<IDominion> Dominions { get; }=new List<IDominion>();
        public Type GetManifestationType(SphereCategory sphereCategory)
        {
            return typeof(MinimalManifestation);
        }


        public void InitialiseManifestation(IManifestation manifestation)
        {
          //
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public void ExecuteHeartbeat(float interval)
        {
            throw new NotImplementedException();
        }

        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            throw new NotImplementedException();
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            throw new NotImplementedException();
        }

        public bool Retire(RetirementVFX vfx)
        {
            throw new NotImplementedException("Retiring a minimal payload. should this kill the token?");
        }

        public void InteractWithIncoming(Token incomingToken)
        {
            //
        }

        public bool ReceiveNote(string label, string description, Context context)
        {
            return false;
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            //

        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
            //

        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            //

        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
            //

        }

        public void OpenAt(TokenLocation location)
        {
            //

        }

        public void Close()
        {
            //

        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
            //
        }
    }
}
