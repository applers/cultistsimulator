﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Core;
using SecretHistories.Constants;
using SecretHistories.Constants.Events;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEngine;

namespace SecretHistories.Entities {


    [Immanence(typeof(SphereCatalogue))]
    public class SphereCatalogue:ISphereEventSubscriber {

        public bool EnableAspectCaching = true;
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();
        private readonly HashSet<ISphereCatalogueEventSubscriber> _subscribers = new HashSet<ISphereCatalogueEventSubscriber>();
        private AspectsDictionary _tabletopAspects = null;
        private AspectsDictionary _allAspectsExtant = null;
        private bool _tabletopAspectsDirty = true;
        private bool _allAspectsExtantDirty = true;

        public const string EN_ROUTE_PATH = "enroute";
        
        public const string STORAGE_PATH = "storage";


        public FucinePath GetDefaultWorldSpherePath()
        {
            var dictum = Watchman.Get<Compendium>().GetSingleEntity<Dictum>();
            var spherePath = new FucinePath(dictum.DefaultWorldSpherePath);
            return spherePath;
        }

        public Sphere GetDefaultWorldSphere()
        {
            var defaultWorldSphere = GetSphereByPath(GetDefaultWorldSpherePath());
            return defaultWorldSphere;
        }

        public FucinePath GetDefaultEnRouteSpherePath()
        {
            var dictum = Watchman.Get<Compendium>().GetSingleEntity<Dictum>();
            var spherePath = new FucinePath(dictum.DefaultEnRouteSpherePath);
            return spherePath;
        }

        
        public Sphere GetDefaultEnRouteSphere()
        {
            var defaultEnRouteSphere = GetSphereByPath(GetDefaultEnRouteSpherePath());
            return defaultEnRouteSphere;
        }

        public HashSet<Sphere> GetSpheres() {
            return new HashSet<Sphere>(_spheres);
        }

        public IEnumerable<Sphere> GetSpheresOfCategory(SphereCategory category)
        {
            return _spheres.Where(c => c.SphereCategory == category);
        }


        public void RegisterSphere(Sphere sphere) {
            
            _spheres.Add(sphere);
        }

        public void DeregisterSphere(Sphere sphere) {
            
            _spheres.Remove(sphere);
        }


        public void Reset()
        {
            foreach(var c in new List<Sphere>(_spheres))
                _spheres.RemoveWhere(tc => !tc.PersistBetweenScenes);
            
            _subscribers.Clear();
        }


        public void Subscribe(ISphereCatalogueEventSubscriber subscriber) {
            _subscribers.Add(subscriber);
         }

        public void Unsubscribe(ISphereCatalogueEventSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }


        public Sphere GetSphereByPath(FucinePath getsphereForPath, FucinePath relativeTo)
        {
            throw new NotImplementedException();
        }

        public Sphere GetSphereByPath(FucinePath spherePath)
        {
            if(!spherePath.IsAbsolute())
                throw new ApplicationException($"trying to find a sphere with sphere path {spherePath.ToString()}, but that's not an absolute path, and no context was provided");
            try
            {
                var specifiedSphere = _spheres.SingleOrDefault(c => c.GetPath() == spherePath);
                if (specifiedSphere == null)
                {
                    if (spherePath == GetDefaultWorldSpherePath())
                        return Watchman.Get<Limbo>(); //we can't find the default world sphere, so no point getting stuck in an infinite loop - just return limbo.
                    else
                        return GetDefaultWorldSphere(); //no longer limbo; let's let people recover things
                }
                
                return specifiedSphere;

            }
            catch (Exception e)
            {
                NoonUtility.LogWarning($"Error retrieving container with path {spherePath}: {e.Message}");
                return Watchman.Get<Limbo>();
            }

        }


        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            NotifyAspectsDirty();

            foreach(var s in new List<ISphereCatalogueEventSubscriber>(_subscribers))
                if(s.Equals(null))
                    Unsubscribe(s);
                else
                    s.NotifyTokensChanged(args);
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            foreach (var s in new List<ISphereCatalogueEventSubscriber>(_subscribers))

                if (s.Equals(null))
                    Unsubscribe(s);
                else
                    s.OnTokenInteraction(args);
        }


        public AspectsInContext GetAspectsInContext(AspectsDictionary aspectsInSituation)
        {
            if (!EnableAspectCaching)
            {
                _tabletopAspectsDirty = true;
                _allAspectsExtantDirty = true;
            }

            if (_tabletopAspectsDirty)
            {
                if (_tabletopAspects == null)
                    _tabletopAspects = new AspectsDictionary();
                else
                    _tabletopAspects.Clear();

                List<Token> tabletopStacks=new List<Token>();


                var tabletopContainers =
                    _spheres.Where(tc => tc.SphereCategory == SphereCategory.World);

                foreach(var tc in tabletopContainers)
                    tabletopStacks.AddRange(tc.GetElementTokens());

                
                foreach (var tabletopStack in tabletopStacks)
                {
                    AspectsDictionary stackAspects = tabletopStack.GetAspects();
                    AspectsDictionary multipliedAspects = new AspectsDictionary();
                    //If we just count aspects, a stack of 10 cards only counts them once. I *think* this is the only place we need to worry about this rn,
                    //but bear it in mind in case there's ever a similar issue inside situations <--there is! if multiple cards are output, they stack.
                    //However! To complicate matters, if we're counting elements rather than aspects, there is already code in the stack to multiply aspect * quality, and we don't want to multiply it twice
                    foreach (var aspect in stackAspects)
                    {

                        if (aspect.Key == tabletopStack.Payload.Id)
                            multipliedAspects.Add(aspect.Key, aspect.Value);
                        else
                            multipliedAspects.Add(aspect.Key, aspect.Value * tabletopStack.Quantity);
                    }
                    _tabletopAspects.CombineAspects(multipliedAspects);
                }


                if (EnableAspectCaching)
                {
                    _tabletopAspectsDirty = false;		// If left dirty the aspects will recalc every frame
                    _allAspectsExtantDirty = true;      // Force the aspects below to recalc
                }
            }

            if (_allAspectsExtantDirty)
            {
                if (_allAspectsExtant == null)
                    _allAspectsExtant = new AspectsDictionary();
                else
                    _allAspectsExtant.Clear();

                var allSituations = Watchman.Get<SituationsCatalogue>();
                foreach (var s in allSituations.GetRegisteredSituations())
                {
                    var stacksInSituation = s.GetElementTokensInSituation();
                    foreach (var situationStack in stacksInSituation)
                    {
                        AspectsDictionary stackAspects = situationStack.GetAspects();
                        AspectsDictionary multipliedAspects = new AspectsDictionary();
                        //See notes above. We need to multiply aspects to take account of stack quantities here too.
                        foreach (var aspect in stackAspects)
                        {

                            if (aspect.Key == situationStack.Payload.Id)
                                multipliedAspects.Add(aspect.Key, aspect.Value);
                            else
                                multipliedAspects.Add(aspect.Key, aspect.Value * situationStack.Quantity);
                        }
                        _allAspectsExtant.CombineAspects(multipliedAspects);
                    }

                }
                _allAspectsExtant.CombineAspects(_tabletopAspects);

                if (EnableAspectCaching)
                    _allAspectsExtantDirty = false;     // If left dirty the aspects will recalc every frame
            }

            AspectsInContext aspectsInContext = new AspectsInContext(aspectsInSituation, _tabletopAspects, _allAspectsExtant);

            return aspectsInContext;

        }

        public int PurgeElement(string elementId, int maxToPurge)
        {
            var compendium = Watchman.Get<Compendium>();

            Element elementToPurge = compendium.GetEntityById<Element>(elementId);

            var worldSpheres = GetSpheresOfCategory(SphereCategory.World);
            foreach (var worldSphere in worldSpheres)
            {
                if (maxToPurge <= 0)
                    return maxToPurge;

                maxToPurge -= worldSphere.TryPurgeStacks(elementToPurge, maxToPurge);

            }

   

            var situationsCatalogue = Watchman.Get<SituationsCatalogue>();
            foreach (var s in situationsCatalogue.GetRegisteredSituations())
            {
                if (maxToPurge <= 0)
                    return maxToPurge;
           
                maxToPurge -= s.TryPurgeStacks(elementToPurge, maxToPurge);

            }

            return maxToPurge;
        }

        public void NotifyAspectsDirty()
        {
            _tabletopAspectsDirty = true;
        }


    }
}
