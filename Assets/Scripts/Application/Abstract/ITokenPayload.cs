﻿using System;
using System.Collections.Generic;
using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Commands;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;

namespace SecretHistories.Abstract
    {
    public interface ITokenPayload: IEncaustable,IManifestable
    {
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;
        bool IsOpen { get;}


        public FucinePath AbsolutePath { get; }
        public List<IDominion> Dominions { get; }
        Type GetManifestationType(SphereCategory sphereCategory);
        void InitialiseManifestation(IManifestation manifestation);
        bool IsValidElementStack();
        void ExecuteHeartbeat(float interval);
        bool CanInteractWith(ITokenPayload incomingTokenPayload);
        bool CanMergeWith(ITokenPayload incomingTokenPayload);
        bool Retire(RetirementVFX vfx);
        void InteractWithIncoming(Token incomingToken);
        bool ReceiveNote(string label, string description,Context context);
        void ShowNoMergeMessage(ITokenPayload incomingTokenPayload);
        void SetQuantity(int quantityToLeaveBehind, Context context);
        void ModifyQuantity(int unsatisfiedChange, Context context);

        void ExecuteTokenEffectCommand(IAffectsTokenCommand command);

        void OpenAt(TokenLocation location);
        void Close();

        void OnTokenMoved(TokenLocation toLocation);
    }
    }

