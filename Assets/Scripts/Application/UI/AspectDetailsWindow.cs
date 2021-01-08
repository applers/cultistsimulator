﻿#pragma warning disable 0649
using System.Collections.Generic;
using SecretHistories.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using SecretHistories.Entities;
using SecretHistories.Enums.UI;

namespace SecretHistories.UI {
    public class AspectDetailsWindow : BaseDetailsWindow {

		[SerializeField] RectTransform tokenDetailsHeight;
        [SerializeField] Vector2 posNoTokenDetails = new Vector2(0f, 0f);
        [SerializeField] private BackgroundAdjusterForText adjuster;

        // These are saved here to make sure we have a ref when we're kicking off the anim
        Element element;
		bool _noTokenDetails;

        public void ShowAspectDetails(Element element, bool noTokenDetails) {
            // Check if we'd show the same, if so: do nothing
            if (this.element == element && gameObject.activeSelf && _noTokenDetails == noTokenDetails)
                return;

           // Debug.Log("Position" + (transform as RectTransform).anchoredPosition);

            this._noTokenDetails = noTokenDetails;
            this.element = element;
            Show();
        }

        protected override void ClearContent() {
            this.element = null;
        }

        protected override void UpdateContent() {
            if (element != null)
                SetAspect(element);

			if (_noTokenDetails)
				(transform as RectTransform).anchoredPosition = posNoTokenDetails;
			else
				(transform as RectTransform).anchoredPosition = new Vector2( 0f, -tokenDetailsHeight.sizeDelta.y - 10f);

		//	Debug.Log("tokenDetails size : "+ tokenDetailsHeight.sizeDelta.y);
        }

        void SetAspect(Element element)
		{
            ShowImage(ResourcesManager.GetSpriteForAspect(element.Icon));
            ShowText(Registry.Get<ILocStringProvider>().Get("UI_ASPECT") + element.Label, element.Description);
            adjuster.Adjust();
        }
    }
}