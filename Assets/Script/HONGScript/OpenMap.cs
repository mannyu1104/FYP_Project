using UnityEngine;

public class OpenMap : MonoBehaviour
{
    private bool MapOpen;
    [SerializeField] CanvasGroup MapCanvas;
    [SerializeField] bool mKeyToOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MapOpen = false;

        if (MapCanvas == null)
            foreach (var group in FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include))
                if (group.gameObject.scene == gameObject.scene && group.name == "CanvasMap")
                { MapCanvas = group; break; }
        if (MapCanvas == null) { enabled = false; return; }
        foreach (var button in FindObjectsByType<MapButton>(FindObjectsInactive.Include))
            if (button.mapPanel == MapCanvas.gameObject) return;

        MapCanvas.alpha = 0f;
        MapCanvas.interactable = false;
        MapCanvas.blocksRaycasts = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (MapCanvas == null) return;
        if (Input.GetKeyDown(KeyCode.M) && mKeyToOpen)
        {
            foreach (var button in FindObjectsByType<MapButton>(FindObjectsInactive.Include))
                if (button.mapPanel == MapCanvas.gameObject)
                {
                    if (InvestigationFlowController.Instance != null &&
                        InvestigationFlowController.Instance.CurrentStage != InvestigationFlowController.Stage.Investigation) return;
                    button.ToggleMap();
                    return;
                }
            if (!MapOpen)
            {
                MapCanvas.alpha = 1f;
                MapCanvas.interactable = true;
                MapCanvas.blocksRaycasts = true;
                MapOpen = true;
            }
            else if (MapOpen)
            {
                MapCanvas.alpha = 0f;
                MapCanvas.interactable = false;
                MapCanvas.blocksRaycasts = false;
                MapOpen = false;
            }
        }
    }

}
