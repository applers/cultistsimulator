﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;


public class Character: BaseElementsContainer, IElementsContainer
    {

    private readonly Dictionary<string, int> _elementsInStockpile;

    private string _title;
    private string _firstName;
    private string _lastName;
    public CharacterState State { get; set; }
    private string _endingTriggeredId;

    public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                PublishDetailsChange();
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
            PublishDetailsChange();
        }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
            PublishDetailsChange();
         }

        }

    public string EndingTriggeredId
    { get { return _endingTriggeredId; } }




        public Character()
        {
            _elements=new Dictionary<string, int>();
        _elementsInStockpile=new Dictionary<string, int>();
        _elementsDisplaySubscribers=new List<IElementQuantityDisplay>();
        _detailsSubscribers=new List<ICharacterInfoSubscriber>();
            State = CharacterState.Viable;

        }

        public void SubscribeElementQuantityDisplay(IElementQuantityDisplay elementQuantityDisplay)
        {
        if(!_elementsDisplaySubscribers.Contains(elementQuantityDisplay))
            _elementsDisplaySubscribers.Add(elementQuantityDisplay);
        }

        public void Subscribe(ICharacterInfoSubscriber characterInfoSubscriber)
        {
            if(!_detailsSubscribers.Contains(characterInfoSubscriber))
            _detailsSubscribers.Add(characterInfoSubscriber);
        }




        private void PublishDetailsChange()
        {
            foreach(var d in _detailsSubscribers)
                d.ReceiveUpdate(this);
            
        }

       public override void ModifyElementQuantity(string elementId, int quantity)
        {
            if (!_elements.ContainsKey(elementId))
        { 
                _elements.Add(elementId, quantity);
                PublishElementQuantityUpdate(elementId, GetCurrentElementQuantityInStockpile(elementId), 0);

        }
        else
            {
                if (quantity < (0 - GetCurrentElementQuantityInStockpile(elementId)))
                    //if quantity is negative, and more of it than we have in stockpile
                {
                    //move enough from the workspace back to stockpile to satisfy the consumption
                    int removeFromWorkspace = quantity + GetCurrentElementQuantityInStockpile(elementId);
                        //quantity here is negative
                    ElementIntoStockpile(elementId, -removeFromWorkspace); //move it back into stockpile...
                 _elements[elementId] = _elements[elementId] + quantity; //...and then subtract from total quantity
                //the amount not offset by quantity in stockpile; nb we changed the sign here
                PublishElementQuantityUpdate(elementId, GetCurrentElementQuantityInStockpile(elementId), removeFromWorkspace);
                }
                else
                {

                    _elements[elementId] = _elements[elementId] + quantity;
                    PublishElementQuantityUpdate(elementId, GetCurrentElementQuantityInStockpile(elementId), 0);

                }
            }
        }

   public bool ElementOutOfStockpile(string elementId, int plusQuantity)
   {
       if (GetCurrentElementQuantity(elementId) < plusQuantity)
           return false;
        if(!_elementsInStockpile.ContainsKey(elementId))
            _elementsInStockpile.Add(elementId,plusQuantity);
        else
           _elementsInStockpile[elementId] += plusQuantity;

        PublishElementQuantityUpdate(elementId, GetCurrentElementQuantityInStockpile(elementId),0);
        return true;
   }

    public bool ElementIntoStockpile(string elementId, int minusQuantity)
    {
        if (GetCurrentElementQuantityInWorkspace(elementId) < minusQuantity)
            return false;

        if (!_elementsInStockpile.ContainsKey(elementId))
            _elementsInStockpile.Add(elementId, -minusQuantity);
        else
            _elementsInStockpile[elementId] -= minusQuantity;

        PublishElementQuantityUpdate(elementId, GetCurrentElementQuantityInStockpile(elementId),0);
        return true;
    }

    public int GetCurrentElementQuantityInWorkspace(string elementId)
    {
        return _elementsInStockpile.ContainsKey(elementId) ? _elementsInStockpile[elementId] : 0;
    }

        public int GetCurrentElementQuantityInStockpile(string elementId)
        {
            var instockpile = _elements.ContainsKey(elementId) ? _elements[elementId] : 0;

            if (_elementsInStockpile.ContainsKey(elementId))
                instockpile -= _elementsInStockpile[elementId];

            return instockpile;
        }

        public override void TriggerSpecialEvent(string endingId)
        {
            State=CharacterState.Extinct;
            _endingTriggeredId = endingId;
          PublishDetailsChange();
        }
    }

