using UnityEngine;

public class OpenCanvasButton : MonoBehaviour
{
    //private bool MapOpen;
    [SerializeField] CanvasGroup InGameInventoryCanvas;
    [SerializeField] CanvasGroup TutorialInventoryCanvas;
    [SerializeField] CanvasGroup NotebookCanvas;
    [SerializeField] CanvasGroup ScoreShowing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //MapOpen = false;
        InGameInventoryCanvas.alpha = 0f;
        InGameInventoryCanvas.interactable = false;
        InGameInventoryCanvas.blocksRaycasts = false;

        TutorialInventoryCanvas.alpha = 0f;
        TutorialInventoryCanvas.interactable = false;
        TutorialInventoryCanvas.blocksRaycasts = false;

        NotebookCanvas.alpha = 0f;
        NotebookCanvas.interactable = false;
        NotebookCanvas.blocksRaycasts = false;

        ScoreShowing.alpha = 0f;
        ScoreShowing.interactable = false;
        ScoreShowing.blocksRaycasts = false;
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
        NotebookCanvas.alpha = 1f;
        NotebookCanvas.interactable = true;
        NotebookCanvas.blocksRaycasts = true;
    }

    public void AvaliableInGameCanva()
    {
        InGameInventoryCanvas.alpha = 1f;
        InGameInventoryCanvas.interactable = true;
        InGameInventoryCanvas.blocksRaycasts = true;
    }

    public void AvaliableTutorialCanva()
    {
        TutorialInventoryCanvas.alpha = 1f;
        TutorialInventoryCanvas.interactable = true;
        TutorialInventoryCanvas.blocksRaycasts = true;
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
        InGameInventoryCanvas.alpha = 0f;
        InGameInventoryCanvas.interactable = false;
        InGameInventoryCanvas.blocksRaycasts = false;
    }

    public void DisablingTutorialCanva()
    {
        TutorialInventoryCanvas.alpha = 0f;
        TutorialInventoryCanvas.interactable = false;
        TutorialInventoryCanvas.blocksRaycasts = false;
    }
}
