﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;
using Assets.TabletopUi.Scripts;
using Noon;
using Object = UnityEngine.Object;


namespace Assets.TabletopUi.Scripts.Services
{
    class PrefabFactory : MonoBehaviour
    {
        [Header("Prefabs")]
        public ElementFrame ElementFrame = null;
        public SituationToken SituationToken = null;
        public ElementStackToken ElementStackToken = null;
        public SituationWindow SituationWindow = null;
        public RecipeSlot RecipeSlot = null;
        public NotificationWindow NotificationWindow = null;
        public SituationNote SituationNote = null;
		public DropZoneToken DropZoneToken = null;	// Bit of a hack - selection is done by type but I wanted DropZone to be a customised ElementStackToken

        [Header("Token Subscribers")]
        [SerializeField] TabletopManager TabletopManager = null;


        public static T CreateToken<T>(Transform destination, string saveLocationInfo = null) where T : DraggableToken
        {
            var token = PrefabFactory.CreateLocally<T>(destination);
            try
            {
                
                var pf = Instance();

                token.SetTokenContainer(pf.TabletopManager._tabletop, new Context(Context.ActionSource.Unknown));
                if (saveLocationInfo != null)
                    token.SaveLocationInfo = saveLocationInfo;

                
            }
            catch
            {
                Debug.Log("problem: " + saveLocationInfo);
            }

            return token;
        }


        public static T CreateLocally<T>(Transform parent) where T : Component
        {
            var o = GetPrefab<T>();
            try
            { 
            T c = Object.Instantiate(o, parent, false) as T;
                c.transform.localScale = Vector3.one;

                return c;
            }
            catch (Exception e)
            {
                NoonUtility.Log("Couldn't instantiate prefab " + typeof(T) + "\n" + e);
                return null;
            }

        }

        public static T GetPrefab<T>() where T : Component
        {
            var pf = Instance();

            string prefabFieldName = typeof(T).Name;

            FieldInfo field = pf.GetType().GetField(prefabFieldName);
            if (field == null)
                throw new ApplicationException(prefabFieldName +
                                               " not registered in prefab factory; must have field name and type both '" +
                                               prefabFieldName + "', must have field populated in editor");

            T prefab = field.GetValue(pf) as T;
            return prefab;
        }


        private static PrefabFactory Instance()
        {
            var instance = FindObjectOfType<PrefabFactory>();
            if (instance == null)
                throw new ApplicationException("No prefab factory in scene");

            return instance;
        }
    }
}