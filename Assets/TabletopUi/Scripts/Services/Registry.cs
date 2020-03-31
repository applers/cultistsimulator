﻿using System;
using System.Collections.Generic;
using Assets.Core;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.CS.TabletopUI
{
    //public interface IRegisterable
    //{
        
    //}

    public class Registry
    {
        private static Dictionary<Type, System.Object> registered=new Dictionary<Type, object>();

        public static T Retrieve<T>() where T: class
        {
            if (!registered.ContainsKey(typeof(T)))
                throw new ApplicationException(typeof(T).Name + " wasn't registered");
            T got = registered[typeof(T)] as T;
      
            return got;
        }

        public static void Replace<T>(T toRegister) where T : class
        {
            if (!registered.ContainsKey(typeof(T)))
                throw new ApplicationException(typeof(T).Name + " hadn't been registered - can't replace it.");
            registered[typeof(T)] = toRegister;
        }

        public void Register<T>(T toRegister) where T: class
        {
            registered[typeof(T)] = toRegister;
        }

    }
}
