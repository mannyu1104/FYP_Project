using UnityEngine;

public class SummaryButton : MonoBehaviour
{
    [SerializeField] public GameObject SaveIntObject;
    public InventoryManager inventory;

    public void BecomeInvet()
    {
        inventory.AddItem(SaveIntObject);
    }
}
