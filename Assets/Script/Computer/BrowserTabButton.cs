using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CustomButtonUi))]
public class BrowserTabButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CustomButtonUi clickable;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;

    private BrowserTab targetTab;

    private void Reset()
    {
        clickable = GetComponent<CustomButtonUi>();
    }

    public void Bind(BrowserTab tab)
    {
        targetTab = tab;
        iconImage.sprite = tab.Page.TabIcon;
        titleText.text = tab.Page.TabTitle;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => BrowserTabManager.Instance.CloseTab(targetTab));

        clickable.onLeftClick.RemoveAllListeners();
        clickable.onLeftClick.AddListener(() => BrowserTabManager.Instance.SwitchTab(targetTab));

        SetActiveVisual(false);
    }

    public void SetActiveVisual(bool isActive)
    {
        clickable.SetForcedHighlight(isActive);
    }
}