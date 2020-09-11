﻿using System;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts.UI
{
    public class FucineSliderSettingControlStrategy : SliderSettingControlStrategy
    {
        public override void SetSliderValues(Slider slider)
        {
            slider.minValue = boundSetting.MinValue;
            slider.maxValue = boundSetting.MaxValue;
            
            slider.SetValueWithoutNotify( Convert.ToSingle(boundSetting.CurrentValue));
        }


        public override string GetLabelForValue(float forValue)
        {
            boundSetting.ValueLabels.TryGetValue(forValue.ToString(), out var matchingValueLabelString);
            if (!string.IsNullOrEmpty(matchingValueLabelString))
            {
                return matchingValueLabelString;
            }
            else
                return NoonConstants.TEMPLATE_MARKER +  forValue;
        }

    }
}