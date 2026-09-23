using UnityEngine;
using UnityEngine.UI;

public class OpenCanvasButton : MonoBehaviour
{
    [SerializeField] CanvasGroup InGameInventoryCanvas;
    [SerializeField] CanvasGroup TutorialInventoryCanvas;
    [SerializeField] CanvasGroup NotebookCanvas;
    public CanvasGroup[] Panels => new[] { InGameInventoryCanvas, TutorialInventoryCanvas, NotebookCanvas };
    public bool IsNotebookOpen => NotebookCanvas != null && NotebookCanvas.gameObject.activeInHierarchy && NotebookCanvas.alpha > .01f;
    public bool OwnsShortcut(GameObject button) => NotebookCanvas != null && button.transform.IsChildOf(NotebookCanvas.transform);
    public bool IsAnyOpen
    {
        get { foreach (var panel in Panels) if (panel != null && panel.gameObject.activeInHierarchy && panel.alpha > .01f) return true; return false; }
    }
    void Awake()
    {
        foreach (var panel in Panels)
        {
            if (panel == null) continue;
            var canvas = panel.GetComponent<Canvas>();
            if (canvas == null) canvas = panel.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true; canvas.sortingOrder = 2001;
            if (panel.GetComponent<GraphicRaycaster>() == null) panel.gameObject.AddComponent<GraphicRaycaster>();
        }
        CloseAll();
    }
    private static void SetVisible(CanvasGroup panel, bool visible)
    {
        if (panel == null) return;
        if (visible) panel.gameObject.SetActive(true);
        panel.alpha = visible ? 1f : 0f;
        panel.interactable = visible; panel.blocksRaycasts = visible;
    }
    private void Open(CanvasGroup panel)
    {
        // AllNoteBook is the shared parent, not a third sibling page.
        SetVisible(InGameInventoryCanvas, false);
        SetVisible(TutorialInventoryCanvas, false);
        SetVisible(NotebookCanvas, true);
        if (panel != NotebookCanvas) SetVisible(panel, true);
        foreach (var item in FindObjectsByType<DragableItem>(FindObjectsInactive.Include))
            if (item.DesUI != null) item.DesUI.SetActive(false);
    }
    public void CloseAll()
    {
        foreach (var panel in Panels) SetVisible(panel, false);
        foreach (var item in FindObjectsByType<DragableItem>(FindObjectsInactive.Include))
            if (item.DesUI != null) item.DesUI.SetActive(false);
    }
    public void AvaliableNotebookCanva() => Open(NotebookCanvas);
    public void AvaliableInGameCanva() => Open(InGameInventoryCanvas);
    public void AvaliableTutorialCanva() => Open(TutorialInventoryCanvas);
    public void DisablingNotebookCanva() => SetVisible(NotebookCanvas, false);
    public void DisablingInGameCanva() => SetVisible(InGameInventoryCanvas, false);
    public void DisablingTutorialCanva() => SetVisible(TutorialInventoryCanvas, false);
}
