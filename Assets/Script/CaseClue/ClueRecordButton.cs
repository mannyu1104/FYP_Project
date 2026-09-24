using UnityEngine;

// "Record as Important Clue" button.
public class ClueRecordButton : MonoBehaviour
{
    [SerializeField] private CustomButtonUi customButton;
    [SerializeField] private ClueSourceData source; // Optional or can be set via SetSource() if not assigned in inspector.

    private void Reset()
    {
        customButton = GetComponent<CustomButtonUi>();
    }

    private void Awake()
    {
        if (customButton == null) customButton = GetComponent<CustomButtonUi>();
        if (customButton != null) customButton.onLeftClick.AddListener(RecordClue);
    }

    public void SetSource(ClueSourceData newSource)
    {
        source = newSource;
    }

    private void RecordClue()
    {
        if (source == null)
        {
            Debug.LogWarning("RecordClueButton was clicked with no ClueSourceData assigned.", this);
            return;
        }

        if (ClueManager.Instance == null) return;
        ClueManager.Instance.RecordClue(source.ClueTitle, source.ClueSummary, source.Credibility, source.Case);
        // Inventory pages may be closed, so their event subscriptions are not sufficient.
        // Also retry already-recorded clues if their inventory slot was previously full.
        var recorded = new ClueManager.RecordedClue
        { title = source.ClueTitle, summary = source.ClueSummary, credibility = source.Credibility, caseDefinition = source.Case };
        foreach (var receiver in FindObjectsByType<SummaryButton>(FindObjectsInactive.Include))
            if (receiver.gameObject.scene == gameObject.scene) receiver.GetTitle(recorded);
    }
}