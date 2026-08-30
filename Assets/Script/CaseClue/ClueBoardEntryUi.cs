using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One card on the clue board. Whether it counts as "marked credible" is now
// decided by which zone it's been dragged into (see ClueSubmission), not by
// a checkbox on this component. GroundTruthIsCredible is only read at submit
// time and never shown to the player.
[RequireComponent(typeof(DraggableClueEntry))]
public class ClueBoardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;   
    [SerializeField] private TMP_Text summaryText;

    public ClueCredibility Credibility { get; private set; }

    public void Set(ClueManager.RecordedClue clue)
    {
        titleText.text = clue.title;
        summaryText.text = clue.summary;
        Credibility = clue.credibility;
    }
}