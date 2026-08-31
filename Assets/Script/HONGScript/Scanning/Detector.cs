using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Detector : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button ConfirmButton;
    public GameObject DetectedTarget;
    public RectTransform canvasRect;
    public bool buttonspawned;
    private Unlocking ItemTransfer;
    public InventoryManager inventoryTrans3;

    private void Start()
    {
        buttonspawned = false;

        if (canvasRect == null)
        {
            canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        }
    }
    private void Update()
    {
        if (buttonspawned == true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                buttonspawned = false;

                Unlocking unlockbutton = FindAnyObjectByType<Unlocking>();
                Destroy(unlockbutton.gameObject);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && buttonspawned == false)
        {
            Vector2 localPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localPosition);

            Button newButton = Instantiate(ConfirmButton, canvasRect);

            newButton.GetComponent<RectTransform>().anchoredPosition = localPosition;

            ItemTransfer = newButton.GetComponent<Unlocking>();

            ItemTransfer.UnlockingTarget = DetectedTarget;

            ItemTransfer.inventory = inventoryTrans3;

            buttonspawned = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Detected");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            DragableItemSave item = collision.gameObject.GetComponent<DragableItemSave>();
            if (item.thisGet == false)
            {
                Debug.Log("Unlocking");
                DetectedTarget = collision.gameObject;
            }
            else
            {
                return;
            }
        }
    }
}
