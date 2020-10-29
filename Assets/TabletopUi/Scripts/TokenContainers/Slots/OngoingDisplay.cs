﻿#pragma warning disable 0649
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine.Events;

namespace Assets.CS.TabletopUI {
    public class OngoingDisplay:MonoBehaviour {

        [SerializeField] Transform slotHolder; 
        [SerializeField] Image countdownBar;
		[SerializeField] TextMeshProUGUI countdownText;
        [SerializeField] LayoutGroup storedCardsLayout;
        public CanvasGroupFader canvasGroupFader;

        [SerializeField] DeckEffectView[] deckEffectViews; 
        HashSet<RecipeSlot> ongoingSlots=new HashSet<RecipeSlot>();



        public void UpdateForRecipe(Recipe recipe,OnContainerAddedEvent onContainerAdded,OnContainerRemovedEvent onContainerRemoved,string situationPath)
        {
            foreach (var os in ongoingSlots)
            {
                onContainerRemoved.Invoke(os);
                os.Retire();
            }

            ongoingSlots.Clear();

            foreach (var spec in recipe.Slots)
            {
                var newSlot = Registry.Get<PrefabFactory>().Create<RecipeSlot>();
                newSlot.name = spec.UniqueId;

                newSlot.Initialise(spec,situationPath);
              //slot.onCardDropped += RespondToStackAdded; //trialling removing these and running it through new event system
              //slot.onCardRemoved += RespondToStackRemoved;
              
              //UGH:
                  var slotTransform = newSlot.SlotLabel.GetComponent<RectTransform>();
                  var originalSize = slotTransform.sizeDelta;
                  slotTransform.sizeDelta = new Vector2(originalSize.x * 1.5f, originalSize.y * 0.75f);
  
                newSlot.transform.position = slotHolder.position; //won't work if we ever have more than one
                ongoingSlots.Add(newSlot);
                onContainerAdded.Invoke(newSlot);

            }

            ShowDeckEffects(recipe.DeckEffects);
            
        }


        public void UpdateTime(float duration, float timeRemaining, EndingFlavour forEndingFlavour)
		{
            Color barColor = UIStyle.GetColorForCountdownBar(forEndingFlavour, timeRemaining);

            countdownBar.color = barColor;
            countdownBar.fillAmount = Mathf.Lerp(0.055f, 0.945f, 1f - (timeRemaining / duration));
            countdownText.color = barColor;
			countdownText.text = Registry.Get<ILocStringProvider>().GetTimeStringForCurrentLanguage( timeRemaining );
            countdownText.richText = true;
        }

        public void ShowStoredAspects(IEnumerable<ElementStackToken> stacks) {
            int i = 0;

            var aspectFrames = storedCardsLayout.GetComponentsInChildren<ElementFrame>();
            ElementFrame frame;
            Element element;

            foreach (var stack in stacks) {
                element = Registry.Get<ICompendium>().GetEntityById<Element>(stack.EntityId);

                if(!element.IsHidden)
                { 
                for (int q = 0; q < stack.Quantity; q++) {
                    if (i < aspectFrames.Length)
                        frame = aspectFrames[i];
                    else
                        frame = Registry.Get<PrefabFactory>().CreateLocally<ElementFrame>(storedCardsLayout.transform);

                    frame.PopulateDisplay(element,1, stack as ElementStackToken);
                    frame.gameObject.SetActive(true);
                    i++;
                }
                }
            }

            while (i < aspectFrames.Length) {
                aspectFrames[i].gameObject.SetActive(false);

                i++;
            }
        }

        public void ShowDeckEffects(Dictionary<string, int> deckEffects) {
            if(deckEffects.Count>deckEffectViews.Length)
                NoonUtility.LogWarning($"{deckEffects.Count} deck effects to show in OngoingDisplay, but only {deckEffectViews.Length} slots.");

            int i = 0;
            foreach(var dev in deckEffectViews)
                dev.gameObject.SetActive(false);


            // Populate those we need
            foreach (var item in deckEffects) {
                var deckSpec = Registry.Get<ICompendium>().GetEntityById<DeckSpec>(item.Key);
                deckEffectViews[i].PopulateDisplay(deckSpec, item.Value);
                deckEffectViews[i].gameObject.SetActive(true);
                i++;
            }


        }

    }
}