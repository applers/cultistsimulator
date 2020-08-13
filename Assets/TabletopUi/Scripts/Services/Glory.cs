﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class Glory: MonoBehaviour
    {
        [SerializeField]
        public LanguageManager languageManager;


        public void Awake()
        {

            var registryAccess = new Registry();


            var storefrontServicesProvider = new StorefrontServicesProvider();
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Steam);
            storefrontServicesProvider.InitialiseForStorefrontClientType(StoreClient.Gog);
            registryAccess.Register<StorefrontServicesProvider>(storefrontServicesProvider);




            //TODO: make this async
            registryAccess.Register(new ModManager());
            registryAccess.Register<ICompendium>(new Compendium());


            string startingCultureId;

            // Try to auto-detect the culture from the system language first
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                    startingCultureId = "ru";
                    break;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    startingCultureId = "zh-hans";
                    break;
                default:
                    switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                    {
                        case "zh":
                            startingCultureId = "zh-hans";
                            break;
                        case "ru":
                            startingCultureId = "ru";
                            break;
                        default:
                            startingCultureId = "en";
                            break;
                    }
                    break;
            }

            // If the player has already chosen a culture, use that one instead
            if (PlayerPrefs.HasKey(NoonConstants.CULTURE_SETTING_KEY))
            {
                startingCultureId = PlayerPrefs.GetString(NoonConstants.CULTURE_SETTING_KEY);
            }

            // If an override is specified, ignore everything else and use that
            if (Config.Instance.culture != null)
            {
                startingCultureId = Config.Instance.culture;
            }

            if (string.IsNullOrEmpty(startingCultureId))
                startingCultureId = NoonConstants.DEFAULT_CULTURE_ID;



            var contentImporter = new CompendiumLoader();
            var log = contentImporter.PopulateCompendium(Registry.Retrieve<ICompendium>(),startingCultureId);

            foreach (var m in log.GetMessages())
                NoonUtility.Log(m);

            registryAccess.Register<LanguageManager>(languageManager);
            languageManager.Initialise(Registry.Retrieve<ICompendium>(),startingCultureId);
  

        }

    }
}
