//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.EventSystems;

//public class SpawnScanner : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
//{
//    [SerializeField] private GameObject DetectorPrefab;
//    private GridSystem GridInfo;
//    private bool Spawned;
//    [SerializeField] private int MaxDetect;
     
//    private void Start()
//    {
//        Spawned = false;
//        GridInfo = GetComponentInParent<GridSystem>();
//    }

//    public void OnPointerDown(PointerEventData eventData)
//    {
//        if (Input.GetMouseButton(0))
//        {
//            if (!Spawned && GridInfo.DetectorCount < MaxDetect)
//            {
//                GenerateDetector();
//            }
//        }
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (Input.GetMouseButton(0))
//        {
//            if (!Spawned && GridInfo.DetectorCount < MaxDetect)
//            {
//                GenerateDetector();
//            }
//        }
//    }


//    void GenerateDetector()
//    {
//        Spawned = true;

//        GameObject detector = Instantiate(DetectorPrefab, transform);

//        RectTransform rect = detector.GetComponent<RectTransform>();

//        rect.sizeDelta = new Vector2(GridInfo.cellSizeofx, GridInfo.cellSizeofy);

//        detector.name = $"Detector {GridInfo.DetectorCount}";

//        GridInfo.DetectorCount += 1;
//    }
//}
