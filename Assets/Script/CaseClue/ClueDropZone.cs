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
        if (eventData == null || eventData.pointerDrag == null)
        {
            return;
        }

        GameObject dropped = eventData.pointerDrag;
        DraggableClueEntry draggable = dropped.GetComponent<DraggableClueEntry>() ?? dropped.GetComponentInParent<DraggableClueEntry>();
        if (draggable == null)
        {
            return; // something else was being dragged, ignore it
        }

        ClueBoardEntryUI entry = dropped.GetComponent<ClueBoardEntryUI>() ?? dropped.GetComponentInParent<ClueBoardEntryUI>();
        ClueBoardUI board = entry != null ? entry.Board : null;
        if (board == null)
        {
            return;
        }

        Transform targetParent = contentParent != null ? contentParent : transform;
        if (!board.AcceptsDestination(targetParent))
        {
            return;
        }

        dropped.transform.SetParent(targetParent, false);
        dropped.transform.SetAsLastSibling();
        board.RecordPlacement(entry);
    }
}
