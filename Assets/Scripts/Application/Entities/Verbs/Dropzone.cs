﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Application.Fucine;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Logic;
using SecretHistories.Manifestations;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using UnityEngine;

namespace SecretHistories.Entities.Verbs
{
    [IsEncaustableClass(typeof(DropzoneCreationCommand))]
    public class Dropzone: ITokenPayload
    {
        private Token _token;
        private List<IDominion> _dominions=new List<IDominion>();
#pragma warning disable 67
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
#pragma warning restore 67
        [DontEncaust]
        public string Id { get; private set; }
        [DontEncaust]
        public Token Token
        {
            get
            {
                {
                    if (_token == null)
                        return NullToken.Create();
                    return _token;
                }
            }
        }

 

        [Encaust]
        public int Quantity => 1;
        [DontEncaust]
        public Dictionary<string, int> Mutations { get; }
        [DontEncaust]
        public string Icon => string.Empty;
        [DontEncaust]
        public bool IsOpen => false;

        [DontEncaust]
        public string EntityId => string.Empty;

        [DontEncaust] public FucinePath AbsolutePath => new NullFucinePath();

        [Encaust]
        public List<IDominion> Dominions
        {
            get { return new List<IDominion>(_dominions); }
        }
        [DontEncaust] public string Label => "Dropzone";
        [DontEncaust] public string Description => "Description";
        
        [DontEncaust]
        public string UniquenessGroup => string.Empty;
        [DontEncaust]
        public bool Unique => false;


        private List<Sphere> _spheres { get; set; }


        public Dropzone()
        {
            _spheres = new List<Sphere>();
            Id = "dropzoneclassic";
        }

        public string GetIllumination(string key)
        {
            return string.Empty;
        }

        public void SetIllumination(string key, string value)
        {
            //
        }

        public FucinePath GetAbsolutePath()
        {
            var pathAbove = _token.Sphere.GetAbsolutePath();
            var absolutePath = pathAbove.AppendToken(this.Id);
            return absolutePath;
        }

        public Timeshadow GetTimeshadow()
        {
            return Timeshadow.CreateTimelessShadow();
        }


        public bool RegisterDominion(IDominion dominion)
        {
            if (_dominions.Contains(dominion))
                return false;
            _dominions.Add(dominion);
            return true;
        }

        public Sphere GetSphereById(string id)
        {
            return _spheres.SingleOrDefault(s => s.Id == Id && !s.Defunct);
        }

        public Type GetManifestationType(SphereCategory forSphereCategory)
        {
            return typeof(DropzoneManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.InitialiseVisuals(this);

            //assume we have exactly one dropzone bubble sphere
            var dropzoneSpherePath = _spheres.Single().GetAbsolutePath();
            var tabletopSphere = Watchman.Get<HornedAxe>().GetDefaultSphere();
            tabletopSphere.AddAngel(new TidyAngel(dropzoneSpherePath));
        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public void FirstHeartbeat()
        {
            ExecuteHeartbeat(0f);
        }



        public Sphere GetEnRouteSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.EnRouteSpherePath.IsValid() && !Token.Sphere.GoverningSphereSpec.EnRouteSpherePath.IsEmpty())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.EnRouteSpherePath);

            return Token.Sphere.GetContainer().GetEnRouteSphere();
        }

        public Sphere GetWindowsSphere()
        {
            if (Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsValid() && !Token.Sphere.GoverningSphereSpec.WindowsSpherePath.IsEmpty())
                return Watchman.Get<HornedAxe>().GetSphereByPath(Token.Sphere.GoverningSphereSpec.WindowsSpherePath);

            return Token.Sphere.GetContainer().GetWindowsSphere();
        }

        public void AttachSphere(Sphere sphere)
        {
            if (!_spheres.Contains(sphere))
            {
                _spheres.Add(sphere);
                sphere.SetContainer(this);
            }
        }

        public void DetachSphere(Sphere c)
        {
            _spheres.Remove(c);
        }

        public static Dropzone Create()
        {
            return new Dropzone();

        }


        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public void ExecuteHeartbeat(float interval)
        {
            //
        }

        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public ITokenPayload Decay(float interval)
        {
            return this;
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
            //
        }

        public bool Retire(RetirementVFX vfx)
        {
            return true;
        }

        public void InteractWithIncoming(Token incomingToken)
        {
            //
        }

        public bool ReceiveNote(string label, string description,Context context)
        {
            return false;
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            //
        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            //
        }

        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {//
        }

        public string GetSignature()
        {
            return Id;
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

        public void SetToken(Token token)
        {
            _token = token;
        }

        public void OnTokenMoved(TokenLocation toLocation)
        {
           //
        }
    }
}
