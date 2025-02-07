﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;

namespace SecretHistories.Spheres.Angels

{
    public class AngelFlock
    {
        protected readonly HashSet<IAngel> _angels = new HashSet<IAngel>();
        public void AddAngel(IAngel angel)
        {
            _angels.Add(angel);
        }

        public void Act(float seconds, float metaseconds)
        {
            _angels.RemoveWhere(a => a.Defunct);

            foreach (var angel in _angels)

                    angel.Act(seconds, metaseconds);
        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            foreach (var a in _angels.OrderByDescending(angel => angel.Authority))
                if (a.MinisterToDepartingToken(token, context))
                    return true;

            return false;
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            foreach (var a in _angels.OrderByDescending(angel=>angel.Authority))
                if (a.MinisterToEvictedToken(token, context))
                    return true;

            return false;
        }

        public bool HasAngel(Type angelType)
        {
            return _angels.Any(a => a.GetType() == angelType);
        }

        public bool HasHomingAngelFor(Token token)
        {
            var _homingAngels = _angels.Where(a => a.GetType() == typeof(HomingAngel)).Select(a=>a as HomingAngel).ToList();
            if (!_homingAngels.Any())
                return false;
            var matchingHomingAngel = _homingAngels.Where(ha => ha.GetTokenToBringHome() == token);

            if (matchingHomingAngel.Any()) //should only ever be one, but just in case
                return true;
            else
                return false;
        }

        public void RetireAllAngels()
        {
            foreach (var a in _angels)
                a.Retire(); //they'll get cleaned out of the collection on next Act()
        }


        public IEnumerable<SphereBlock> GetImplicitAngelBlocks()
        {
            var blocks = new List<SphereBlock>();
            foreach(var a in _angels)
                if(a.GetType()==typeof(GreedyAngel)) // TODO: it's called polymorphism you pig-toothed dog-licker
                    blocks.Add(new SphereBlock(FucinePath.Current(), BlockDirection.Inward,BlockReason.GreedyAngel));

            return blocks;
        }

    }
}
