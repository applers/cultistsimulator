﻿#pragma warning disable 0649
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Fucine;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Events;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using Steamworks;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VR;
using static Noon.NoonUtility;

public class DebugTools : MonoBehaviour,ISphereCatalogueEventSubscriber
{
    private const int MaxAutoCompletionSuggestions = 50;

    [SerializeField] private TabletopSphere tabletop;
    [SerializeField] private Heart heart;
    [SerializeField] private InputField input;
    [SerializeField] private ScrollRect autoCompletionBox;
    [SerializeField] private VerticalLayoutGroup autoCompletionSuggestions;
    [SerializeField] private Button btnPlusOne;
    [SerializeField] private Button btnMinusOne;
    [SerializeField] private Button btnBeginSituation;
    [SerializeField] private Button btnHaltVerb;
    [SerializeField] private Button btnDeleteVerb;
    [SerializeField] private Button btnPurgeElement;
    [SerializeField] public Button btnTriggerAchievement;
    [SerializeField] private Button btnResetAchivement;
    [SerializeField] private Button btnFastForward;
    [SerializeField] private Button btnNextTrack;
    [SerializeField] private Button btnUpdateContent;
    [SerializeField] private Button btnEndGame;
    [SerializeField] private Button btnLoadGame;
    [SerializeField] private Button btnSaveGame;
    [SerializeField] private Button btnResetDecks;
    [SerializeField] private BackgroundMusic backgroundMusic;
    [SerializeField] private Button btnQueueRoll;
    [SerializeField] private TMP_InputField rollToQueue;
    [SerializeField] private TextMeshProUGUI rollsQueued;

    // Debug Load/Save/Delete buttons
    [SerializeField] private List<Button> saveButtons;
    [SerializeField] private List<Button> loadButtons;
    [SerializeField] private List<Button> delButtons;

    public string endingAnimFXName = "DramaticLightEvil";

    public List<int> QueuedRollsList;

    public Transform AutoCompletionSuggestionPrefab;

    // Indicates the last selected auto-completion suggestion
    // -1 means no previous suggestion was selected
    private int currentAutoCompletionSuggestion = -1;

    public void Toggle()
    {
        gameObject.SetActive(!isActiveAndEnabled);
    }


    public void Awake()
    {
        Toggle(); //start by hiding the panel. If it's not enabled at the beginning, this won't run
        var registry = new Registry();
        registry.Register(this);


        {
            if (Registry.Get<Config>().knock)
                btnTriggerAchievement.gameObject.SetActive(true);

        }

        autoCompletionBox.gameObject.SetActive(false);
        input.onValueChanged.AddListener(AttemptAutoCompletion);
        btnPlusOne.onClick.AddListener(() => AddCard(input.text));
        btnMinusOne.onClick.AddListener(() => RemoveItem(input.text));
        btnFastForward.onClick.AddListener(() => FastForward(30));
        btnUpdateContent.onClick.AddListener(UpdateCompendiumContent);
        btnEndGame.onClick.AddListener(()=>EndGame(input.text));
        btnLoadGame.onClick.AddListener(LoadGame);
        btnSaveGame.onClick.AddListener(SaveGame);
        btnResetDecks.onClick.AddListener(ResetDecks);
        btnNextTrack.onClick.AddListener(NextTrack);
        btnBeginSituation.onClick.AddListener(()=>BeginSituation(input.text));
        btnHaltVerb.onClick.AddListener(() => HaltVerb(input.text));
        btnDeleteVerb.onClick.AddListener(() => DeleteVerb(input.text));
        btnPurgeElement.onClick.AddListener(() => PurgeElement(input.text));


        btnTriggerAchievement.onClick.AddListener(()=>TriggerAchievement(input.text));
        btnResetAchivement.onClick.AddListener(() => ResetAchievement(input.text));


        btnQueueRoll.onClick.AddListener(()=>QueueRoll(rollToQueue.text));

        QueuedRollsList=new List<int>();

        int sbIndex = 1;
        foreach (var saveButton in saveButtons)
        {
            var index = sbIndex;
            saveButton.onClick.AddListener(() => SaveDebugSave(index));
            sbIndex++;
        }

        int lbIndex = 1;
        foreach (var loadButton in loadButtons)
        {
            var index = lbIndex;
            loadButton.onClick.AddListener((() => LoadDebugSave(index)));
            if (!CheckDebugSaveExists(lbIndex))
                loadButton.interactable = false;

            lbIndex++;
        }

        int dbIndex = 1;
        foreach (var deleteButton in delButtons)
        {
            var index = dbIndex;
            deleteButton.onClick.AddListener((() => DeleteDebugSave(index)));
            if (!CheckDebugSaveExists(dbIndex))
                deleteButton.interactable = false;

            dbIndex++;
        }

        Registry.Get<SphereCatalogue>().Subscribe(this);
    }


    public void ToggleVisibleLog()
    {
        Registry.Get<Concursum>().ToggleSecretHistory();
    }

    public bool ProcessInput()
    {
        if (!autoCompletionBox.isActiveAndEnabled)
            return false;

        // If the user has right-clicked, close the suggestions box
        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
        {
            AttemptAutoCompletion(null);
            return true;
        }

        // Only process the rest when the main input field is open
        if (!input.isFocused)
            return false;

        List<AutoCompletionSuggestion> suggestions = new List<AutoCompletionSuggestion>();
        autoCompletionSuggestions.GetComponentsInChildren(suggestions);

        if (suggestions.Count == 0)
            return false;

        // Check if the user is tab-completing
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            currentAutoCompletionSuggestion = 0;
            AutoCompletionSuggestion suggestion = suggestions.First();
            SetInput(suggestion.GetText());
            input.MoveTextEnd(false);
            return true;
        }

        // Check if the user is navigating suggestions with the arrow keys
        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            // Get the next suggestion based on what was previously used
            if (currentAutoCompletionSuggestion < 0)
                currentAutoCompletionSuggestion = 0;
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                currentAutoCompletionSuggestion++;
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                currentAutoCompletionSuggestion--;

            // Fold back to beginning and end of the suggestions if we overflow
            if (currentAutoCompletionSuggestion >= suggestions.Count)
                currentAutoCompletionSuggestion = 0;
            else if (currentAutoCompletionSuggestion < 0)
                currentAutoCompletionSuggestion = suggestions.Count - 1;

            SetInput(suggestions[currentAutoCompletionSuggestion].GetText());
            input.MoveTextEnd(false);
            return true;
        }

        return false;
    }

    public void SetInput(string text)
    {
        // Do nothing if it's not open
        if (!isActiveAndEnabled || text == null)
            return;

        // Temporarily disable suggestions so that this doesn't trigger a new auto-completion attempt
        input.onValueChanged.RemoveListener(AttemptAutoCompletion);
        input.text = text;
        input.onValueChanged.AddListener(AttemptAutoCompletion);
    }

    void AttemptAutoCompletion(string value)
    {
        // Don't show the suggestion box if the field is empty
        if (string.IsNullOrEmpty(value))
        {
            autoCompletionBox.gameObject.SetActive(false);
            return;
        }
        autoCompletionBox.gameObject.SetActive(true);

        // Clear the list
        foreach (Transform child in autoCompletionSuggestions.transform)
            Destroy(child.gameObject);

        // Re-populate it with updated suggestions
        // Disable the suggestion box if there are no suggestions
        Compendium compendium = Registry.Get<Compendium>();
        List<AutoCompletionSuggestion> suggestions = GetElementAutoCompletionSuggestions(compendium, value)
            .Concat(GetRecipeAutoCompletionSuggestions(compendium, value))
            .OrderBy(acs => acs.GetText())
            .ToList();
        if (suggestions.Count == 0)
        {
            autoCompletionBox.gameObject.SetActive(false);
            return;
        }
        foreach (var suggestion in suggestions)
            suggestion.transform.SetParent(autoCompletionSuggestions.transform, false);
    }

    void ApplySuggestion(string suggestion)
    {
        SetInput(suggestion);
        autoCompletionBox.gameObject.SetActive(false);
    }

    List<AutoCompletionSuggestion> GetElementAutoCompletionSuggestions(Compendium compendium, string prompt)
    {
        return compendium.GetEntitiesAsList<Element>().
            Where(e => e.Id.StartsWith(prompt)).Select(e => MakeAutocompleteSuggestion(compendium, e.Id, true)).ToList();
    }

    List<AutoCompletionSuggestion> GetRecipeAutoCompletionSuggestions(Compendium compendium, string prompt)
    {
        return compendium.GetEntitiesAsList<Recipe>().
            Where(r => r.Id.StartsWith(prompt)).Select(r => MakeAutocompleteSuggestion(compendium, r.Id, false)).ToList();
    }

    AutoCompletionSuggestion MakeAutocompleteSuggestion(Compendium compendium, string suggestedId, bool isElement)
    {
        AutoCompletionSuggestion suggestion = Instantiate(AutoCompletionSuggestionPrefab).GetComponent<AutoCompletionSuggestion>();
        suggestion.SetText(suggestedId);
        suggestion.AddClickListener(() => ApplySuggestion(suggestedId));

        // Show the element image if applicable
        if (isElement)
            suggestion.SetIconForElement(compendium.GetEntityById<Element>(suggestedId));

        return suggestion;
    }

    void AddCard(string elementId)
    {
       
        var element = Registry.Get<Compendium>().GetEntityById<Element>(elementId);

        if (element == null) {
            Debug.LogWarning("No Element with ID " + elementId + " found!");
            return;
        }

        Context debugContext = new Context(Context.ActionSource.Debug);


        tabletop.ModifyElementQuantity(elementId,1, Source.Fresh(), debugContext);

    }

    void RemoveItem(string itemId)
    {
        tabletop.ModifyElementQuantity(itemId, -1, Source.Fresh(), new Context(Context.ActionSource.Debug));
    }

    void BeginSituation(string recipeId)
    {
        var compendium = Registry.Get<Compendium>();
        var recipe = compendium.GetEntityById<Recipe>(recipeId.Trim());
        if (recipe!=null)
        {
            IVerb verbForNewSituation = Registry.Get<Compendium>().GetVerbForRecipe(recipe);


            SituationCreationCommand scc = new SituationCreationCommand(verbForNewSituation, recipe, StateEnum.Ongoing,
                new TokenLocation(0f,0f,-100f,tabletop.GetPath()));
            scc.Open = false;
        Registry.Get<SituationsCatalogue>().TryBeginNewSituation(scc,new List<Token>());
        }
        else
            NoonUtility.LogWarning("Tried to begin situation via debug, but couldn't find this recipe: " + recipeId);
    }

    void HaltVerb(string verbId)
    {
        Registry.Get<TabletopManager>().HaltVerb(verbId, 1);

    }

    private void DeleteVerb(string verbId)
    {
        Registry.Get<TabletopManager>().DeleteVerb(verbId,1);
    }

    private void PurgeElement(string elementId)
    {
        Registry.Get<TabletopManager>().PurgeElement(elementId, 1);
    }


    void BeginLegacy(string legacyId)
    {
        var l = Registry.Get<Compendium>().GetEntityById<Legacy>(legacyId);
        if (l == null)
            return;

    }

    void TriggerAchievement(string achievementId)
    {
        var storefrontServicesProvider = Registry.Get<StorefrontServicesProvider>();
        storefrontServicesProvider.SetAchievementForCurrentStorefronts(achievementId,true);
    }

    void ResetAchievement(string achievementId)
    {
        var storefrontServicesProvider = Registry.Get<StorefrontServicesProvider>();
        storefrontServicesProvider.SetAchievementForCurrentStorefronts(achievementId, false);
    }

    void FastForward(float interval)
    {
            heart.FastForward(interval);
    }

    void UpdateCompendiumContent()
    {
        Registry.Get<ModManager>().CatalogueMods();
            
           var existingCompendium = Registry.Get<Compendium>();
           var compendiumLoader = new CompendiumLoader();

           var startImport = DateTime.Now;
           var log=compendiumLoader.PopulateCompendium(existingCompendium, Registry.Get<Config>().GetConfigValue(NoonConstants.CULTURE_SETTING_KEY));
        foreach(var m in log.GetMessages())
            Log(m.Description,m.MessageLevel);

        Log("Total time to import: " + (DateTime.Now-startImport));

        // Populate current decks with new cards (this will shuffle the deck)
        Registry.Get<Character>().ResetStartingDecks();

    }

    public void WordCount()
    {
        var compendium = Registry.Get<Compendium>();
        var log=new ContentImportLog();
        compendium.CountWords(log);
        foreach (var m in log.GetMessages())
            Log(m.Description, m.MessageLevel);

    }

    public void FnordCount()
    {
        var compendium = Registry.Get<Compendium>();
        var log = new ContentImportLog();
        compendium.LogFnords(log);
        foreach (var m in log.GetMessages())
            Log(m.Description, m.MessageLevel);

    }

    public void ImageCheck()
    {

        var compendium = Registry.Get<Compendium>();
        var log = new ContentImportLog();
        compendium.LogMissingImages(log);
        foreach (var m in log.GetMessages())
            Log(m.Description, m.MessageLevel);
    }


    void NextTrack()
    {
        backgroundMusic.PlayNextClip();
    }

    // to allow access from HotkeyWatcher
    public void EndGame(string endingId)
    {
        var compendium = Registry.Get<Compendium>();

        var ending = compendium.GetEntityById<Ending>(endingId);
        if (ending == null)
            ending = compendium.GetEntitiesAsList<Ending>().First();

        
        // Get us a random situation that killed us!
        var situationControllers = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
        
    throw new NotImplementedException();
    }

    public void LoadGame()
    {
        Registry.Get<StageHand>().LoadGameOnTabletop(SourceForGameState.DefaultSave);
    }

    public async void SaveGame()
    {
      NoonUtility.LogWarning("This doesn't work rn");
    }

    void ResetDecks()
    {
        var character= Registry.Get<Character>();
        character.ResetStartingDecks();
    }

    void QueueRoll(string roll)
    {
        int rollValue;
        int.TryParse(roll, out rollValue);
        if(rollValue>=1 && rollValue<=100)
            QueuedRollsList.Add(rollValue);

        UpdatedQueuedRollsDisplay();

    }

    public void UpdatedQueuedRollsDisplay()
    {
        rollsQueued.text = string.Empty;
        foreach(var i in QueuedRollsList)
        {
            if (rollsQueued.text!="")
                rollsQueued.text += ", ";

            rollsQueued.text += i.ToString();
        }
    }

    public int PopNextOverrideValue(Recipe recipe = null)
    {
        if (!QueuedRollsList.Any())
            return 0;
        else
        {
            int result = QueuedRollsList.First();
            QueuedRollsList.RemoveAt(0);
            UpdatedQueuedRollsDisplay();
            return result;
        }
    }

    async void  SaveDebugSave(int index)
    {
        TabletopManager tabletopManager = Registry.Get<TabletopManager>();
        var source = (SourceForGameState) index;

        NoonUtility.LogWarning("THis doesn't work rn");
      //  var task = tabletopManager.SaveGameAsync(true, source);
        //var success = await task;

        
        //    loadButtons[index-1].interactable = success;
        //    delButtons[index -1].interactable = success;
        
    }

    void LoadDebugSave(int index)
    {
        if (!CheckDebugSaveExists(index))
            return;
        SourceForGameState source = (SourceForGameState) index;
        Registry.Get<StageHand>().LoadGameOnTabletop(source);
        }

    void DeleteDebugSave(int index)
    {
        if (!CheckDebugSaveExists(index))
            return;
        File.Delete(GetGameSaveLocation(index));
        loadButtons[index-1].interactable = false;
        delButtons[index-1].interactable = false;
    }

    private bool CheckDebugSaveExists(int index)
    {
        return File.Exists(GetGameSaveLocation(index));
    }

    public void NotifyTokensChanged(TokenInteractionEventArgs args)
    {
        //
    }

    public void OnTokenInteraction(TokenInteractionEventArgs args)
    {
     if(args.Interaction==Interaction.OnClicked)
         if (args.Token.ElementStack.IsValidElementStack())
             SetInput(args.Token.Element.Id);
    }




}

