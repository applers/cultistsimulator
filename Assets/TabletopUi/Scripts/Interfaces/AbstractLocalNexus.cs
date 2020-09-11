﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Assets.TabletopUi.Scripts.Interfaces
{

    

   public abstract class LocalNexus : MonoBehaviour
   {
       [SerializeField] public UnityEvent ViewFilesEvent;
       [SerializeField] public UnityEvent ToggleOptionsEvent;
       [SerializeField] public UnityEvent SaveAndExitEvent;
       [SerializeField] public UnityEvent ToggleDebugEvent;
       [SerializeField] public UnityEvent StackCardsEvent;
        [SerializeField] public SpeedControlEvent SpeedControlEvent;
       [SerializeField] public UILookAtMeEvent UILookAtMeEvent;
       [SerializeField] public ZoomEvent ZoomEvent;

        public void Awake()
        {
       var registry = new Registry();
       registry.Register(this);

        }
    }
}