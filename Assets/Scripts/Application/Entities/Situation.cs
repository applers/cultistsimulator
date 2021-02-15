﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Interfaces;
using SecretHistories.NullObjects;
using SecretHistories.Services;
using SecretHistories.States;
using SecretHistories.UI;
using Assets.Logic;
using Assets.Scripts.Application.Commands.SituationCommands;
using Assets.Scripts.Application.Entities;
using Assets.Scripts.Application.Entities.NullEntities;
using Assets.Scripts.Application.Infrastructure.Events;
using Assets.Scripts.Application.Spheres;
using Newtonsoft.Json;
using SecretHistories.Commands.SituationCommands;
using SecretHistories.Constants.Events;
using SecretHistories.Spheres;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Core;
using SecretHistories.Elements.Manifestations;
using SecretHistories.Logic;
using SecretHistories.States.TokenStates;
using UnityEngine;

namespace SecretHistories.Entities {

    [IsEncaustableClass(typeof(SituationCreationCommand))]
    public class Situation: ISphereEventSubscriber,ITokenPayload
    {
        public event Action<TokenPayloadChangedArgs> OnChanged;
        public event Action<float> OnLifetimeSpent;

        [Encaust]
        public StateEnum StateForRehydration => State.RehydrationValue;


        [Encaust] public string RecipeId => Recipe.Id;

        [DontEncaust]
        public Recipe Recipe { get; set; }

        [Encaust]
        public float TimeRemaining => _timeshadow.LifetimeRemaining;

        [Encaust]
        public string VerbId => Verb.Id;

        [DontEncaust]
        public Verb Verb { get; set; }

        [Encaust]
        public string OverrideTitle { get; set; }

        [Encaust]
        public SituationPath Path { get; }

        [Encaust]
        public bool IsOpen { get; private set; }

        [Encaust]
        public SituationCommandQueue CommandQueue { get; set; } = new SituationCommandQueue();

      //  [Encaust]
      //  public RecipeCompletionEffectCommand CurrentCompletionEffectCommand { get; set; } = new RecipeCompletionEffectCommand();

        [Encaust] public Dictionary<string, int> Mutations => new Dictionary<string, int>();

        [DontEncaust] public SituationState State { get; set; }
        [DontEncaust] public float Warmup => Recipe.Warmup;
        [DontEncaust] public RecipePrediction CurrentRecipePrediction => _currentRecipePrediction;
        [DontEncaust] public string Id => Path.ToString();
        [DontEncaust] public string Label => GetMostRecentNoteLabel();
        [DontEncaust] public string Description => GetMostRecentNoteDescription();
        [DontEncaust] public int Quantity => 1;
        [DontEncaust] public float IntervalForLastHeartbeat => _timeshadow.LastInterval;
        [DontEncaust]
        public string UniquenessGroup
        {
            get
            {
                if (Unique)
                    return Verb.Id;
                return string.Empty;
            }
        }

        [DontEncaust]
        public bool Unique
        {
            get
            {
                if (!Verb.Spontaneous)
                    return true;
                if (!State.AllowDuplicateVerbIfVerbSpontaneous)
                    return true;

                return false;
            }
        }
        [DontEncaust]
        public string Icon
        {
            get
            {
                if (!string.IsNullOrEmpty(Verb.Icon))
                    return Verb.Icon;

                return Verb.Id;
            }

        }


        private RecipePrediction _currentRecipePrediction;

        private readonly List<ISituationSubscriber> _subscribers = new List<ISituationSubscriber>();
        private readonly List<Dominion> _registeredDominions = new List<Dominion>();
        private readonly HashSet<Sphere> _spheres = new HashSet<Sphere>();
        
        private Timeshadow _timeshadow;
        



        public Situation(SituationPath path,Verb verb)
        {
            SituationsCatalogue situationsCatalogue = Watchman.Get<SituationsCatalogue>();

            situationsCatalogue.RegisterSituation(this);

            Path = path;
            Verb = verb;
            Recipe = NullRecipe.Create(Verb);
         UpdateCurrentRecipePrediction(RecipePrediction.DefaultFromVerb(Verb),new Context(Context.ActionSource.SituationCreated));
            State=new NullSituationState();
            _timeshadow = new Timeshadow(Recipe.Warmup,
                Recipe.Warmup,
                false);
        }

        public void TransitionToState(SituationState newState)
        {
            State.Exit(this);
            newState.Enter(this);
            State = newState;
            NotifyStateChange();
            NotifyTimerChange();
        }

        public bool RegisterDominion(Dominion dominionToRegister)
        {
            dominionToRegister.OnSphereAdded.AddListener(AttachSphere);
            dominionToRegister.OnSphereRemoved.AddListener(RemoveSphere);

            if (_registeredDominions.Contains(dominionToRegister))
                return false;

            _registeredDominions.Add(dominionToRegister);
            return true;
        }

   
        public bool AddSubscriber(ISituationSubscriber subscriber)
        {

            if (_subscribers.Contains(subscriber))
                return false;

            _subscribers.Add(subscriber);
            return true;
        }

        public bool RemoveSubscriber(ISituationSubscriber subscriber)
        {
            if (!_subscribers.Contains(subscriber))
                return false;

            _subscribers.Remove(subscriber);
            return true;
        }


        public void AttachSphere(Sphere sphere)
        {
            sphere.Subscribe(this);
            sphere.SetSituationPath(Path);
            _spheres.Add(sphere);
        }

        public void AttachSpheres(IEnumerable<Sphere> containers)
        {
            foreach (var c in containers)
                AttachSphere(c);
        }

        public void RemoveSphere(Sphere c)
        {
            c.Unsubscribe(this);
            _spheres.Remove(c);
            
        }

        public void UpdateCurrentRecipePrediction(RecipePrediction newRecipePrediction,Context context)
        {
            if (newRecipePrediction != _currentRecipePrediction) //repeatedly assigning might mean infinite loops, because eg changing prediction might add recipe note elements
                    _currentRecipePrediction = newRecipePrediction;

            if (Label != newRecipePrediction.Title || Description != newRecipePrediction.DescriptiveText)
            {
                var addNoteCommand=new AddNoteCommand(newRecipePrediction.Title,newRecipePrediction.DescriptiveText,context);
                addNoteCommand.ExecuteOn(this);
            }
        }


        public void Reset()
        {
            Recipe = NullRecipe.Create(Verb);
            UpdateCurrentRecipePrediction(RecipePrediction.DefaultFromVerb(Verb), new Context(Context.ActionSource.SituationReset));
           _timeshadow=Timeshadow.CreateTimelessShadow();
            NotifyStateChange();
            NotifyTimerChange();
        }




        public List<Sphere> GetSpheres()
        {
            return new List<Sphere>(_spheres);
        }


        public List<Sphere> GetSpheresActiveForCurrentState()
        {
            return new List<Sphere>(_spheres).Where(sphere => State.IsActiveInThisState(sphere)).ToList();
        }

        public List<Sphere> GetSpheresByCategory(SphereCategory category)
        {
            return new List<Sphere>(_spheres.Where(c => c.SphereCategory == category));
        }

        public Sphere GetSingleSphereByCategory(SphereCategory category)
        {
            try
            {
                return _spheres.SingleOrDefault(c => c.SphereCategory == category);
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }

            try
            {
                return GetSpheresByCategory(category).First();
            }
            catch (Exception e)
            {
                NoonUtility.LogException(e);
            }

            return null;

        }
        
        public void Retire()
        {
            foreach (var c in _spheres)
            {
                c.Retire(SphereRetirementType.Destructive);
            }

            TokenPayloadChangedArgs args = new TokenPayloadChangedArgs(this, PayloadChangeType.Retirement);
            args.VFX = RetirementVFX.VerbAnchorVanish;
            OnChanged?.Invoke(args);
            Watchman.Get<SituationsCatalogue>().DeregisterSituation(this);
        }



        public Type GetManifestationType(SphereCategory sphereCategory)
        {
            return typeof(VerbManifestation);
        }

        public void InitialiseManifestation(IManifestation manifestation)
        {
            manifestation.InitialiseVisuals(this);

        }

        public bool IsValidElementStack()
        {
            return false;
        }

        public void ExecuteHeartbeat(float interval)
        {
            Continue(interval);
        }

        public bool CanInteractWith(ITokenPayload incomingTokenPayload)
        {
            if (GetAvailableThresholdsForStackPush(incomingTokenPayload).Count > 0)
                return true;

            return false;
        }

        public bool CanMergeWith(ITokenPayload incomingTokenPayload)
        {
            return false;
        }

        public bool Retire(RetirementVFX vfx)
        {
            return false;
        }

        public void InteractWithIncoming(Token token)
        {
            if (token.IsValidElementStack())
            {
                TryPushDraggedStackIntoThreshold(token);
                if (!IsOpen)
                    OpenAt(token.Location);
            }
            else
            {
                //something has gone awryy
                token.SetState(new RejectedBySituationState());
            }
        }

        public bool ReceiveNote(string label, string description,Context context)
        {
            //we often add a . to indicate that the description is intentionally empty.
            //if we do that, or if it's a mistaken empty string, just go back.
            if (description == "." || description == string.Empty)
                return true;

            //no infinite loops pls
            if (label == this.Label && description == this.Description)
                return true;

            var noteElementId = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().NoteElementId;

            var notesSpheres = GetSpheresByCategory(SphereCategory.Notes);

            Sphere emptyNoteSphere;
            try
            {
               emptyNoteSphere = notesSpheres.SingleOrDefault(ns => ns.GetTotalStacksCount() == 0);
               if(emptyNoteSphere==null)
               {
                   var notesDominion = GetSituationDominionsForCommandCategory(CommandCategory.Notes).FirstOrDefault();
                   if (notesDominion == null)
                   {
                       NoonUtility.Log($"No notes sphere and no notes dominion found in {Path}: we won't add note {label}, then.");
                        return false;
                   }
                   var notesSphereSpec=new SphereSpec(new NotesSphereSpecIdentifierStrategy(0));
                   emptyNoteSphere=notesDominion.CreatePrimarySphere(notesSphereSpec);
               }
            }
            catch (Exception e)
            {
                NoonUtility.Log($"More than one empty notes sphere found in {Path} when we try to add a note. We'll take the first empty notes sphere and put a note in that.");
                emptyNoteSphere=notesSpheres.First(ns => ns.GetTotalStacksCount() == 0);
            }

            var newNoteCommand = new ElementStackCreationCommand(noteElementId, 1);
            newNoteCommand.Illuminations.Add(NoonConstants.TLG_NOTES_TITLE_KEY, label);
            newNoteCommand.Illuminations.Add(NoonConstants.TLG_NOTES_DESCRIPTION_KEY, description);

            var tokenCreationCommand =
                new TokenCreationCommand(newNoteCommand, TokenLocation.Default().WithSphere(emptyNoteSphere));

            tokenCreationCommand.Execute(context);

            
            return true;
        }

        private Sphere LastSphereWithANote()
        {
            var notesSpheres = GetSpheresByCategory(SphereCategory.Notes);
            var lastSphereWithANote = notesSpheres.LastOrDefault(s => s.GetAllTokens().Any());
            return lastSphereWithANote;
        }

        private string GetMostRecentNoteLabel()
        {
            var noteSphereToCheck = LastSphereWithANote();
            if (noteSphereToCheck == null)
                return string.Empty;

            var note = noteSphereToCheck.GetAllTokens().First().Payload;
            string title = note.GetIllumination(NoonConstants.TLG_NOTES_TITLE_KEY);
            return title;


        }

        private string GetMostRecentNoteDescription()
        {
            var noteSphereToCheck = LastSphereWithANote();
            if (noteSphereToCheck == null)
                return string.Empty;

            var note = noteSphereToCheck.GetAllTokens().First().Payload;
            string description = note.GetIllumination(NoonConstants.TLG_NOTES_DESCRIPTION_KEY);
            return description;
        }

        public void ShowNoMergeMessage(ITokenPayload incomingTokenPayload)
        {
            //
        }

        public void SetQuantity(int quantityToLeaveBehind, Context context)
        {
         //
        }

        public void ModifyQuantity(int unsatisfiedChange, Context context)
        {
            //
        }

        public void ExecuteTokenEffectCommand(IAffectsTokenCommand command)
        {
            command.ExecuteOn(this);
        }


        public int TryPurgeStacks(Element elementToPurge, int maxToPurge)
        {

            var containersToPurge = GetSpheresByCategory(SphereCategory.Threshold).ToList();

            containersToPurge
                .Reverse(); //I couldn't remember why I put this - but I think it must have been to start with the final slot, so we don't dump everything by purging the primary slot.


            containersToPurge.AddRange(GetSpheresByCategory(SphereCategory.Output));


            foreach (var container in containersToPurge)
            {
                if (maxToPurge <= 0)
                    return maxToPurge;
                else
                    maxToPurge -= container.TryPurgeStacks(elementToPurge, maxToPurge);
            }

            return maxToPurge;



        }

        
    public void AcceptToken(SphereCategory forSphereCategory, Token token, Context context)
        {
            var tokenList = new List<Token> {token};
            AcceptTokens(forSphereCategory, tokenList, context);
        }

        public void AcceptTokens(SphereCategory forSphereCategory, IEnumerable<Token> tokens,
            Context context)
        {
            var acceptingSphere = GetSingleSphereByCategory(forSphereCategory);
            acceptingSphere.AcceptTokens(tokens, context);
        }

        public void AcceptTokens(SphereCategory forSphereCategory, IEnumerable<Token> tokens)
        {
            AcceptTokens(forSphereCategory,tokens,new Context(Context.ActionSource.Unknown));
        }

        public List<Token> GetElementTokensInSituation()
        {
            List<Token> stacks = new List<Token>();
            foreach (var sphere in _spheres)
                stacks.AddRange(sphere.GetElementTokens());
            return stacks;
        }

        public List<Token> GetTokens(SphereCategory forSphereCategory)
        {
            List<Token> stacks = new List<Token>();
            foreach (var container in _spheres.Where(c => c.SphereCategory == forSphereCategory))
                stacks.AddRange(container.GetAllTokens());

            return stacks;
        }

        public List<Token> GetElementTokens(SphereCategory forSphereCategory)
        {
            List<Token> stacks = new List<Token>();
            foreach (var container in _spheres.Where(c => c.SphereCategory == forSphereCategory))
                stacks.AddRange(container.GetElementTokens());

            return stacks;
        }

        /// <summary>
        /// These are the aspects in the situation, not the aspects available to recipe criteria in the situation
        /// </summary>
        /// <param name="includeElementAspects"></param>
        /// <returns></returns>
        public AspectsDictionary GetAspects(bool includeElementAspects)
        {
            var aspects = new AspectsDictionary();

            foreach (var container in _spheres.Where(c =>
                c.SphereCategory == SphereCategory.SituationStorage ||
                c.SphereCategory == SphereCategory.Threshold ||
            c.SphereCategory == SphereCategory.Output))
            {
                aspects.CombineAspects(container.GetTotalAspects(includeElementAspects));
            }

            return aspects;

        }

        

        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
            throw new NotImplementedException("Does it mean anything to set mutations on a situation?");
        }

        public string GetSignature()
        {
            // Generate a distinctive signature

            return "situation_" + Verb.Id;
        }

        private SituationState Continue(float interval)
        {
            

        _timeshadow.SpendTime(interval);

            State.Continue(this);

            return State;
        }


        public void NotifyStateChange()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.SituationStateChanged(this);
                subscriber.TimerValuesChanged(this);
            }

            foreach (var dominion in _registeredDominions)
            {
                if (State.IsVisibleInThisState(dominion))
                    dominion.Show();
                else
                    dominion.Hide();
            }

            OnChanged?.Invoke(new TokenPayloadChangedArgs(this,PayloadChangeType.Update));
        }

        public void NotifyTimerChange()
        {
            
            foreach (var subscriber in _subscribers)
                subscriber.TimerValuesChanged(this);
            OnChanged?.Invoke(new TokenPayloadChangedArgs(this, PayloadChangeType.Update));
        }


        public void SendCommandToSubscribers(IAffectsTokenCommand command)
        {
            foreach(var s in _subscribers)
                s.ReceiveCommand(command);
        }




        public void ExecuteCurrentRecipe()
        {
            
            var tc = Watchman.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(GetAspects(true));

            RecipeConductor rc =new RecipeConductor(aspectsInContext, Watchman.Get<Stable>().Protag());

            IList<RecipeExecutionCommand> recipeExecutionCommands = rc.GetRecipeExecutionCommands(Recipe);

            //actually replace the current recipe with the first on the list: any others will be additionals,
            //but we want to loop from this one.
            if (recipeExecutionCommands.First().Recipe.Id != Recipe.Id)
                Recipe = recipeExecutionCommands.First().Recipe;


            //Check for possible text refinements based on the aspects in context
            var aspectsInSituation = GetAspects(true);
            TextRefiner tr = new TextRefiner(aspectsInSituation);

            var addNoteCommand = new AddNoteCommand(Recipe.Label, tr.RefineString(Recipe.Description),new Context(Context.ActionSource.SituationEffect));

            ExecuteTokenEffectCommand(addNoteCommand);
            

            foreach (var c in recipeExecutionCommands)
            {
                RecipeCompletionEffectCommand currentEffectCommand = new RecipeCompletionEffectCommand(c.Recipe,
                    c.Recipe.ActionId != Recipe.ActionId, c.Expulsion,c.ToPath);
                if (currentEffectCommand.AsNewSituation)
                    SpawnNewSituation(currentEffectCommand);
                else
                {
                    Watchman.Get<Stable>().Protag().AddExecutionsToHistory(currentEffectCommand.Recipe.Id, 1);
                    var executor = new SituationEffectExecutor(Watchman.Get<TabletopManager>());
                    executor.RunEffects(currentEffectCommand, GetSingleSphereByCategory(SphereCategory.SituationStorage), Watchman.Get<Stable>().Protag(), Watchman.Get<IDice>());




                    if (!string.IsNullOrEmpty(currentEffectCommand.Recipe.Ending))
                    {
                        var ending = Watchman.Get<Compendium>().GetEntityById<Ending>(currentEffectCommand.Recipe.Ending);
                        
                        var endGameCommand=new EndGameAtTokenCommand(ending);

                        SendCommandToSubscribers(endGameCommand);
                    }


                }

            }
        }

        private void SpawnNewSituation(RecipeCompletionEffectCommand effectCommand)
        {
            List<Token> stacksToAddToNewSituation = new List<Token>();
            //if there's an expulsion
            if (effectCommand.Expulsion.Limit > 0)
            {
                //find one or more matching stacks. Important! the limit applies to stacks, not cards. This might need to change.
                AspectMatchFilter filter = new AspectMatchFilter(effectCommand.Expulsion.Filter);
                var filteredStacks = filter.FilterElementStacks(GetElementTokens(SphereCategory.SituationStorage)).ToList();

                if (filteredStacks.Any() && effectCommand.Expulsion.Limit > 0)
                {
                    while (filteredStacks.Count > effectCommand.Expulsion.Limit)
                    {
                        filteredStacks.RemoveAt(filteredStacks.Count - 1);
                    }

                    stacksToAddToNewSituation = filteredStacks;
                }

            }

            var situationCreationCommand = new SituationCreationCommand(Recipe.ActionId, effectCommand.Recipe.Id,new SituationPath(Recipe.ActionId),
                StateEnum.Ongoing);
            situationCreationCommand.TokensToMigrate = stacksToAddToNewSituation;
            var spawnNewTokenCommand = new SpawnNewTokenFromHereCommand(situationCreationCommand,effectCommand.ToPath,new Context(Context.ActionSource.SpawningAnchor));

            SendCommandToSubscribers(spawnNewTokenCommand);

        }


    public void AttemptAspectInductions(Recipe currentRecipe, List<Token> outputTokens) // this should absolutely go through subscription - something to succeed ttm
    {
        //If any elements in the output, or in the situation itself, have inductions, test whether to start a new recipe

     var inducingAspects = new AspectsDictionary();

        //shrouded cards don't trigger inductions. This is because we don't generally want to trigger an induction
        //for something that has JUST BEEN CREATED. This started out as a hack, but now we've moved from 'face-down'
        //to 'shrouded' it feels more suitable.

        foreach (var os in outputTokens)
        {
            if (!os.Shrouded())
                inducingAspects.CombineAspects(os.GetAspects(true));
        }


        inducingAspects.CombineAspects(currentRecipe.Aspects);


        foreach (var a in inducingAspects)
        {
            var aspectElement = Watchman.Get<Compendium>().GetEntityById<Element>(a.Key);

            if (aspectElement != null)
                PerformAspectInduction(aspectElement);
            else
                NoonUtility.Log("unknown aspect " + a + " in output");
        }
    }



    void PerformAspectInduction(Element aspectElement)
    {
        foreach (var induction in aspectElement.Induces)
        {
            var d = Watchman.Get<IDice>();

            if (d.Rolld100() <= induction.Chance)
                CreateRecipeFromInduction(Watchman.Get<Compendium>() .GetEntityById<Recipe>(induction.Id), aspectElement.Id);
        }
    }

    void CreateRecipeFromInduction(Recipe inducedRecipe, string aspectID) // yeah this *definitely* should be through subscription!
    {
        if (inducedRecipe == null)
        {
            NoonUtility.Log("unknown recipe " + inducedRecipe + " in induction for " + aspectID);
            return;
        }

        
            SituationCreationCommand inducedSituationCreationCommand = new SituationCreationCommand(inducedRecipe.ActionId,
            inducedRecipe.Id, new SituationPath(inducedRecipe.ActionId), StateEnum.Ongoing);

            var spawnNewTokenCommand = new SpawnNewTokenFromHereCommand(inducedSituationCreationCommand, SpherePath.Current(), new Context(Context.ActionSource.SpawningAnchor));
            SendCommandToSubscribers(spawnNewTokenCommand);

    }


        public void OnTokenMoved(TokenLocation toLocation)
        {
            foreach (var sphere in GetSpheres())
                sphere.SetReferencePosition(toLocation);
        }


        public void OpenAt(TokenLocation location)
    {
           IsOpen = true;
           var changeArgs = new TokenPayloadChangedArgs(this, PayloadChangeType.Update);
           OnChanged?.Invoke(changeArgs);


            Watchman.Get<TabletopManager>().CloseAllSituationWindowsExcept(Id);
    }

        public void Close()
        {
            IsOpen = false;

            var changeArgs = new TokenPayloadChangedArgs(this, PayloadChangeType.Update);

            OnChanged?.Invoke(changeArgs);

            DumpUnstartedBusiness();
        }



        public List<Dominion> GetSituationDominionsForCommandCategory(CommandCategory commandCategory)
    {
            return new List<Dominion>(_registeredDominions.Where(a=>a.MatchesCommandCategory(commandCategory)));
    }

    public List<Sphere> GetAvailableThresholdsForStackPush(ITokenPayload stack)
    {
        List<Sphere> thresholdsAvailable=new List<Sphere>();
        var thresholds = GetSpheresByCategory(SphereCategory.Threshold);
        foreach (var t in thresholds)

            if (!t.IsGreedy
               && State.IsActiveInThisState(t)
               && !t.CurrentlyBlockedFor(BlockDirection.Inward)
               && t.GetMatchForTokenPayload(stack).MatchType ==SlotMatchForAspectsType.Okay)

                thresholdsAvailable.Add(t);

        return thresholdsAvailable;

    }


        public bool TryPushDraggedStackIntoThreshold(Token token)
        {
            var thresholdSpheres = GetAvailableThresholdsForStackPush(token.Payload);
            if (thresholdSpheres.Count <= 0)
                return false;

            Sphere emptyThresholdSphere = thresholdSpheres.FirstOrDefault(t => t.IsEmpty());

            if(emptyThresholdSphere!=null)
                return emptyThresholdSphere.TryAcceptToken(token,new Context(Context.ActionSource.PlayerDrag));

            Sphere occupiedThresholdSphere = thresholdSpheres.FirstOrDefault();
            if(occupiedThresholdSphere!=null)
            {
                var incumbent = occupiedThresholdSphere.GetElementTokens().FirstOrDefault();
                if(incumbent!=null) //should never happen, but code might shift later
                    incumbent.GoAway(new Context(Context.ActionSource.PushToThresholddUsurpedThisStack));
                 
                return occupiedThresholdSphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag));
            }


            return false;

        }



        public void DumpUnstartedBusiness()
        {
       //deferring this a bit longer. I need to think about how to connect startingslot behaviour with the BOH model:
       //slot behaviour: dump when window closed?
       //slot behaviour: block for certain kinds of interaction? using existing block?
       //slot behaviour: specify connection type with other containers? ie expand 'greedy' effect to mean multiple things and directions
            //if(CurrentState!=CurrentState.Ongoing)
            //{
            //    var slotted = GetStacks(ContainerCategory.Threshold);
            //    foreach (var item in slotted)
            //        item.ReturnToTabletop(new Context(Context.ActionSource.PlayerDumpAll));
            //}


        }
        
        /// <summary>
        /// collects output stacks, retires transient verbs (or should)
        /// </summary>
        public void Conclude()
        {
           CommandQueue.AddCommand(new ConcludeCommand());
            Continue(0f);
        }


        public void TryStart()
        {
         
            var aspects = GetAspects(true);
            var tc = Watchman.Get<SphereCatalogue>();
            var aspectsInContext = tc.GetAspectsInContext(aspects);


            var recipe = Watchman.Get<Compendium>().GetFirstMatchingRecipe(aspectsInContext, Verb.Id, Watchman.Get<Stable>().Protag(), false);

            //no recipe found? get outta here
            if (recipe != null)

            {
               var activateRecipeCommand=new TryActivateRecipeCommand(recipe);
                CommandQueue.AddCommand(activateRecipeCommand);

                //The game might be paused! or the player might just be incredibly quick off the mark
                //so immediately continue with a 0 interval

                Continue(0f);
            }

        }


        public RecipePrediction GetRecipePredictionForCurrentStateAndAspects()
        {
            var aspectsAvailableToSituation = GetAspects(true);

            var aspectsInContext =
                Watchman.Get<SphereCatalogue>().GetAspectsInContext(aspectsAvailableToSituation);

            RecipeConductor rc = new RecipeConductor(aspectsInContext,Watchman.Get<Stable>().Protag());

            return rc.GetPredictionForFollowupRecipe(Recipe, this);
        }


        public void OnTokensChangedForSphere(SphereContentsChangedEventArgs args)
        {
            var oldEndingFlavour = CurrentRecipePrediction.SignalEndingFlavour;
            UpdateCurrentRecipePrediction(GetRecipePredictionForCurrentStateAndAspects(),args.Context);
            var newEndingFlavour = CurrentRecipePrediction.SignalEndingFlavour;

            if (oldEndingFlavour!=newEndingFlavour)
                SendCommandToSubscribers(new SignalEndingFlavourCommand(newEndingFlavour));

            foreach (var s in _subscribers)
                s.SituationSphereContentsUpdated(this);
        }

        public void OnTokenInteractionInSphere(TokenInteractionEventArgs args)
        {
            //
        }
        
        public virtual bool IsValidSituation()
        {
            return true;
        }




        public string GetIllumination(string key)
        {
            if (key == NoonConstants.IK_SPONTANEOUS)
            {
                if (Verb.Spontaneous)
                    return "true";
            }

            return string.Empty;
        }

        public void SetIllumination(string key, string value)
        {
    //
        }

        public Timeshadow GetTimeshadow()
        {
            return _timeshadow;

        }

        public void ActivateRecipe(Recipe recipeToActivate)
        {
            Recipe = recipeToActivate;
            _timeshadow = new Timeshadow(Recipe.Warmup, Recipe.Warmup, false);
        }

        public void ReduceLifetimeBy(float timeSpent)
        {
            _timeshadow.SpendTime(timeSpent);
        }
    }

}
