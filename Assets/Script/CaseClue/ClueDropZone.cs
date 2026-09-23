using UnityEngine;
using UnityEngine.EventSystems;

public class ClueDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform contentParent;

    private void Reset()
    {
        contentParent = transform;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null)
        {
            return;
        }

        if (dropped.GetComponent<DraggableClueEntry>() == null)
        {
            return; // something else was being dragged, ignore it
        }

        ClueBoardEntryUI entry = dropped.GetComponent<ClueBoardEntryUI>();
        ClueBoardUI board = entry != null ? entry.Board : null;
        if (board == null || !board.AcceptsDestination(contentParent)) return;
        dropped.transform.SetParent(contentParent);
        dropped.transform.SetAsLastSibling();
        board.RecordPlacement(entry);
    }
}
