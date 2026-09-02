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
        customButton.onLeftClick.AddListener(RecordClue);
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

        ClueManager.Instance.RecordClue(source.ClueTitle, source.ClueSummary, source.Credibility, source.Case);
    }
}