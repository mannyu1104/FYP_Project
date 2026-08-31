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

    private ClueManager.RecordedClue clueData;

    public void Set(ClueManager.RecordedClue clue)
    {
        UnsubscribeFromLocalization();
        clueData = clue;

        SubscribeToLocalization();

        Credibility = clue.credibility;
    }

    private void SubscribeToLocalization()
    {
        if (clueData == null) return;
        clueData.title.StringChanged += UpdateTitleText;
        clueData.summary.StringChanged += UpdateSummaryText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (clueData == null) return;
        clueData.title.StringChanged -= UpdateTitleText;
        clueData.summary.StringChanged -= UpdateSummaryText;
    }

    private void UpdateTitleText(string value)
    {
        if (titleText != null) titleText.text = value;
    }

    private void UpdateSummaryText(string value)
    {
        if (summaryText != null) summaryText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}