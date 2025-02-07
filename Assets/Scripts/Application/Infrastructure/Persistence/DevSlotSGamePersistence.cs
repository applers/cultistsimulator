﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using UnityEngine;

namespace SecretHistories.Infrastructure.Persistence
{
    public class DevSlotSaveGamePersistenceProvider: GamePersistenceProvider
    {
        private readonly int _slotNumber;

        public DevSlotSaveGamePersistenceProvider(int slotNumber)
        {
            _slotNumber = slotNumber;
        }

        protected override string GetSaveFileLocation()
        {
            return $"{Application.persistentDataPath}/devsave_{_slotNumber}.json";
        }

        public override void DepersistGameState()
        {
            throw new NotImplementedException();
        }
    }
}
