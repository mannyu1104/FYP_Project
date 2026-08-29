using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Central store of every clue the player has recorded
public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance { get; private set; }

    [Serializable]
    public class RecordedClue
    {
        public string title;
        public string summary;
        public ClueCredibility credibility;
        public CaseDefinition caseDefinition;
    }

    public CaseDefinition CurrentCase { get; set; }

    public IReadOnlyList<RecordedClue> RecordedClues => recordedClues;
    private readonly List<RecordedClue> recordedClues = new List<RecordedClue>();

    // Fire whenever a new clue is recorded
    public event Action<RecordedClue> OnClueRecorded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            PrintAllClues();
        }
    }

    public void RecordClue(string title, string summary, ClueCredibility credibility, CaseDefinition caseDefinition)
    {
        // Same clue should not be recorded twice
        foreach (RecordedClue existing in recordedClues)
        {
            if (existing.title == title && existing.summary == summary)
                return;
        }

        RecordedClue clue = new RecordedClue
        {
            title = title,
            summary = summary,
            credibility = credibility,
            caseDefinition = caseDefinition,
        };
        recordedClues.Add(clue);
        OnClueRecorded?.Invoke(clue);

        Debug.Log($"Clue recorded: {title} (case: {caseDefinition?.CaseName})");
    }

    // Call this if the player can retry the puzzle without reloading the scene.
    public void ClearAllClues()
    {
        recordedClues.Clear();
    }

    // For debugging purposes, print all recorded clues to the console
    public void PrintAllClues()
    {
        foreach (RecordedClue clue in recordedClues)
        {
            Debug.Log($"Title: {clue.title}, Summary: {clue.summary}, Credibility: {clue.credibility}, Case: {clue.caseDefinition?.CaseName}");
        }
    }
}