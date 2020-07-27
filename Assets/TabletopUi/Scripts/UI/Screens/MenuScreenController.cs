﻿// based on LoadingScreenManager
// --------------------------------
// built by Martin Nerurkar (http://www.martin.nerurkar.de)
// for Nowhere Prophet (http://www.noprophet.com)
//
// Licensed under GNU General Public License v3.0
// http://www.gnu.org/licenses/gpl-3.0.txt

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Core.Entities;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
#if MODS
using Assets.TabletopUi.Scripts.Infrastructure.Modding;    
#endif
using Noon;
using TabletopUi.Scripts.Services;
using TabletopUi.Scripts.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuScreenController : MonoBehaviour {

    // can be used to disable interaction when we start loading into a scene
    public EventSystem eventSystem;

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueGameButton;
    public Button purgeButton;
    public Button modsButton;
    public Button languageButton;

    [Header("Overlays")]
    public CanvasGroupFader modal;
    public CanvasGroupFader purgeConfirm;
	public CanvasGroupFader credits;
	public CanvasGroupFader settings;
	public CanvasGroupFader language;
    public CanvasGroupFader versionHints;
    public CanvasGroupFader modsPanel;
    public CanvasGroupFader startDLCLegacyConfirmPanel;

	public OptionsPanel optionsPanel;

    [Header("Version News")]

    [Header("Fade Visuals")]
    public Image fadeOverlay;
    public float fadeDuration = 0.25f;

    [Header("Hints")]
    public GameObject purgeSaveMessage;
    public TextMeshProUGUI VersionNumber;
    public Animation versionAnim;
   

    public MenuSubtitle Subtitle;

    [Header("DLC & Mods)")]
    public Transform dlcEntries;
    public MenuDlcEntry dlcEntryPrefab;
    public GameObject dlcUnavailableLabel;
    public GameObject modEntryPrefab;
    public TextMeshProUGUI modEmptyMessage;
    public Transform modEntries;

    [Header("Localisation")]
    public Button russianLanguageButton;

    bool canTakeInput;
    int sceneToLoad;
	private string cultureContentLoaded = "none";	// Used to track which culture we have got loaded. If language changes on the Menu screen, we must re-import the content.
    VersionNumber currentVersion;
    CanvasGroupFader currentOverlay;

    GameSaveManager saveGameManager;

#if MODS    
    private ModManager _modManager;
#endif


    private static readonly MenuDlcEntry.Spec[] DlcEntrySpecs =
    {
        new MenuDlcEntry.Spec(
            "DANCER",
            new Dictionary<Storefront, string>
            {
                {Storefront.Steam, "https://store.steampowered.com/app/871650/Cultist_Simulator_The_Dancer/"},
                {Storefront.Gog, "https://www.gog.com/game/cultist_simulator_the_dancer"},
                {Storefront.Humble, "https://www.humblebundle.com/store/cultist-simulator-the-dancer"},
                {Storefront.Unknown,"https://www.cultistsimulator.com" }
            },
            true),
        new MenuDlcEntry.Spec(
            "PRIEST",
            new Dictionary<Storefront, string>
            {
                {Storefront.Steam, "https://store.steampowered.com/app/871651/Cultist_Simulator_The_Priest/"},
                {Storefront.Gog, "https://www.gog.com/game/cultist_simulator_the_priest"},
                {Storefront.Humble, "https://www.humblebundle.com/store/cultist-simulator-the-priest"},
                {Storefront.Unknown,"https://www.cultistsimulator.com" }
            },
            true),
        new MenuDlcEntry.Spec(
            "GHOUL",
            new Dictionary<Storefront, string>
            {
                {Storefront.Steam, "https://store.steampowered.com/app/871900/Cultist_Simulator_The_Ghoul/"},
                {Storefront.Gog, "https://www.gog.com/game/cultist_simulator_the_ghoul"},
                {Storefront.Humble, "https://www.humblebundle.com/store/cultist-simulator-the-ghoul"},
                {Storefront.Unknown,"https://www.cultistsimulator.com" }
            },
            true)
        ,
        new MenuDlcEntry.Spec(
            "EXILE",
            new Dictionary<Storefront, string>
            {
                {Storefront.Steam, "https://weatherfactory.biz/mar-1-edmund/"},
                {Storefront.Gog, "https://weatherfactory.biz/mar-1-edmund/"},
                {Storefront.Humble, "https://weatherfactory.biz/mar-1-edmund/"},
                {Storefront.Unknown, "https://weatherfactory.biz/mar-1-edmund/" }
            },
            false
            )
    };
    private static readonly MenuDlcEntry.Spec OstSpec = new MenuDlcEntry.Spec(
        "OST",
        new Dictionary<Storefront, string>
        {
            {Storefront.Steam, "https://store.steampowered.com/app/988320/Cultist_Simulator_Original_Soundtrack/"},
            {Storefront.Gog, "https://www.gog.com/game/cultist_simulator_original_soundtrack"},
            {Storefront.Humble, "https://www.humblebundle.com/store/cultist-simulator-original-soundtrack"},
        },
        true);
    
    void Start() {
        // make sure the screen is black
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.canvasRenderer.SetAlpha(1f);




        InitialiseServices();
		canTakeInput = false; // The UpdateAndShowMenu reenables the input

        // We delay the showing to get a proper fade in
        Invoke("UpdateAndShowMenu", 0.1f); 

    }

    private static void SetEditionStatus()
    {
        string perpetualEditionDumbfileLocation = Application.streamingAssetsPath + "/edition/semper.txt";
        if (File.Exists(perpetualEditionDumbfileLocation))
            NoonUtility.PerpetualEdition = true;
    }

    private static Storefront GetCurrentStorefront()
    {
        var storeFilePath = Path.Combine(Application.streamingAssetsPath, NoonConstants.STOREFRONT_PATH_IN_STREAMINGASSETS);
        if (!File.Exists(storeFilePath))
        {
            return Storefront.Unknown;
        }

        var edition = File.ReadAllText(storeFilePath).Trim().ToUpper();
        switch (edition)
        {
            case "STEAM":
                return Storefront.Steam;
            case "GOG":
                return Storefront.Gog;
            case "HUMBLE":
                return Storefront.Humble;
            case "ITCH":
                return Storefront.Itch;
            default:
                return Storefront.Unknown;
        }
    }

    void InitialiseServices()
	{
        var registry = new Registry();

#if MODS
        _modManager = new ModManager(true);
        _modManager.LoadAll();
        registry.Register(_modManager);
        BuildDlcPanel();
        BuildModsPanel();
#else
        modsButton.enabled = false;
#endif

		InitialiseContent();	// Moved content into its own function, so it can happen again after language select if necessary

        var metaInfo = new MetaInfo(new VersionNumber(Application.version));
        registry.Register<MetaInfo>(metaInfo);
        CrossSceneState.SetMetaInfo(metaInfo);

		optionsPanel.InitPreferences(null,false);

        saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());

        currentVersion = metaInfo.VersionNumber;
    }

	void InitialiseContent()
	{
		if (cultureContentLoaded.CompareTo(LanguageTable.targetCulture) == 0)	// Bail if we have the correct culture loaded already
		{
			return;
		}

		var registry = new Registry();

		var compendium = new Compendium();
		registry.Register<ICompendium>(compendium);

		var contentImporter = new CompendiumLoader();
		contentImporter.PopulateCompendium(compendium);

		cultureContentLoaded = LanguageTable.targetCulture;
	}

    void UpdateAndShowMenu() {
        bool hasSavegame = saveGameManager.DoesGameSaveExist();
        bool isLegalSaveGame = hasSavegame ? saveGameManager.SaveGameHasMatchingVersionNumber(currentVersion) : false;

        // Show the buttons as needed
        newGameButton.gameObject.SetActive(!hasSavegame);
        continueGameButton.gameObject.SetActive(hasSavegame);
        continueGameButton.interactable = isLegalSaveGame;
        purgeButton.gameObject.SetActive(hasSavegame);

        //update subtitle text
        SetEditionStatus();
        if (NoonUtility.PerpetualEdition)
        {
            Subtitle.SetText("UI_PERPETUAL_EDITION");
        }
        else
		{
            Subtitle.SetText("UI_BRING_THE_DAWN");
		}

        // Show the purge message if needed
        purgeSaveMessage.gameObject.SetActive(hasSavegame && !isLegalSaveGame);
        UpdateVersionNumber(!isLegalSaveGame);
        HideAllOverlays();
        FadeIn();

		// now we can take input
		canTakeInput = true;
    }

    void UpdateVersionNumber(bool hasNews) {
        // Show the current version number
        VersionNumber.text = currentVersion.Version;

        if (hasNews)
            versionAnim.Play();
        else
            versionAnim.Stop();
    }

    void HideAllOverlays() {
        modal.gameObject.SetActive(false);
        purgeConfirm.gameObject.SetActive(false);
        credits.gameObject.SetActive(false);
        versionHints.gameObject.SetActive(false);
    }

#region -- View Changes ------------------------

    void FadeIn() {
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.canvasRenderer.SetAlpha(1f);
        fadeOverlay.CrossFadeAlpha(0, fadeDuration, true);
    }

    void FadeOut() {
        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.canvasRenderer.SetAlpha(0f);
        fadeOverlay.CrossFadeAlpha(1, fadeDuration, true);
    }

    void ShowOverlay(CanvasGroupFader overlay) {
		if (currentOverlay != null)
            return;

        currentOverlay = overlay;

        overlay.Show();
        modal.Show();
    }

    void HideCurrentOverlay() {
        if (currentOverlay == null)
            return;

        currentOverlay.Hide();
        modal.Hide();
        currentOverlay = null;
    }

#endregion

#region -- User Actions via Scene Buttons ------------------------

    public void StartGame()
	{
        if (!canTakeInput)
            return;
		
        // Set the legacy to the first in the list; this should be the starting legacy
        CrossSceneState.SetChosenLegacy(Registry.Retrieve<ICompendium>().GetEntitiesAsList<Legacy>().First());
        // Load directly into the game scene, no legacy select
        LoadScene(SceneNumber.GameScene);
    }

    public void ContinueGame()
	{
        if (!canTakeInput)
            return;
		
        if (saveGameManager.IsSavedGameActive()) {
            //back into the game!
            LoadScene(SceneNumber.GameScene);
            return;
        }

        //we left the game from the game over or legacy screen: go back to choose a legacy.
        //Retrieve the saved state, and then selectively populate properties in the current state...
        //this is ugly: it could use appropriate methods
        var savedCrossSceneState = saveGameManager.RetrieveSavedCrossSceneState();

        //this is essential. If we earlier left the game in the legacy screen, we're about to go back there, 
        //and we need to retrieve the legacy ids and populate with compendium data
        if (savedCrossSceneState.AvailableLegacies.Count > 0)
            CrossSceneState.SetAvailableLegacies(savedCrossSceneState.AvailableLegacies);

        //this is currently unnecessary: we don't go back to the game over screen. but it's very likely we might want to track / restore this information.
        if (savedCrossSceneState.CurrentEnding != null)
            CrossSceneState.SetCurrentEnding(savedCrossSceneState.CurrentEnding);

        if (savedCrossSceneState.DefunctCharacter != null)
            CrossSceneState.SetDefunctCharacter(savedCrossSceneState.DefunctCharacter);

        LoadScene(SceneNumber.NewGameScene);
    }

    void LoadScene(int sceneNr) {		
        canTakeInput = false;
        sceneToLoad = sceneNr;

		if (sceneToLoad == SceneNumber.GameScene)
			SoundManager.PlaySfx("UIStartGame");

        FadeOut();
        Invoke("LoadSceneDelayed", fadeDuration);
    }

    void LoadSceneDelayed() {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void TryPurgeSave() {
        if (!canTakeInput)
            return;

        ShowOverlay(purgeConfirm);
    }

    public void PurgeSave() {
        if (!canTakeInput)
            return;

        saveGameManager.DeleteCurrentSave();
        currentOverlay = null; // to ensure we can re-open another overlay afterwards
        FadeOut();
        Invoke("UpdateAndShowMenu", fadeDuration);
    }

    public void BeginNewGameWithSpecifiedLegacyAndPurgeOldSave(string legacyId)
    {
        saveGameManager.DeleteCurrentSave();
        CrossSceneState.SetChosenLegacy(Registry.Retrieve<ICompendium>().GetEntityById<Legacy>(legacyId));
        // Load directly into the game scene, no legacy select
        LoadScene(SceneNumber.GameScene);
    }

    public void ShowCredits() {
        if (!canTakeInput)
            return;

        ShowOverlay(credits);
	}

	public void ShowSettings() {
		if (!canTakeInput)
			return;

		ShowOverlay(settings);
	}

	public void ShowLanguage()
	{
		if (!canTakeInput)
			return;
		
		ShowOverlay(language);
	}

	public void SetLanguage( string lang_code )
	{
		if (!canTakeInput)
			return;

		if (LanguageManager.Instance)
		{		
			LanguageManager.Instance.SetLanguage( lang_code );
		}

		HideCurrentOverlay();
	}

    public void ShowVersionHints() {
        if (!canTakeInput)
            return;

        ShowOverlay(versionHints);
        versionAnim.Stop(); // Ensure that the anim is no longer playing
    }


    public void ShowModsPanel()
    {
#if MODS
        if (!canTakeInput)
            return;
        
        ShowOverlay(modsPanel);
#endif
    }

    public void ShowStartDLCLegacyConfirmPanel(string legacyId)
    {
        HideCurrentOverlay();
        StartDLCLegacyConfirm confirmPanelComponent=startDLCLegacyConfirmPanel.GetComponent<StartDLCLegacyConfirm>();
        confirmPanelComponent.LegacyId = legacyId;
        confirmPanelComponent.msc = this;

        ShowOverlay(startDLCLegacyConfirmPanel);
        
    }

    private void BuildDlcPanel()
    {
        var dlc = new HashSet<string>(CompendiumLoader.GetInstalledDlc());
        var store = GetCurrentStorefront();
        var hasAnyDlc = false;
        foreach (var dlcEntrySpec in DlcEntrySpecs)
        {
            var dlcEntry = Instantiate(dlcEntryPrefab, dlcEntries);
            var hasDlc = dlc.Contains(dlcEntrySpec.Id);
            dlcEntry.Initialize(dlcEntrySpec, store, hasDlc,this);
            hasAnyDlc |= hasDlc;
        }

        dlcUnavailableLabel.SetActive(store == Storefront.Itch && !hasAnyDlc);
    }
    
#if MODS
    private void BuildModsPanel()
    {
        // Clear the list and repopulate with one entry per loaded mod
        foreach (Transform modEntry in modEntries)
            Destroy(modEntry);
        
        foreach (var mod in _modManager.Mods.Values)
        {
            var modEntry = Instantiate(modEntryPrefab).GetComponent<ModEntry>();
            modEntry.transform.SetParent(modEntries, false);
            modEntry.Initialize(mod);
        }
        modEmptyMessage.enabled = !_modManager.Mods.Any();
    }
#endif
    
    public void CloseCurrentOverlay() {
        if (!canTakeInput)
            return;

        HideCurrentOverlay();
    }

    public void Exit() {
        if (!canTakeInput)
            return;

        Application.Quit();
    }

    public void BrowseToSaves()
    {
        if (!canTakeInput)
            return;
        
        optionsPanel.BrowseFiles();
    }

    public void ShowPromo()
    {
        SoundManager.PlaySfx("UIButtonClick");
        Application.OpenURL(
            "https://weatherfactory.us13.list-manage.com/subscribe?u=97d06a3faac1573fa4330bb7d&id=c3f9b32720");
    }

#endregion
}
