using UnityEngine;
using System;

[RequireComponent(typeof(CustomButtonUi))]
public class BrowserAppButton : MonoBehaviour
{
    public static event Action BrowserAppOpened;

    [SerializeField] private WebPageDataScriptableObject page;

    private void Awake()
    {
        CustomButtonUi clickable = GetComponent<CustomButtonUi>();
        clickable.onLeftClick.AddListener(OpenThisPage);
    }

    private void OpenThisPage()
    {
        BrowserAppOpened?.Invoke();
        BrowserTabManager.Instance.OpenPage(page);
        Debug.Log($"Opened page: {page.TabTitle} (ID: {page.PageId})");
    }
}
