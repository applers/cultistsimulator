﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
   public class StorefrontServicesProvider
   {
       private IStoreFrontClientProvider _steamClientProvider;
#pragma warning disable 649
       private IStoreFrontClientProvider _gogClientProvider; //it is assigned, it's just in a compile block below
#pragma warning restore 649
        public void InitialiseForStorefrontClientType(StoreClient clientType)
        {
            if (clientType == StoreClient.Steam)
            {
                _steamClientProvider=new SteamworksStorefrontClientProvider();

            }
            if (clientType == StoreClient.Gog)
            {
#if UNITY_STANDALONE_LINUX
return;
#elif UNITY_WEBGL
                return;
#else
              //  if (Application.platform == RuntimePlatform.OSXPlayer)
                //    return;
                //we're integrating with GOG again

                _gogClientProvider = new GOGStorefrontProvider();
               return;
#endif

            }

        }
        public void SetAchievementForCurrentStorefronts(string achievementId, bool setStatus)
        {
            try
            {
            
                if (!NoonUtility.AchievementsActive)
                    return;
                if(_steamClientProvider!=null)
                    _steamClientProvider.SetAchievement(achievementId,setStatus);

                if(_gogClientProvider!=null)
                    _gogClientProvider.SetAchievement(achievementId,setStatus);
            }
            catch (Exception e)
            {
                //let's try not to bring down the house because the phone line isn't working
                NoonUtility.Log("WARNING: tried to set achievement" + achievementId + ", but failed: " + e.Message);
                //throw;
            }
            
        }


        public void UploadModForCurrentStorefront(Mod modToUpload)
        {
            var steamClient = _steamClientProvider as SteamworksStorefrontClientProvider;

            if(steamClient!=null)

                steamClient.UploadMod(modToUpload);

        }
    }
}
