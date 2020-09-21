﻿using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public abstract class SliderSettingControlStrategy:SettingControlStrategy
    {



        public float SettingCurrentValue
        {
            get { return boundSetting.CurrentValue is float ? (float) boundSetting.CurrentValue : 0; }
        }

        
        public abstract void SetSliderValues(Slider slider);

        
        public void OnSliderValueChangeComplete(float newValue)
        {
            boundSetting.CurrentValue = newValue;
            
        }

        public abstract string GetLabelForValue(float forValue);
    
    }
}