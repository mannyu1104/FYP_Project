using UnityEngine;

public class OpenMap : MonoBehaviour
{
    private bool MapOpen;
    [SerializeField] CanvasGroup MapCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MapOpen = false;

        MapCanvas.alpha = 0f;
        MapCanvas.interactable = false;
        MapCanvas.blocksRaycasts = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
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
