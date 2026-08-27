using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class GridSystem : MonoBehaviour
{
    //private bool choosing;
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.G))
    //    {
    //        choosing = true;
    //        GridSystemAble();
    //    }
    //}

    [SerializeField] private int width, height;
    [SerializeField] private GameObject GridPrefab;
    private float cellSizeofx;
    private float cellSizeofy;
    [SerializeField] private RectTransform canvaRect;

    private void Start()
    {
        GenerateGrid();
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
    }
}
