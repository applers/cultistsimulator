﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.Core.Services;
using JetBrains.Annotations;
using Noon;

public enum LegacyEventRecordId
{
    LastCharacterName,
    LastDesire,
    LastTool,
    LastBook,
    LastSignificantPainting,
    LastCult,
    LastHeadquarters,
    LastPersonKilled
}

public class Character:IGameEntityStorage
{
    private string _name="[unnamed]";
    
    public CharacterState State { get; set; }
    public List<IDeckInstance> DeckInstances { get; set; }
    private Dictionary<LegacyEventRecordId, string> _futureLegacyEventRecords;
    private Dictionary<LegacyEventRecordId, string> _pastLegacyEventRecords;


    private Dictionary<string, int> recipeExecutions;
    private string _endingTriggeredId = null;

    public Character() : this(null)
    {

    }

    public Character(Character previousCharacter)
    {
        State = CharacterState.Viable;
        recipeExecutions = new Dictionary<string, int>();
        DeckInstances = new List<IDeckInstance>();
        if (previousCharacter == null && _pastLegacyEventRecords==null)
        { 
            HistoryBuilder hb=new HistoryBuilder();
            _pastLegacyEventRecords = hb.SpecifyDefaultPast();
        }
        else if(previousCharacter!=null)
            _pastLegacyEventRecords = previousCharacter.GetAllFutureLegacyEventRecords(); //THEIR FUTURE IS OUR PAST
        else
        throw new ApplicationException("Previous character supplied, but past legacy event records are already in current character. Where did they come from?");

        _futureLegacyEventRecords = new Dictionary<LegacyEventRecordId, string>(_pastLegacyEventRecords);
    }

    public Dictionary<string, int> GetAllExecutions()
    {
        return new Dictionary<string, int>(recipeExecutions);
    }

    public void AddExecutionsToHistory(string forRecipeId,int executions)
    {
        if (recipeExecutions.ContainsKey(forRecipeId))
            recipeExecutions[forRecipeId]+=executions;
        else
            recipeExecutions[forRecipeId] = executions;
    }

    public int GetExecutionsCount(string forRecipeId)
    {
        if (recipeExecutions.ContainsKey(forRecipeId))
            return recipeExecutions[forRecipeId];

        return 0;
    }

    public bool HasExhaustedRecipe(Recipe forRecipe)
    {
        if (forRecipe.HasInfiniteExecutions())
            return false;

        return forRecipe.MaxExecutions <= GetExecutionsCount(forRecipe.Id);
    }

    public void ReestablishPastLegacyEventRecord(LegacyEventRecordId id, string value)
    {
        if (_pastLegacyEventRecords.ContainsKey(id))
            throw new ApplicationException(
                "Trying to overwrite the past! ReestablishPastLegacyEventRecord should only be called on character load");
        else
            _pastLegacyEventRecords.Add(id, value);
    }


    public void SetFutureLegacyEventRecord(LegacyEventRecordId id, string value)
    {
        if (_futureLegacyEventRecords.ContainsKey(id))
            _futureLegacyEventRecords[id] = value;
        else
            _futureLegacyEventRecords.Add(id, value);
    }
    public string GetFutureLegacyEventRecord(LegacyEventRecordId forId)
    {
        if (_futureLegacyEventRecords.ContainsKey(forId))
            return _futureLegacyEventRecords[forId];
        else
            return null;
    }


    public string GetPastLegacyEventRecord(LegacyEventRecordId forId)
    {
        if (_pastLegacyEventRecords.ContainsKey(forId))
            return _pastLegacyEventRecords[forId];
        else
            return null;
    }

    public IDeckInstance GetDeckInstanceById(string id)
    {
        return  DeckInstances.SingleOrDefault(d => d.Id == id);
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string Profession { get ; set; }


    public string EndingTriggeredId
    {
        get { return _endingTriggeredId; }
    }




    public Dictionary<LegacyEventRecordId, string> GetAllFutureLegacyEventRecords()
    {
        return new Dictionary<LegacyEventRecordId, string>(_futureLegacyEventRecords);
    }

    public Dictionary<LegacyEventRecordId, string> GetAllPastLegacyEventRecords()
    {
        return new Dictionary<LegacyEventRecordId,string>(_pastLegacyEventRecords);
    }
}

