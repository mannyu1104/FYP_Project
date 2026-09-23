using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Localization;

public class MapUnlockManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private TMP_InputField unlockInputField;

    [Header("Maps")]
    [SerializeField] private List<MapIcon> mapIcons = new List<MapIcon>();

    [Header("Feedback (optional)")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string successMessage = "Unlocked!";
    [SerializeField] private string alreadyUnlockedMessage = "Already unlocked.";
    [SerializeField] private string noMatchMessage = "No location found with that name.";
    [SerializeField] private LocalizedString successMessageLocalized;
    [SerializeField] private LocalizedString alreadyUnlockedMessageLocalized;
    [SerializeField] private LocalizedString noMatchMessageLocalized;

    private void Awake()
    {

        if (unlockInputField != null)
        {
            unlockInputField.onSubmit.AddListener(_ => OnUnlockSubmitted());
        }
    }

    private void OnUnlockSubmitted()
    {
        if (unlockInputField == null) return;

        TryUnlock(unlockInputField.text);
        unlockInputField.text = string.Empty;
    }

    public void CloseMap()
    {
        foreach (var button in FindObjectsByType<MapButton>(FindObjectsInactive.Include))
            if (button.mapPanel != null && transform.IsChildOf(button.mapPanel.transform))
            {
                button.CloseMap();
                return;
            }
    }

    public bool TryUnlock(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            ShowFeedback(noMatchMessageLocalized.GetLocalizedString());
            return false;
        }

        string normalizedInput = input.Trim().ToLowerInvariant();

        foreach (MapIcon icon in mapIcons)
        {
            if (icon == null || icon.Mapdetials == null) continue;

            string normalizedPlaceName = icon.Mapdetials.PlaceNameLocalized.GetLocalizedString()?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(normalizedPlaceName)) continue;

            if (normalizedPlaceName != normalizedInput) continue;

            if (icon.thisUnlocked)
            {
                ShowFeedback(alreadyUnlockedMessageLocalized.GetLocalizedString());
                return true;
            }

            icon.Unlocking();
            ShowFeedback(successMessageLocalized.GetLocalizedString());
            return true;
        }

        ShowFeedback(noMatchMessageLocalized.GetLocalizedString());
        return false;
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null) return;
            
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
    }
}
