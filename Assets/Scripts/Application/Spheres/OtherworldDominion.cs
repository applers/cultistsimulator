﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class OtherworldDominion: AbstractDominion
    {

        [SerializeField] public EgressThreshold EgressSphere;

        [SerializeField] private string EditableIdentifier;

        [SerializeField]
        private bool IsAlwaysVisible;
        
        
        public OtherworldDominion()
        {
            
        }

        public override void Awake()
        {
            Identifier = EditableIdentifier;
            base.Awake();
        }

        public bool MatchesEgress(string egressId)
        
        {
            //Portal identifiers used to be enums, with ToString= eg Wood. Let's be extra forgiving.
            return String.Equals(Identifier, egressId, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool VisibleFor(string egressId)
        { 

            if (MatchesEgress(egressId))
                return true;
            else
                return IsAlwaysVisible;

        }


        public override bool RemoveSphere(string id, SphereRetirementType retirementType)
        {
            var sphereToRemove = GetSphereById(id);
            if (sphereToRemove != null)
            {
                _spheres.Remove(sphereToRemove);
                OnSphereRemoved.Invoke(sphereToRemove);
                sphereToRemove.Retire(retirementType);
                return true;
            }
            else
                return false;
        }




        public override Sphere TryCreateOrRetrieveSphere(SphereSpec spec)
        {
 
            var existingSphere = GetSphereById(spec.Id);
            if (existingSphere != null)
                return existingSphere;

            if (!CanCreateSphere(spec))
            {
                NoonUtility.Log($"Can't create sphere with ID {spec.Id} in dominion {Identifier}; returning NullSphere");
                return NullSphere.Create();
            }

            var newSphere = Watchman.Get<PrefabFactory>().InstantiateSphere(spec, _manifestable);
            newSphere.transform.SetParent(transform, false);
            _spheres.Add(newSphere);
            return newSphere;
        }

      
    }
}
