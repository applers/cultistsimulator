﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;

namespace Assets.Scripts.Meta
{
    public class ConsciencePoinker
    {
        private void AppealToConscience()
        {
            string appealToConscienceLocation = Application.streamingAssetsPath + "/edition/please_buy_our_game.txt";
            if (File.Exists(appealToConscienceLocation))
            {
                var content = File.ReadLines(appealToConscienceLocation);
                DateTime expiry = Convert.ToDateTime(content.First());
                if (DateTime.Today > expiry)
                {
                    Registry.Get<Concursum>().ShowNotification(new NotificationArgs("ERROR - PLEASE UPDATE GAME", @"CRITICAL UPDATE REQUIRED"));
                    return; //something here needs to end execution
                }
            }
        }
    }
}
