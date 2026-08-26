using UnityEngine;

[RequireComponent(typeof(CustomButtonUi))]
public class BrowserAppButton : MonoBehaviour
{
    [SerializeField] private WebPageDataScriptableObject page;

    private void Awake()
    {
        CustomButtonUi clickable = GetComponent<CustomButtonUi>();
        clickable.onLeftClick.AddListener(OpenThisPage);
    }

    private void OpenThisPage()
    {
        BrowserTabManager.Instance.OpenPage(page);
        Debug.Log($"Opened page: {page.TabTitle} (ID: {page.PageId})");
    }
}