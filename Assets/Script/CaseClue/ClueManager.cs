using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

// Central store of every clue the player has recorded
public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance { get; private set; }

    [Serializable]
    public class RecordedClue
    {
        public LocalizedString title;
        public LocalizedString summary;
        public ClueCredibility credibility;
        public CaseDefinition caseDefinition;
        public bool inJudgmentZone;
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
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            PrintAllClues();
        }
    }

    public void RecordClue(LocalizedString title, LocalizedString summary, ClueCredibility credibility, CaseDefinition caseDefinition)
    {
        // Same clue should not be recorded twice
        foreach (RecordedClue existing in recordedClues)
        {
            if (IsSameEntry(existing.title, title) && IsSameEntry(existing.summary, summary))
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

        Debug.Log($"Clue recorded: {title.GetLocalizedString()} (case: {caseDefinition?.CaseName})");
    }

    private bool IsSameEntry(LocalizedString a, LocalizedString b)
    {
        if (a == null || b == null) return a == b;
        return a.TableReference.Equals(b.TableReference) && a.TableEntryReference.Equals(b.TableEntryReference);
    }

    // Call this if the player can retry the puzzle without reloading the scene.
    public void ClearAllClues()
    {
        foreach (ClueBoardUI board in FindObjectsByType<ClueBoardUI>(FindObjectsInactive.Include))
            board.ClearEntries();
        recordedClues.Clear();
    }

    [Serializable]
    private class SavedClue
    {
        public LocalizedString title;
        public LocalizedString summary;
        public ClueCredibility credibility;
        public string caseId;
        public bool inJudgmentZone;
    }

    [Serializable]
    private class ClueSaveData
    {
        public List<SavedClue> clues = new List<SavedClue>();
    }

    public void SaveClues()
    {
        if (MainMenuController.IsStartingNewGame) return;
        ClueSaveData data = new ClueSaveData();
        foreach (RecordedClue clue in recordedClues)
        {
            data.clues.Add(new SavedClue
            {
                title = clue.title,
                summary = clue.summary,
                credibility = clue.credibility,
                caseId = clue.caseDefinition != null ? clue.caseDefinition.name : string.Empty,
                inJudgmentZone = clue.inJudgmentZone
            });
        }
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "clues.json"), JsonUtility.ToJson(data, true));
    }

    public void LoadClues()
    {
        string path = Path.Combine(Application.persistentDataPath, "clues.json");
        ClueSaveData data = File.Exists(path)
            ? JsonUtility.FromJson<ClueSaveData>(File.ReadAllText(path))
            : new ClueSaveData();
        if (data == null || data.clues == null) return;

        ClearAllClues();
        CaseDefinition[] cases = Resources.FindObjectsOfTypeAll<CaseDefinition>();
        foreach (SavedClue saved in data.clues)
        {
            CaseDefinition definition = Array.Find(cases, candidate => candidate.name == saved.caseId);
            if (definition == null && !string.IsNullOrEmpty(saved.caseId))
            {
                Debug.LogWarning("Cannot restore clue for missing case: " + saved.caseId, this);
                continue;
            }
            recordedClues.Add(new RecordedClue
            {
                title = saved.title,
                summary = saved.summary,
                credibility = saved.credibility,
                caseDefinition = definition,
                inJudgmentZone = saved.inJudgmentZone
            });
        }
        foreach (ClueBoardUI board in FindObjectsByType<ClueBoardUI>(FindObjectsInactive.Include))
            if (board.isActiveAndEnabled) board.RefreshEntries();
    }

    // For debugging purposes, print all recorded clues to the console
    public void PrintAllClues()
    {
        foreach (RecordedClue clue in recordedClues)
        {
            Debug.Log($"Title: {clue.title.GetLocalizedString()}, Summary: {clue.summary.GetLocalizedString()}, Credibility: {clue.credibility}, Case: {clue.caseDefinition?.CaseName}");
        }
    }
}
