﻿using UnityEngine;
using System.Collections;

public class Heartbeat : BoardMonoBehaviour,ICharacterInfoSubscriber
{
    [SerializeField] private EndingPanel pnlEnding;

    private const string DO="Do"; //so we don't get a tiny daft typo with the Invoke
    public Character Character;
    public Character CreateBlankCharacter()
    {
        Character character = new Character();
        character.Subscribe(BM.characterNamePanel);
        character.Subscribe(this);
        character.SubscribeElementQuantityDisplay(BM);
        character.Title = "Mr";
        character.FirstName = "Vivian";
        character.LastName = "Keyes";

        character.ModifyElementQuantity("clique", 1);
        character.ModifyElementQuantity("ordinarylife", 1);
        character.ModifyElementQuantity("suitablepremises", 1);
        // character.ModifyElementQuantity("health", 3);
        character.ModifyElementQuantity("reason", 3);
        character.ModifyElementQuantity("health", 1);
        character.ModifyElementQuantity("occultscrap", 1);
      //  character.ModifyElementQuantity("shilling", 10);

        return character;
    }

  
    void Start () {
        ContentRepository.Instance.ImportVerbs();
        ContentRepository.Instance.ImportElements();
        ContentRepository.Instance.ImportRecipes();

        foreach (Verb v in ContentRepository.Instance.GetAllVerbs())
        {
            BM.AddVerbToBoard(v);
        }

        NewGame();

    }

    void BeginHeartbeat()
    {
        InvokeRepeating(DO, 0, 1);
    }

    void PauseHeartbeat()
    {
        CancelInvoke(DO);
    }

    void Do()
    {
        if(Character.State==CharacterState.Viable)
        BM.DoHeartbeat(Character);
    }

    public void ReceiveUpdate(Character character)
    {
        if (character.State == CharacterState.Extinct)
        {
      EndGame();
        }
    }

    public void EndGame()
    {
        PauseHeartbeat();
        BM.gameObject.SetActive(false);

        pnlEnding.gameObject.SetActive(true);
        pnlEnding.DetailText.text = Character.EndingTriggeredId;
    }

    public void NewGame()
    {
        pnlEnding.gameObject.SetActive(false);
        BM.gameObject.SetActive(true);

        BM.ClearBoard();

        BM.QueueRecipe(ContentRepository.Instance.RecipeCompendium.GetRecipeById("starvation"));

        BeginHeartbeat();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
