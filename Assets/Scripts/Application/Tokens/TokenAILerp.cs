﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathfinding;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens
{
    public class TokenAILerp : AILerp
    {
        public event Action<Token, Context> OnTokenArrival;
        private Heart _heart;


        public override void OnTargetReached()
        {
            var token = gameObject.GetComponent<Token>();
            if (OnTokenArrival != null)
                OnTokenArrival.Invoke(token, Context.Unknown());

        }

        protected override void Start()
        {
            _heart = Watchman.Get<Heart>();
            base.Start();
            

        }

    protected override void Update()
    {
        if (_heart == null)
            return;

        if (_heart.IsPaused()) //This is pretty trivial. We will likely want to work in fast-forward effects
            return;
            

            base.Update();

            if (!hasPath || !canMove)
                return;


            var token = gameObject.GetComponent<Token>();
            
            //has the token moved to within the bounds of another World sphere?
            var traversableSpheres = Watchman.Get<HornedAxe>().GetTraversableSpheres();
            foreach (var ts in traversableSpheres)
            {
                if(ts==token.Sphere)
                    continue;

                if(ts.IsWorldPointInBoundingRect(token.TokenRectTransform.position))
                {
                    token.Sphere.Understate();
                    ts.AcceptToken(token,Context.Unknown());
                    ts.Emphasise();
                }
            }

        
        }
    }
}
