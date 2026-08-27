using UnityEngine;

// "Record as Important Clue" button.
[RequireComponent(typeof(CustomButtonUi))]
public class ClueRecordButton : MonoBehaviour
{
    [SerializeField] private CustomButtonUi clickable;
    [SerializeField] private ClueSourceData source; // Optional or can be set via SetSource() if not assigned in inspector.

    private void Reset()
    {
        clickable = GetComponent<CustomButtonUi>();
    }

    private void Awake()
    {
        clickable.onLeftClick.AddListener(RecordClue);
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

        ClueManager.Instance.RecordClue(source.ClueTitle, source.ClueSummary);
    }
}