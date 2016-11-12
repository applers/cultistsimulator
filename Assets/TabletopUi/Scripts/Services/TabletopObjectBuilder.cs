﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class TabletopObjectBuilder
    {
        private Transform destination;
        string[] legalElementIDs = new string[7] {
            "health",
            "reason",
            "clique",
            "ordinarylife",
            "suitablepremises",
            "occultscrap",
            "shilling"
        };

        public TabletopObjectBuilder(Transform tabletop)
        {
            destination = tabletop;

        }

       public void PopulateTabletop()
        {

            VerbBox box;
            ElementStack stack;

            float boxWidth = (PrefabFactory.GetPrefab<VerbBox>().transform as RectTransform).rect.width + 20f;
            float boxHeight = (PrefabFactory.GetPrefab<VerbBox>().transform as RectTransform).rect.height + 50f;
            float cardWidth = (PrefabFactory.GetPrefab<ElementStack>().transform as RectTransform).rect.width + 20f;


            // build verbs
            var verbs = Registry.Compendium.GetAllVerbs();

            for (int i = 0; i < verbs.Count; i++)
            {
                box = PrefabFactory.CreateTokenWithSubscribers<VerbBox>(destination);
                box.SetVerb(verbs[i]);
                box.transform.localPosition = new Vector3(-1000f + i * boxWidth, boxHeight);
            }


            for (int i = 0; i < 10; i++)
            {
                stack = PrefabFactory.CreateTokenWithSubscribers<ElementStack>(destination);
                stack.Populate(legalElementIDs[i % legalElementIDs.Length], 3);
                stack.transform.localPosition = new Vector3(-1000f + i * cardWidth, 0f);
            }
        }



    }
}
