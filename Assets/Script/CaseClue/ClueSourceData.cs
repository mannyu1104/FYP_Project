using System;
using UnityEngine;
using UnityEngine.Localization;

public enum ClueCredibility
{
    Neutral,     
    Credible,    
    NotCredible, 
}

// Base class for any ScriptableObject that can be recorded as important clue
public abstract class ClueSourceData : ScriptableObject
{
    [Header("Clue Details")]
    [SerializeField] private LocalizedString clueTitle;
    [SerializeField] private LocalizedString clueSummary;
    [SerializeField] private ClueCredibility clueCredibility = ClueCredibility.Neutral; // Default to Neutral if not specified
    [SerializeField] private CaseDefinition caseDefinition;

    public LocalizedString ClueTitle => clueTitle;
    public LocalizedString ClueSummary => clueSummary;
    public ClueCredibility Credibility => clueCredibility;
    public CaseDefinition Case => caseDefinition;
}