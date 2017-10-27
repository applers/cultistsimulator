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
    public class ElementStackToken : DraggableToken, IElementStack, IGlowableView {

        [SerializeField] Image artwork;
        [SerializeField] Image backArtwork;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] ElementStackBadge stackBadge;
        [SerializeField] TextMeshProUGUI stackCountText;
        [SerializeField] GameObject decayView;
        [SerializeField] TextMeshProUGUI decayCountText;

        [SerializeField] CardEffectRemoveColorAnim cardBurnFX;

        private Element _element;
        private int _quantity;
        private ITokenTransformWrapper currentWrapper;
        private float lifetimeRemaining;
        private bool isFront = true;

        private Coroutine turnCoroutine;
        private Coroutine animCoroutine;

        private ElementStackToken originStack = null; // if it was pulled from a stack, save that stack!

        protected override bool AllowDrag { get { return isFront && turnCoroutine == null; } } // no dragging while not front or busy turning

        protected override void Awake() {
            base.Awake();
            /*
            decayCountText.enableCulling = true;
            stackCountText.enableCulling = true;
            text.enableCulling = true;
            */
        }

        protected override void OnDisable() {
            base.OnDisable();

            // this resets any animation frames so we don't get stuck when deactivating mid-anim
            artwork.overrideSprite = null; 

            // we're turning? Just set us to the garget
            if (turnCoroutine != null) {
                turnCoroutine = null;
                Flip(isFront, true); // instant to set where it wants to go
            }
        }

        public void SetBackface(string backId) {
            Sprite sprite;

            if (string.IsNullOrEmpty(backId))
                sprite = null;
            else
                sprite = ResourcesManager.GetSpriteForCardBack(backId);

            backArtwork.overrideSprite = sprite;
        }

        #region -- Turn Card ------------------------------------------------------------------------------------

        public void FlipToFaceUp(bool instant = false) {
            Flip(true, instant);
        }

        public void FlipToFaceDown(bool instant = false) {
            Flip(false, instant);
        }

        public void Flip(bool state, bool instant = false) {
            if (isFront == state && !instant) // if we're instant, ignore this to allow forcing of pos
                return;

            isFront = state;

            if (gameObject.activeInHierarchy == false || instant) {
                transform.localRotation = GetFrontRotation(isFront);
                return;
            }

            if (turnCoroutine != null)
                StopCoroutine(turnCoroutine);

            turnCoroutine = StartCoroutine(DoTurn());
        }

        Quaternion GetFrontRotation(bool isFront) {
            return Quaternion.Euler(0f, isFront ? 0f : 180f, 0f);
        }

        public bool IsFront() {
            return isFront;
        }

        IEnumerator DoTurn() {
            float time = 0f;
            float targetAngle = isFront ? 0f : 180f;
            float currentAngle = transform.localEulerAngles.y;
            float duration = Mathf.Abs(targetAngle - currentAngle) / 900f;

            while (time < duration) {
                time += Time.deltaTime;
                transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(currentAngle, targetAngle, time / duration), 0f);
                yield return null;
            }

            transform.localRotation = Quaternion.Euler(0f, targetAngle, 0f);
            turnCoroutine = null;
        }

        #endregion

        #region -- Animated Card ------------------------------------------------------------------------------------

        public bool CanAnimate() {
            if (gameObject.activeInHierarchy == false)
                return false; // can not animate if deactivated

            // TODO: Add a check to see if the element has any frames/frame time defined in the first place. 
            // For testing purposes I'm assuming only health is good to go
            if (Id != "health")
                return false;

            return true;
        }

        /// <summary>
        /// Trigger an animation on the card
        /// </summary>
        /// <param name="duration">Determines how long the animation runs. Time is spent equally on all frames</param>
        /// <param name="frameCount">How many frames to show. Default is 1</param>
        /// <param name="frameIndex">At which frame to start. Default is 0</param>
        public void StartArtAnimation() {
            if (!CanAnimate())
                return;

            if (animCoroutine != null)
                StopCoroutine(animCoroutine);

            // TODO: pull data from element itself and use that to drive the values below
            float duration = 0.2f;
            int frameCount = 1;
            int frameIndex = 0;

            animCoroutine = StartCoroutine( DoAnim(duration, frameCount, frameIndex) );
        }

        IEnumerator DoAnim(float duration, int frameCount, int frameIndex) {
            Sprite[] animSprites = new Sprite[frameCount];

            for (int i = 0; i < animSprites.Length; i++) 
                animSprites[i] = ResourcesManager.GetSpriteForElement(Id, frameIndex + i);

            float time = 0f;
            int spriteIndex = -1;
            int lastSpriteIndex = -1;

            while (time < duration) {
                time += Time.deltaTime;
                spriteIndex = (frameCount == 1 ? 0 : Mathf.FloorToInt(time / duration * frameCount));

                if (spriteIndex != lastSpriteIndex) {
                    lastSpriteIndex = spriteIndex;
                    // Ternary operator since the spriteIndex math will sometimes result in the last frame popping out of range, which is fine.
                    artwork.overrideSprite = (spriteIndex < animSprites.Length ? animSprites[spriteIndex] : null);
                }
                yield return null;
            }

            // remove anim 
            artwork.overrideSprite = null;
        }

        #endregion

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

                var token = stackDroppedOn as DraggableToken;

                if (token != null) // make sure the glow is done in case we highlighted this
                    token.ShowGlow(false, true);

                // we're destroying the token so it never throws an onDropped and it's container was not changed, so tell the current container, not the old.
                this.container.TokenDropped(this);
                this.Retire(false);                
            }
        }

        public void SplitAllButNCardsToNewStack(int n) {
            if (Quantity > n) {
                var cardLeftBehind = PrefabFactory.CreateToken<ElementStackToken>(transform.parent);
                cardLeftBehind.Populate(Id, Quantity - n);

                originStack = cardLeftBehind;

                //goes weird when we pick things up from a slot. Do we need to refactor to Accept/Gateway in order to fix?
                SetQuantity(1);

                var gateway = container.GetElementStacksManager();
                gateway.AcceptStack(cardLeftBehind);

                // Gateway accepting stack puts it to pos Vector3.zero, so this is last
                cardLeftBehind.transform.position = transform.position;
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
			if (stackBadge.IsHovering() == false) 
            	SplitAllButNCardsToNewStack(1);

            Registry.Retrieve<TabletopManager>().ShowDestinationsForStack(this);

            base.StartDrag(eventData);
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

        // Hover

        public virtual void OnPointerEnter(PointerEventData eventData) {
            ShowHoverGlow(true);
        }



    }
}
