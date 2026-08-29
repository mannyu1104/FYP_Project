using System.Collections.Generic;
using UnityEngine;

// Attach this to the Clue Board panel (the parent that has both the
// "未整理线索区" and "判定区" as children, each with a ClueDropZone).
public class ClueBoardUI : MonoBehaviour
{
    [SerializeField] private ClueBoardEntryUI entryPrefab;
    [SerializeField] private CaseDefinition caseDefinition;
    [SerializeField] private Transform unsortedContent; // 未整理线索区's content container

    private readonly List<ClueBoardEntryUI> entries = new List<ClueBoardEntryUI>();
    private readonly HashSet<string> spawnedClueKeys = new HashSet<string>();

    // Read by ClueSubmission when the player hits Submit.
    public IReadOnlyList<ClueBoardEntryUI> Entries => entries;

    private void OnEnable()
    {
        // Only spawn clues we haven't seen before. This is deliberate: if the
        // player already dragged some clues into the judgment zone and then
        // leaves to question another witness, coming back here must NOT reset
        // their sorting progress.
        SpawnAnyNewClues();
        ClueManager.Instance.OnClueRecorded += HandleClueRecorded;
    }

    private void OnDisable()
    {
        if (ClueManager.Instance != null)
        {
            ClueManager.Instance.OnClueRecorded -= HandleClueRecorded;
        }
    }

    private void SpawnAnyNewClues()
    {
        foreach (ClueManager.RecordedClue clue in ClueManager.Instance.RecordedClues)
        {
            if (spawnedClueKeys.Contains(clue.title))
            {
                continue;
            }
            AddEntry(clue);
        }
    }

    private void HandleClueRecorded(ClueManager.RecordedClue clue)
    {
        AddEntry(clue);
    }

    private void AddEntry(ClueManager.RecordedClue clue)
    {
        if (clue.caseDefinition != caseDefinition)
        {
            return;
        }

        spawnedClueKeys.Add(clue.title);
        ClueBoardEntryUI entry = Instantiate(entryPrefab, unsortedContent);
        entry.Set(clue);
        entries.Add(entry);
    }
}