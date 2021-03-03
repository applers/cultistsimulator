﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands;
using SecretHistories.Commands;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Entities.NullEntities
{
    [IsEncaustableClass(typeof(RootPopulationCommand))]
    public sealed class FucineRoot: IHasAspects,IEncaustable
    {
        private const string IIKEY = "II";
        static FucineRoot _instance=new FucineRoot();

        static FucineRoot()
        {
            //Jon Skeet says do this because
            // "Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit"
        }

        [DontEncaust]
        public string Id => FucinePath.ROOT.ToString();

        private ITokenPayload _payload;

        private readonly List<Sphere> _spheres=new List<Sphere>();
        private Dictionary<string, int> _mutations= new Dictionary<string, int>();

        [Encaust]
        public List<Sphere> Spheres => new List<Sphere>(_spheres);

        [Encaust]
        public Dictionary<string, int> Mutations
        {
            get => _mutations;
            set => _mutations = value;
        }

        [DontEncaust]
        public Token Token
        {
            get
            {
                return NullToken.Create();
            }
        }

        public void SetMutation(string aspectId, int value, bool additive)
        {
            if (_mutations.ContainsKey(aspectId))
            {
                if (additive)
                    _mutations[aspectId] += value;
                else
                    _mutations[aspectId] = value;

                if (_mutations[aspectId] == 0)
                    _mutations.Remove(aspectId);
            }
            else if (value != 0)
            {
                _mutations.Add(aspectId, value);
            }
        }

        public string GetSignature()
        {
            return FucinePath.ROOT.ToString();
        }
        public Sphere GetEnRouteSphere()
        {
            var defaultSphere=Watchman.Get<HornedAxe>().GetDefaultSphere();
            return Watchman.Get<HornedAxe>().GetSphereByPath(defaultSphere.GoverningSphereSpec.EnRouteSpherePath);
        }

        public Sphere GetWindowsSphere()
        {
            var defaultSphere = Watchman.Get<HornedAxe>().GetDefaultSphere();
            return Watchman.Get<HornedAxe>().GetSphereByPath(defaultSphere.GoverningSphereSpec.WindowsSpherePath);
        }


        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public static FucineRoot Get()
        {
            return _instance;
        }

        public static void Reset()
        {
            _instance=new FucineRoot();
        }

        public FucinePath GetAbsolutePath()
        {
            return FucinePath.Root();
        }


        public Sphere GetSphereById(string id)
        {
            return _spheres.SingleOrDefault(s => s.Id == id);
        }

        public void AttachSphere(Sphere sphere)
        {
            if(!_spheres.Contains(sphere))
            {
                _spheres.Add(sphere);
                sphere.SetContainer(this);
            }
        }

        public void DetachSphere(Sphere c)
        {
            _spheres.Remove(c);
        }

        public int IncrementedIdentity()
        {
            int iiMutationValue;

            if (_mutations.ContainsKey(IIKEY))
                iiMutationValue = (_mutations[IIKEY]);
            else
                iiMutationValue = 0;

            SetMutation(IIKEY,iiMutationValue+1,false);
            return iiMutationValue;
        }


    }
}
