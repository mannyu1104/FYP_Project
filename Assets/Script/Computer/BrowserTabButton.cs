using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

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
        UnsubscribeFromLocalization();

        targetTab = tab;
        iconImage.sprite = tab.Page.TabIcon;

        SubscribeToLocalization();

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

    private void SubscribeToLocalization()
    {
        if (targetTab == null) return;
        targetTab.Page.TabTitle.StringChanged += UpdateTitleText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (targetTab == null) return;
        targetTab.Page.TabTitle.StringChanged -= UpdateTitleText;
    }

    private void UpdateTitleText(string value)
    {
        if (titleText != null) titleText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}