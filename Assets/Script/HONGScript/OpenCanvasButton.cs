using UnityEngine;

public class OpenCanvasButton : MonoBehaviour
{
    //private bool MapOpen;
    [SerializeField] CanvasGroup InGameInventoryCanvas;
    [SerializeField] CanvasGroup TutorialInventoryCanvas;
    [SerializeField] CanvasGroup NotebookCanvas;
    [SerializeField] CanvasGroup ScoreShowing;

    private static bool IsVisible(CanvasGroup panel)
    {
        if (panel == null || !panel.gameObject.activeInHierarchy) return false;
        foreach (var group in panel.GetComponentsInParent<CanvasGroup>())
            if (group.alpha <= .01f) return false;
        return true;
    }
    public bool IsNotebookOpen => IsVisible(NotebookCanvas);
    public bool IsAnyOpen => IsNotebookOpen || IsVisible(InGameInventoryCanvas) ||
        IsVisible(TutorialInventoryCanvas) || IsVisible(ScoreShowing);
    public bool OwnsShortcut(GameObject button) => NotebookCanvas != null && button != null &&
        button.transform.IsChildOf(NotebookCanvas.transform);
    public void CloseAll()
    {
        if (IsNotebookOpen) NotesGrabber.Instance?.FlushForSlot();
        foreach (var panel in new[] { InGameInventoryCanvas, TutorialInventoryCanvas, NotebookCanvas, ScoreShowing })
        {
            if (panel == null) continue;
            panel.alpha = 0f; panel.interactable = false; panel.blocksRaycasts = false;
        }
        foreach (var item in FindObjectsByType<DragableItem>(FindObjectsInactive.Include))
            if (item.DesUI != null) item.DesUI.SetActive(false);
    }

    // Important: do not force the panel state here. The scene and explicit user actions should control
    // visibility; changing alpha in Start overrides the intended scene setup and can make the UI appear
    // or disappear unexpectedly at runtime.
    void Start()
    {
        // Keep the notebook above the whiteboard, including its outside-click layer.
        if (NotebookCanvas != null)
        {
            var canvas = NotebookCanvas.GetComponent<Canvas>();
            if (canvas == null) canvas = NotebookCanvas.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1900;
            if (NotebookCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                NotebookCanvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.M))
    //    {
    //        if (!MapOpen)
    //        {
    //            MapCanvas.alpha = 1f;
    //            MapCanvas.interactable = true;
    //            MapCanvas.blocksRaycasts = true;
    //            MapOpen = true;
    //        }
    //        else if (MapOpen)
    //        {
    //            MapCanvas.alpha = 0f;
    //            MapCanvas.interactable = false;
    //            MapCanvas.blocksRaycasts = false;
    //            MapOpen = false;
    //        }
    //    }
    //}

    public void AvaliableScoreCanva()
    {
        ScoreShowing.alpha = 1f;
        ScoreShowing.interactable = true;
        ScoreShowing.blocksRaycasts = true;
    }

    public void AvaliableNotebookCanva()
    {
        if (NotebookCanvas == null) return;
        NotebookCanvas.gameObject.SetActive(true);
        NotebookCanvas.alpha = 1f;
        NotebookCanvas.interactable = true;
        NotebookCanvas.blocksRaycasts = true;
    }

    public void AvaliableInGameCanva()
    {
        if (InGameInventoryCanvas == null) return;
        InGameInventoryCanvas.gameObject.SetActive(true);
        InGameInventoryCanvas.alpha = 1f;
        InGameInventoryCanvas.interactable = true;
        InGameInventoryCanvas.blocksRaycasts = true;
        DisablingTutorialCanva();
    }

    public void AvaliableTutorialCanva()
    {
        if (TutorialInventoryCanvas == null) return;
        TutorialInventoryCanvas.gameObject.SetActive(true);
        TutorialInventoryCanvas.alpha = 1f;
        TutorialInventoryCanvas.interactable = true;
        TutorialInventoryCanvas.blocksRaycasts = true;
        DisablingInGameCanva();
    }

    public void DiablingScoreCanva()
    {
        ScoreShowing.alpha = 0f;
        ScoreShowing.interactable = false;
        ScoreShowing.blocksRaycasts = false;
    }

    public void DisablingNotebookCanva()
    {
        NotebookCanvas.alpha = 0f;
        NotebookCanvas.interactable = false;
        NotebookCanvas.blocksRaycasts = false;
    }

    public void DisablingInGameCanva()
    {
        if (InGameInventoryCanvas == null) return;
        InGameInventoryCanvas.alpha = 0f;
        InGameInventoryCanvas.interactable = false;
        InGameInventoryCanvas.blocksRaycasts = false;
    }

    public void DisablingTutorialCanva()
    {
        if (TutorialInventoryCanvas == null) return;
        TutorialInventoryCanvas.alpha = 0f;
        TutorialInventoryCanvas.interactable = false;
        TutorialInventoryCanvas.blocksRaycasts = false;
    }
}
