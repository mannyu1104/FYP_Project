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
    private readonly HashSet<ClueManager.RecordedClue> spawnedClues = new HashSet<ClueManager.RecordedClue>();

    // Read by ClueSubmission when the player hits Submit.
    public IReadOnlyList<ClueBoardEntryUI> Entries => entries;

    private ClueSubmission submission;
    private ClueSubmission Submission
    {
        get
        {
            if (submission == null)
                foreach (ClueSubmission candidate in FindObjectsByType<ClueSubmission>(FindObjectsInactive.Include))
                    if (candidate.Board == this) { submission = candidate; break; }
            return submission;
        }
    }

    private void Start() => OnEnable();

    private void OnEnable()
    {
        // Only spawn clues we haven't seen before. This is deliberate: if the
        // player already dragged some clues into the judgment zone and then
        // leaves to question another witness, coming back here must NOT reset
        // their sorting progress.
        if (ClueManager.Instance == null) return;
        SpawnAnyNewClues();
        ClueManager.Instance.OnClueRecorded -= HandleClueRecorded;
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
            if (spawnedClues.Contains(clue))
            {
                continue;
            }
            AddEntry(clue);
        }
    }

    public void RefreshEntries()
    {
        if (ClueManager.Instance != null) SpawnAnyNewClues();
    }

    public void ClearEntries()
    {
        foreach (ClueBoardEntryUI entry in entries)
        {
            if (entry == null) continue;
            entry.gameObject.SetActive(false);
            Destroy(entry.gameObject);
        }
        entries.Clear();
        spawnedClues.Clear();
    }

    public bool AcceptsDestination(Transform destination)
    {
        return destination == unsortedContent ||
            (Submission != null && destination == Submission.JudgmentContent);
    }

    public void RecordPlacement(ClueBoardEntryUI entry)
    {
        ClueSubmission submission = Submission;
        if (entry != null && entries.Contains(entry) && submission != null)
            entry.SetJudgmentPlacement(entry.transform.parent == submission.JudgmentContent);
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

        spawnedClues.Add(clue);
        ClueBoardEntryUI entry = Instantiate(entryPrefab, unsortedContent);
        entry.Set(clue);
        entry.Board = this;
        entries.Add(entry);
        ClueSubmission submission = Submission;
        if (clue.inJudgmentZone && submission != null && submission.JudgmentContent != null)
            entry.transform.SetParent(submission.JudgmentContent, false);
    }
}
