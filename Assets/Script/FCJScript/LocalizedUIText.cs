using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

/// <summary>
/// Updates a UI text component from a Unity Localization string entry.
/// </summary>
[ExecuteAlways]
public class LocalizedUIText : MonoBehaviour
{
    [SerializeField] private LocalizedString localizedText;
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Text legacyText;
    [SerializeField] private bool refreshInEditMode = true;

    private void Reset()
    {
        AutoFindText();
    }

    private void Awake()
    {
        AutoFindText();
    }

    private void OnEnable()
    {
        AutoFindText();
        localizedText.StringChanged += HandleStringChanged;
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;
        RefreshText();
    }

    private void OnDisable()
    {
        localizedText.StringChanged -= HandleStringChanged;
        LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoFindText();

        if (!Application.isPlaying && refreshInEditMode)
        {
            UnityEditor.EditorApplication.delayCall += RefreshTextInEditor;
        }
    }

    private void RefreshTextInEditor()
    {
        if (this == null || Application.isPlaying || !refreshInEditMode)
        {
            return;
        }

        RefreshText();
    }
#endif

    public void SetEntry(string tableName, string entryKey)
    {
        localizedText.TableReference = tableName;
        localizedText.TableEntryReference = entryKey;
        RefreshText();
    }

    private void AutoFindText()
    {
        if (tmpText == null)
        {
            tmpText = GetComponent<TMP_Text>();
        }

        if (legacyText == null)
        {
            legacyText = GetComponent<Text>();
        }
    }

    private void HandleStringChanged(string value)
    {
        ApplyText(value);
    }

    private void HandleSelectedLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        RefreshText();
    }

    private void RefreshText()
    {
        if (!HasLocalizedReference())
        {
            return;
        }

        if (!Application.isPlaying && !refreshInEditMode)
        {
            return;
        }

        ApplyText(localizedText.GetLocalizedString());
    }

    private bool HasLocalizedReference()
    {
        return localizedText.TableReference.ReferenceType != TableReference.Type.Empty &&
            localizedText.TableEntryReference.ReferenceType != TableEntryReference.Type.Empty;
    }

    private void ApplyText(string value)
    {
        if (tmpText != null)
        {
            tmpText.text = value;
        }

        if (legacyText != null)
        {
            legacyText.text = value;
        }
    }
}
