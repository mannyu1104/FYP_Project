using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class GridSystem : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private GameObject GridPrefab;
    [SerializeField] private RectTransform canvaRect;

    public float cellSizeofx;
    public float cellSizeofy;
    public int DetectorCount;
    private bool GridOpen;

    private void Start()
    {
        GridOpen = false;
        DetectorCount = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!GridOpen)
            {
                GenerateGrid();
            }
            else if (GridOpen)
            {
                DestroyGrid();
                DetectorCount = 0;
            }
        }
    }

    void GenerateGrid()
    {
        cellSizeofx = canvaRect.rect.width / width;
        cellSizeofy = canvaRect.rect.height / height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //var SpawnTile = Instantiate(GridPrefab, new Vector3(x, y), Quaternion.identity);

                GameObject spawnTile = Instantiate(GridPrefab, transform);

                RectTransform rect = spawnTile.GetComponent<RectTransform>();

                // Anchor to top-left
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);

                // Pivot at top-left
                rect.pivot = new Vector2(0, 1);

                rect.sizeDelta = new Vector2(cellSizeofx, cellSizeofy);

                rect.anchoredPosition = new Vector2(
                    x * cellSizeofx,
                    -y * cellSizeofy
                );

                spawnTile.name = $"Grid {x} {y}";
            }
        }

        GridOpen = true;
    }

    void DestroyGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Unlocking unlockbutton = FindAnyObjectByType<Unlocking>();
        if (unlockbutton != null)
        {
            GridOpen = false;
            Destroy(unlockbutton.gameObject);
        }
        else if (unlockbutton == null)
        {
            GridOpen = false;
            return;
        }
    }
}
