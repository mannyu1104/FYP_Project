using System;
using UnityEngine;

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
    [SerializeField] private string clueTitle;
    [SerializeField][TextArea(6, 10)] private string clueSummary;
    [SerializeField] private ClueCredibility clueCredibility = ClueCredibility.Neutral; // Default to Neutral if not specified
    [SerializeField] private CaseDefinition caseDefinition;

    public string ClueTitle => clueTitle;
    public string ClueSummary => clueSummary;
    public ClueCredibility Credibility => clueCredibility;
    public CaseDefinition Case => caseDefinition;
}