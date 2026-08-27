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
    }

    public IReadOnlyList<RecordedClue> RecordedClues => recordedClues;
    private readonly List<RecordedClue> recordedClues = new List<RecordedClue>();

    // Fired whenever a new clue is recorded
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

    public void RecordClue(string title, string summary)
    {
        // Clicking the button twice on the same source (or on two different
        // sources that happen to share the same clue text) should not
        // create a duplicate entry on the clue board.
        foreach (RecordedClue existing in recordedClues)
        {
            if (existing.title == title && existing.summary == summary)
                return;
        }

        RecordedClue clue = new RecordedClue { title = title, summary = summary };
        recordedClues.Add(clue);
        OnClueRecorded?.Invoke(clue);
    }

    public void PrintAllClues()
    {
        foreach (RecordedClue clue in recordedClues)
        {
            Debug.Log($"Title: {clue.title}, Summary: {clue.summary}");
        }
    }
}