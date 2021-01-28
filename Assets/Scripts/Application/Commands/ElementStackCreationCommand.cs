﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Abstract;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SecretHistories.Commands
{
    
    public class ElementStackCreationCommand
    {
        /// <summary>
        /// The element id
        /// </summary>
        public string Id { get; set; }
        public int Quantity { get; set; }
        public Dictionary<string,int> Mutations { get; set; }
        public IlluminateLibrarian IlluminateLibrarian { get; set; }
        public bool Defunct { get; set; }

        public float LifetimeRemaining { get; set; }

        public ElementStackCreationCommand(): this(string.Empty,0)
        {}

        public ElementStackCreationCommand(string elementId, int quantity)
        {
            Id = elementId;
            Quantity = quantity;
           IlluminateLibrarian=new IlluminateLibrarian();
           Mutations=Element.EmptyMutationsDictionary();
        }

        public ElementStack Execute(Context context)
        {
            ElementStack elementStack = null;

            try
            {
                var compendium = Watchman.Get<Compendium>();
                  var  element = compendium.GetEntityById<Element>(Id);

                elementStack = new ElementStack(element, Quantity, LifetimeRemaining, context);
                foreach (var m in Mutations)
                    elementStack.SetMutation(m.Key, m.Value, false);

                elementStack.IlluminateLibrarian = IlluminateLibrarian; }
            catch (Exception e)
            {

                NoonUtility.Log("Couldn't create element with ID " + Id + " - " + e.Message + "(This might be an element that no longer exists being referenced in a save file?)");
                elementStack?.Retire(RetirementVFX.None);
            }

            
            return elementStack;
        }
    }
}
