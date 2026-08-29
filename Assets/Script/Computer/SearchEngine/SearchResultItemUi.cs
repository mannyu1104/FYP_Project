using System;
using TMPro;
using UnityEngine;

// Search Result prefab UI component. Displays a single search result entry and handles click events.
[RequireComponent(typeof(CustomButtonUi))]
public class SearchResultItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text urlText;
    [SerializeField] private TMP_Text snippetText;

    private CustomButtonUi clickable;
    private SearchResultEntryData entryData;
    private Action<SearchResultEntryData> onClickCallback;

    private void Awake()
    {
        clickable = GetComponent<CustomButtonUi>();
    }

    /// <summary>
    /// Populates this item's display and registers the click handler.
    /// </summary>
    public void Setup(SearchResultEntryData entry, Action<SearchResultEntryData> onClicked)
    {
        entryData = entry;
        onClickCallback = onClicked;

        if (titleText != null) titleText.text = entry.resultTitle;

        if (clickable == null) clickable = GetComponent<CustomButtonUi>();
        clickable.onLeftClick.RemoveAllListeners();
        clickable.onLeftClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        onClickCallback?.Invoke(entryData);
    }
}