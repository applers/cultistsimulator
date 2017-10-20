﻿using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI.Interfaces;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

// Should inherit from a "TabletopToken" base class same as VerbBox

namespace Assets.CS.TabletopUI
{
    public class ElementStackToken : DraggableToken, IElementStack, IGlowableView
    {

        [SerializeField] Image artwork;
        [SerializeField] Image backArtwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GraphicFader glowImage;
        [SerializeField] GameObject stackBadge;
        [SerializeField] TextMeshProUGUI stackCountText;
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;

        [SerializeField] CardEffectRemoveColorAnim cardBurnFX;

        private Element _element;
        private int _quantity;
        private ITokenTransformWrapper currentWrapper;
        private float lifetimeRemaining;

        private ElementStackToken originStack = null; // if it was pulled from a stack, save that stack!

        public void FlipToFaceUp()
        {
            Flip(true);
        }

        public void FlipToFaceDown()
        {
            Flip(false);
        }
        
        public void Flip(bool toFaceUp)
        {
            throw new NotImplementedException();
        }

        public override string Id
        {
            get { return _element == null ? null : _element.Id; }
        }

        public bool Decays
        {
            get { return _element.Lifetime > 0; }
        }

        public string Label
        {
            get { return _element == null ? null : _element.Label; }
        }

        public int Quantity
        {
            get { return _quantity; }
        }

        public bool Defunct { get; private set; }
        public bool MarkedForConsumption { get; set; }


        public void SetQuantity(int quantity)
        {
            _quantity = quantity;
            if (quantity <= 0)
            {
                Retire(true);
                return;
            }
            DisplayInfo();
        }


        public Dictionary<string, string> GetXTriggers()
        {
            return _element.XTriggers;
        }

        public void ModifyQuantity(int change)
        {
            SetQuantity(_quantity + change);
        }

        public override void ReturnToTabletop(INotification reason) {
            if (originStack != null && originStack.IsOnTabletop()) {
                originStack.MergeIntoStack(this);
                return;
            }

            base.ReturnToTabletop(reason);

            if (lastTablePos != null)
                transform.position = (Vector3)lastTablePos;
            else
                lastTablePos = transform.position;
        }

        public override bool Retire()
        {
            return Retire(true);
        }

        public bool Retire(bool withVFX)
        {
            if (Defunct)
                return false;

            Defunct = true;

            if (withVFX && gameObject.activeInHierarchy)
            {
                var effect = Instantiate<CardEffectRemoveColorAnim>(cardBurnFX) as CardEffectRemoveColorAnim;
                effect.StartAnim(this);
            }
            else
                Destroy(gameObject);

            return true;
        }


        public void Populate(string elementId, int quantity)
        {

            _element = Registry.Retrieve<ICompendium>().GetElementById(elementId);
            try
            {

            SetQuantity(quantity);

            name = "Card_" + elementId;
            if (_element == null)
                return;

            DisplayInfo();
            DisplayIcon();
            ShowGlow(false, false);
            ShowCardDecayTimer(false);
            SetCardDecay(0f);
            lifetimeRemaining = _element.Lifetime;

            }
            catch (Exception e)
            {

                Debug.Log("Couldn't create element with ID " + elementId + " - " + e.Message);
                Retire(false);
            }
        }


        private void DisplayInfo()
		{
			text.text = _element.Label;
			stackBadge.gameObject.SetActive(Quantity > 1);
			stackCountText.text = Quantity.ToString();
        }

        private void DisplayIcon()
        {
            Sprite sprite = ResourcesManager.GetSpriteForElement(_element.Id);
            artwork.sprite = sprite;

            if (sprite == null)
                artwork.color = Color.clear;
            else
                artwork.color = Color.white;
        }

        public IAspectsDictionary GetAspects(bool includeSelf = true)
        {
            if (includeSelf)
                return _element.AspectsIncludingSelf;
            else
                return _element.Aspects;
        }

        public List<SlotSpecification> GetChildSlotSpecifications()
        {
            return _element.ChildSlotSpecifications;
        }


        public bool HasChildSlots()
        {
            return _element.HasChildSlots();
        }

        public Sprite GetSprite()
        {
            return artwork.sprite;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            notifier.ShowElementDetails(_element);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            if (DraggableToken.itemBeingDragged != null)
                DraggableToken.itemBeingDragged.InteractWithTokenDroppedOn(this);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            //remove any suitability glows
            Registry.Retrieve<TabletopManager>().ShowDestinationsForStack(null);
            base.OnEndDrag(eventData);
        }

        public override void InteractWithTokenDroppedOn(IElementStack stackDroppedOn)
        {
            if (stackDroppedOn.Id == this.Id && stackDroppedOn.AllowMerge())
            {
                stackDroppedOn.SetQuantity(stackDroppedOn.Quantity + this.Quantity);
                DraggableToken.resetToStartPos = false;
                SoundManager.PlaySfx("CardPutOnStack");
                this.Retire(false);
            }
        }

        public void SplitAllButNCardsToNewStack(int n) {
            if (Quantity > n) {
                var cardLeftBehind = PrefabFactory.CreateToken<ElementStackToken>(transform.parent);

                cardLeftBehind.Populate(Id, Quantity - n);
                //goes weird when we pick things up from a slot. Do we need to refactor to Accept/Gateway in order to fix?
                SetQuantity(1);
                cardLeftBehind.transform.position = transform.position;
                var gateway = container.GetElementStacksManager();

                originStack = cardLeftBehind;
                gateway.AcceptStack(cardLeftBehind);
            }
        }

        public bool IsOnTabletop() {
            return transform.parent.GetComponent<TabletopContainer>() != null;
        }

        public void MergeIntoStack(ElementStackToken merge) {
            SetQuantity(Quantity + merge.Quantity);
            merge.Retire(false);
        }

        public bool AllowMerge()
        {
            return container.AllowStackMerge && !Decays;
        }

        protected override void StartDrag(PointerEventData eventData)
        {
			// A bit hacky, but it works: DID NOT start dragging from badge? Split cards 
			if (eventData.hovered.Contains(stackBadge) == false) 
            	SplitAllButNCardsToNewStack(1);

            Registry.Retrieve<TabletopManager>().ShowDestinationsForStack(this);


            base.StartDrag(eventData);
        }
        
        // IGlowableView implementation

        public void SetGlowColor(UIStyle.TokenGlowColor colorType) {
            SetGlowColor(UIStyle.GetGlowColor(colorType));
        }

        public void SetGlowColor(Color color) {
            glowImage.SetColor(color);
        }

        public void ShowGlow(bool glowState, bool instant) {
            if (glowState)
                glowImage.Show(instant);
            else
                glowImage.Hide(instant);                     
        }


        public void Decay(float interval)
        {
            if (!Decays)
                return;
            lifetimeRemaining = lifetimeRemaining - interval;

            if (lifetimeRemaining < 0)
                Retire(true);

            if(lifetimeRemaining<_element.Lifetime/2)
            { 
                ShowCardDecayTimer(true);
                SetCardDecayTime(lifetimeRemaining);
            }

            SetCardDecay(1-lifetimeRemaining/_element.Lifetime);
           
        }
        // Card Decay Timer
        public void ShowCardDecayTimer(bool showTimer) {
            decayView.gameObject.SetActive(showTimer);
        }

        public void SetCardDecayTime(float timeRemaining) {
           
            decayCountText.text = timeRemaining.ToString("0.0") + "s";
        }

        public void SetCardDecay(float percentage) {
            percentage = Mathf.Clamp01(percentage);
            artwork.color = new Color(1f - percentage, 1f - percentage, 1f - percentage, 1.5f - percentage);
        }


        IEnumerator DoMove(Vector3 targetPos, float duration) {
            float time = 0f;
            Vector3 startPos = transform.position;

            while (time < duration) {
                time += Time.deltaTime;
                Vector3.Lerp(startPos, targetPos, Easing.Back.Out(time / duration));
                yield return null;
            }

            transform.position = targetPos;
        }

    }
}
