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

namespace SecretHistories.UI
{
    [IsEmulousEncaustable(typeof(AbstractDominion))]
    public class ShelfDominion: AbstractDominion
    {
     


        public override Sphere TryCreateOrRetrieveSphere(SphereSpec spec)
        {
            var existingSphere = _spheres.SingleOrDefault(s => s.Id == spec.Id);
            return existingSphere;
        }


        public override bool VisibleFor(string state)
        {
            return true;
        }

        public override bool RelevantTo(string state, Type sphereType)
        {
            return true;
        }

        public override bool RemoveSphere(string id,SphereRetirementType retirementType)
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



        public override bool CanCreateSphere(SphereSpec spec)
        {
            return true;
        }
    }
}
