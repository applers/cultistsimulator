﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.NullObjects;

namespace Assets.Scripts.Application.Entities.NullEntities
{
    public class CompendiumNullObjectStore
    {
        Dictionary<Type,object> NullObjectsForEntities=new Dictionary<Type, object>();

        public CompendiumNullObjectStore()
        {
            NullObjectsForEntities.Add(typeof(Element),NullElement.Create());
            NullObjectsForEntities.Add(typeof(Ending),Ending.NotEnded());
            NullObjectsForEntities.Add(typeof(Legacy),NullLegacy.Create());
            NullObjectsForEntities.Add(typeof(Verb),NullVerb.Create());
            NullObjectsForEntities.Add(typeof(Recipe),NullRecipe.Create());
            NullObjectsForEntities.Add(typeof(Portal),NullPortal.Create());

        }

        public object GetNullObjectForType(Type forType,string entityId)
        {
            if (NullObjectsForEntities.ContainsKey(forType))
                return NullObjectsForEntities[forType];

            NoonUtility.Log($"Can't find entity id '{entityId}' of type {forType}, and can't find an appropriate NullObject to return in its place - returning plain old null.");
            return null;
        }

    }
}
