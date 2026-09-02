using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Opens the computer canvas when the assigned object is clicked.
/// </summary>
public class ComputerAccessPoint : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ComputerCanvasController computerCanvasController;
    [SerializeField] private bool openOnMouseDown = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenComputer();
        eventData.Use();
    }

    private void OnMouseDown()
    {
        if (openOnMouseDown)
        {
            OpenComputer();
        }
    }

    public void OpenComputer()
    {
        if (computerCanvasController == null)
        {
            computerCanvasController = FindAnyObjectByType<ComputerCanvasController>();
        }

        if (computerCanvasController == null)
        {
            Debug.LogWarning("ComputerAccessPoint: ComputerCanvasController was not found.", this);
            return;
        }

        computerCanvasController.OpenComputer();
    }
}
