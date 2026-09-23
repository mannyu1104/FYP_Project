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
    private LocalizedString subscribedTitle;

    private void Awake()
    {
        if (clickable == null) clickable = GetComponent<CustomButtonUi>();
    }

    private void Reset()
    {
        clickable = GetComponent<CustomButtonUi>();
    }

    public void Bind(BrowserTab tab)
    {
        UnsubscribeFromLocalization();

        targetTab = tab;
        if (iconImage != null) iconImage.sprite = tab?.Page?.TabIcon;
        if (titleText != null) titleText.text = string.Empty;

        SubscribeToLocalization();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseTab);
            closeButton.onClick.AddListener(CloseTab);
        }

        if (clickable == null) clickable = GetComponent<CustomButtonUi>();
        if (clickable != null)
        {
            clickable.onLeftClick.RemoveListener(SwitchTab);
            clickable.onLeftClick.AddListener(SwitchTab);
        }

        SetActiveVisual(false);
    }

    public void SetActiveVisual(bool isActive)
    {
        if (clickable != null) clickable.SetForcedHighlight(isActive);
    }

    private void CloseTab()
    {
        if (targetTab != null && BrowserTabManager.Instance != null)
            BrowserTabManager.Instance.CloseTab(targetTab);
    }

    private void SwitchTab()
    {
        if (targetTab != null && BrowserTabManager.Instance != null)
            BrowserTabManager.Instance.SwitchTab(targetTab);
    }

    private void SubscribeToLocalization()
    {
        subscribedTitle = targetTab?.Page?.TabTitle;
        if (subscribedTitle != null) subscribedTitle.StringChanged += UpdateTitleText;
    }

    private void UnsubscribeFromLocalization()
    {
        // A tab may change/clear its page before its button is destroyed.
        // Unsubscribe from the exact string originally subscribed, not its current page.
        if (subscribedTitle == null) return;
        subscribedTitle.StringChanged -= UpdateTitleText;
        subscribedTitle = null;
    }

    private void UpdateTitleText(string value)
    {
        if (titleText != null) titleText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseTab);
        if (clickable != null) clickable.onLeftClick.RemoveListener(SwitchTab);
        targetTab = null;
    }
}
