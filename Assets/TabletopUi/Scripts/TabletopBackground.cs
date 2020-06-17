﻿using Assets.Core.Entities;
using Noon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.CS.TabletopUI
{
    public class TabletopBackground : MonoBehaviour, IDropHandler, IPointerClickHandler {

        public event System.Action onDropped;
        public event System.Action onClicked;

#pragma warning disable 649
        [SerializeField] Image Cover;
        [SerializeField] Image Edge;
#pragma warning restore 649


        public void OnDrop(PointerEventData eventData) {
            if (DraggableToken.itemBeingDragged == null)
                return;

            onDropped?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClicked?.Invoke();
        }

        public void ShowTabletopFor(Legacy characterActiveLegacy)
        {
            if (characterActiveLegacy.Id.ToLower().Contains("exile"))
            {
                var exileLeather = ResourcesManager.GetSprite("ui/", "table_leather_exile");
                Cover.sprite = exileLeather;
            }
        }

    }
}
