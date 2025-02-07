﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Ghosts;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SecretHistories.Manifestations

{
    [RequireComponent(typeof(RectTransform))]
    public class TextManifestation : BasicManifestation, IManifestation
    {

        
        [SerializeField] private TMP_Text textComponent;
        public override void Retire(RetirementVFX retirementVfx, Action callback)
        {
            Destroy(gameObject);
        }

        public bool CanAnimateIcon()
        {
            return false;
        }

        public void BeginIconAnimation()
        {
            //
        }

        private void UpdateTextFromManifestable(IManifestable manifestable)
        {
            var description = manifestable.GetIllumination(NoonConstants.TLG_NOTES_DESCRIPTION_KEY);
            string emphasisLevel = manifestable.GetIllumination(NoonConstants.TLG_NOTES_EMPHASISLEVEL_KEY);
            int.TryParse(emphasisLevel, out var l);
            if (l == -1)
                textComponent.fontStyle = FontStyles.Italic;
            else
                textComponent.fontStyle = FontStyles.Normal;

            textComponent.text = description;
        }

        public void Initialise(IManifestable manifestable)
        {
            UpdateTextFromManifestable(manifestable);
        }

  
        public void UpdateVisuals(IManifestable manifestable, Sphere sphere)
        {
            UpdateTextFromManifestable(manifestable);
        }


        public void Highlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public void Unhighlight(HighlightType highlightType, IManifestable manifestable)
        {
        }

        public bool NoPush => true;
        public void Unshroud(bool instant)
        {
        
        }

        public void Shroud(bool instant)
        {
            
        }

        public void Emphasise()
        {
           
        }

        public void Understate()
        {
        }

        public bool RequestingNoDrag => false;
        public bool RequestingNoSplit => false;

   
        public void SendNotification(INotification notification)
        {
        }

        public bool HandlePointerClick(PointerEventData eventData, Token token)
        {
            return false;
        }

        public void DisplaySpheres(IEnumerable<Sphere> spheres)
        {
        }

        public IGhost CreateGhost()
        {
            return NullGhost.Create(this);
        }

        public OccupiesSpaceAs OccupiesSpaceAs() => Enums.OccupiesSpaceAs.Intangible;
    }
}
