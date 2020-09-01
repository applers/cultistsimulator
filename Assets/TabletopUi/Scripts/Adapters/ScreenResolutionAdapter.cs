﻿using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    public class ScreenResolutionAdapter: ISettingSubscriber
    {
        public void UpdateValueFromSetting(float newValue)
        {
            throw new System.NotImplementedException();
        }

        protected void SetResolution(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

    }
}