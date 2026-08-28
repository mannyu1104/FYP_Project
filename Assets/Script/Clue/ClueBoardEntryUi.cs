using UnityEngine;
using UnityEngine.UI;

// One card on the clue board. Whether it counts as "marked credible" is now
// decided by which zone it's been dragged into (see ClueSubmission), not by
// a checkbox on this component. GroundTruthIsCredible is only read at submit
// time and never shown to the player.
[RequireComponent(typeof(DraggableClueEntry))]
public class ClueBoardEntryUI : MonoBehaviour
{
    [SerializeField] private Text titleText;   // swap for TMP_Text if the project uses TextMeshPro
    [SerializeField] private Text summaryText;

    public bool GroundTruthIsCredible { get; private set; }

    public void Set(ClueManager.RecordedClue clue)
    {
        titleText.text = clue.title;
        summaryText.text = clue.summary;
        GroundTruthIsCredible = clue.isCredible;
    }
}