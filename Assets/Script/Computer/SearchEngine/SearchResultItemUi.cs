using System;
using System.Xml;
using TMPro;
using UnityEngine;

// Search Result prefab UI component. Displays a single search result entry and handles click events.
[RequireComponent(typeof(CustomButtonUi))]
public class SearchResultItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;

    private CustomButtonUi customButton;
    private SearchResultEntryData entryData;
    private Action<SearchResultEntryData> onClickCallback;

    private void Awake()
    {
        if (customButton == null) customButton = GetComponent<CustomButtonUi>();
    }

    /// <summary>
    /// Populates this item's display and registers the click handler.
    /// </summary>
    public void Setup(SearchResultEntryData entry, Action<SearchResultEntryData> onClicked)
    {
        UnsubscribeFromLocalization();

        entryData = entry;
        onClickCallback = onClicked;


        SubscribeToLocalization();

        customButton.onLeftClick.RemoveAllListeners();
        customButton.onLeftClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        onClickCallback?.Invoke(entryData);
    }

    private void SubscribeToLocalization()
    {
        if (entryData == null) return;
        // Subscribing to the StringChanged event to update the title text when the localized string changes.
        entryData.resultTitle.StringChanged += UpdateTitleText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (entryData == null) return;
        entryData.resultTitle.StringChanged -= UpdateTitleText;
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