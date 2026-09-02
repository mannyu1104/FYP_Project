using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
/// Populates a language dropdown from Unity Localization locales and applies the selected locale.
/// </summary>
[ExecuteAlways]
public class LanguageSelector : MonoBehaviour
{
    private enum LocaleDisplayMode
    {
        FriendlyName,
        LocaleName,
        LocaleCode
    }

    [Header("UI")]
    [Tooltip("Dropdown used to show every available player language.")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [Tooltip("Optional button that opens the language dropdown list.")]
    [SerializeField] private Button openListButton;
    [Tooltip("Optional label that shows the currently selected language.")]
    [SerializeField] private TextMeshProUGUI selectedLanguageLabel;

    [Header("Options")]
    [SerializeField] private bool saveSelection = true;
    [SerializeField] private string playerPrefsKey = "SelectedLocaleCode";
    [SerializeField] private bool includePseudoLocales;
    [SerializeField] private LocaleDisplayMode displayMode = LocaleDisplayMode.FriendlyName;

    [Header("Dropdown Visuals")]
    [SerializeField] private bool applyReadableStyle = true;
    [Range(160f, 520f)]
    [SerializeField] private float dropdownWidth = 320f;
    [Range(36f, 90f)]
    [SerializeField] private float dropdownClosedHeight = 60f;
    [Range(60f, 260f)]
    [SerializeField] private float dropdownOpenHeight = 140f;
    [Range(12f, 42f)]
    [SerializeField] private float captionFontSize = 26f;
    [Range(12f, 42f)]
    [SerializeField] private float optionFontSize = 26f;
    [Range(32f, 90f)]
    [SerializeField] private float optionHeight = 58f;
    [SerializeField] private Color captionTextColor = new Color(0.08f, 0.08f, 0.1f, 1f);
    [SerializeField] private Color optionTextColor = new Color(0.08f, 0.08f, 0.1f, 1f);

    private readonly List<Locale> availableLocales = new List<Locale>();
    private Coroutine initializeCoroutine;
    private bool isChangingDropdownValue;
    private bool isInitialized;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        dropdownWidth = Mathf.Max(160f, dropdownWidth);
        dropdownClosedHeight = Mathf.Max(36f, dropdownClosedHeight);
        dropdownOpenHeight = Mathf.Max(60f, dropdownOpenHeight);
        captionFontSize = Mathf.Max(12f, captionFontSize);
        optionFontSize = Mathf.Max(12f, optionFontSize);
        optionHeight = Mathf.Max(32f, optionHeight);

        ResolveReferences();
        ApplyDropdownStyle();
        RefreshPreviewOptions();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;

        if (Application.isPlaying)
        {
            StartInitializeRoutine();
        }
        else
        {
            ResolveReferences();
            RefreshPreviewOptions();
        }
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;

        if (initializeCoroutine != null)
        {
            StopCoroutine(initializeCoroutine);
            initializeCoroutine = null;
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            StartInitializeRoutine();
        }
    }

    [ContextMenu("Refresh Language Options")]
    public void RefreshLanguageOptions()
    {
        if (!isInitialized)
        {
            if (Application.isPlaying)
            {
                StartInitializeRoutine();
            }
            else
            {
                RefreshPreviewOptions();
            }

            return;
        }

        ResolveReferences();
        RefreshLocaleOptions();
        ApplySavedLocale();
        RefreshDropdownValue();
    }

    private void StartInitializeRoutine()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (initializeCoroutine != null)
        {
            return;
        }

        initializeCoroutine = StartCoroutine(InitializeWhenLocalizationIsReady());
    }

    private IEnumerator InitializeWhenLocalizationIsReady()
    {
        yield return LocalizationSettings.InitializationOperation;

        isInitialized = true;
        initializeCoroutine = null;
        RefreshLanguageOptions();
    }

    public void ShowLanguageList()
    {
        ResolveReferences();
        RefreshLanguageOptions();

        if (languageDropdown != null)
        {
            languageDropdown.Show();
        }
    }

    public void HideLanguageList()
    {
        if (languageDropdown != null)
        {
            languageDropdown.Hide();
        }
    }

    public void SelectLanguage(int index)
    {
        if (!isInitialized || isChangingDropdownValue)
        {
            return;
        }

        if (index < 0 || index >= availableLocales.Count)
        {
            return;
        }

        Locale selectedLocale = availableLocales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;
        SaveLocale(selectedLocale);
        RefreshSelectedLanguageLabel(selectedLocale);
    }

    private void ResolveReferences()
    {
        if (languageDropdown == null)
        {
            languageDropdown = GetComponentInChildren<TMP_Dropdown>(true);
        }

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.RemoveListener(SelectLanguage);
            languageDropdown.onValueChanged.AddListener(SelectLanguage);
            ApplyDropdownStyle();
        }

        if (openListButton != null)
        {
            openListButton.onClick.RemoveListener(ShowLanguageList);
            openListButton.onClick.AddListener(ShowLanguageList);
        }
    }

    private void RefreshLocaleOptions()
    {
        availableLocales.Clear();

        var locales = LocalizationSettings.AvailableLocales.Locales;
        for (int i = 0; i < locales.Count; i++)
        {
            Locale locale = locales[i];
            if (locale == null)
            {
                continue;
            }

            if (!includePseudoLocales && IsPseudoLocale(locale))
            {
                continue;
            }

            availableLocales.Add(locale);
        }

        if (languageDropdown == null)
        {
            return;
        }

        languageDropdown.Hide();

        List<string> options = new List<string>();
        for (int i = 0; i < availableLocales.Count; i++)
        {
            options.Add(GetLocaleDisplayName(availableLocales[i]));
        }

        if (options.Count == 0)
        {
            options.Add("No Languages Found");
            Debug.LogWarning("LanguageSelector: No available locales were found.", this);
        }

        isChangingDropdownValue = true;
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(options);
        languageDropdown.RefreshShownValue();
        ApplyDropdownStyle();
        isChangingDropdownValue = false;
    }

    private void ApplySavedLocale()
    {
        if (!saveSelection || !PlayerPrefs.HasKey(playerPrefsKey))
        {
            return;
        }

        string savedLocaleCode = PlayerPrefs.GetString(playerPrefsKey);
        Locale savedLocale = FindLocaleByCode(savedLocaleCode);
        if (savedLocale != null)
        {
            LocalizationSettings.SelectedLocale = savedLocale;
        }
    }

    private void RefreshDropdownValue()
    {
        Locale selectedLocale = LocalizationSettings.SelectedLocale;
        int selectedIndex = availableLocales.IndexOf(selectedLocale);

        if (languageDropdown != null && selectedIndex >= 0)
        {
            isChangingDropdownValue = true;
            languageDropdown.value = selectedIndex;
            languageDropdown.RefreshShownValue();
            isChangingDropdownValue = false;
        }

        RefreshSelectedLanguageLabel(selectedLocale);
    }

    private void RefreshSelectedLanguageLabel(Locale selectedLocale)
    {
        if (selectedLanguageLabel != null && selectedLocale != null)
        {
            selectedLanguageLabel.text = GetLocaleDisplayName(selectedLocale);
        }
    }

    private void HandleSelectedLocaleChanged(Locale selectedLocale)
    {
        SaveLocale(selectedLocale);
        RefreshDropdownValue();
    }

    private void SaveLocale(Locale locale)
    {
        if (!saveSelection || locale == null)
        {
            return;
        }

        PlayerPrefs.SetString(playerPrefsKey, locale.Identifier.Code);
        PlayerPrefs.Save();
    }

    private Locale FindLocaleByCode(string localeCode)
    {
        if (string.IsNullOrWhiteSpace(localeCode))
        {
            return null;
        }

        for (int i = 0; i < availableLocales.Count; i++)
        {
            Locale locale = availableLocales[i];
            if (locale != null && locale.Identifier.Code == localeCode)
            {
                return locale;
            }
        }

        return null;
    }

    private string GetLocaleDisplayName(Locale locale)
    {
        if (locale == null)
        {
            return string.Empty;
        }

        string code = locale.Identifier.Code;

        if (displayMode == LocaleDisplayMode.FriendlyName)
        {
            string friendlyName = GetFriendlyLocaleName(code);
            if (!string.IsNullOrEmpty(friendlyName))
            {
                return friendlyName;
            }
        }

        if (displayMode == LocaleDisplayMode.LocaleCode)
        {
            return code;
        }

        return string.IsNullOrWhiteSpace(locale.LocaleName)
            ? code
            : locale.LocaleName;
    }

    private void ApplyDropdownStyle()
    {
        if (!applyReadableStyle || languageDropdown == null)
        {
            return;
        }

        ApplyTextStyle(languageDropdown.captionText, captionFontSize, captionTextColor);
        ApplyTextStyle(languageDropdown.itemText, optionFontSize, optionTextColor);
        ApplyDropdownSize();
    }

    private void ApplyDropdownSize()
    {
        RectTransform dropdownRect = languageDropdown.transform as RectTransform;
        if (dropdownRect != null)
        {
            dropdownRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, dropdownWidth);
            dropdownRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dropdownClosedHeight);
        }

        RectTransform template = languageDropdown.template;
        if (template != null)
        {
            template.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, dropdownWidth);
            template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dropdownOpenHeight);
        }

        if (languageDropdown.itemText != null)
        {
            RectTransform itemTextRect = languageDropdown.itemText.rectTransform;
            itemTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, optionHeight);

            Transform itemRoot = languageDropdown.itemText.transform.parent;
            if (itemRoot != null)
            {
                RectTransform itemRect = itemRoot as RectTransform;
                if (itemRect != null)
                {
                    itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, optionHeight);
                }
            }
        }
    }

    private static void ApplyTextStyle(TMP_Text text, float fontSize, Color color)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.color = color;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.margin = new Vector4(12f, 0f, 12f, 0f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void RefreshPreviewOptions()
    {
        if (Application.isPlaying || languageDropdown == null)
        {
            return;
        }

        isChangingDropdownValue = true;
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string> { "\u4E2D\u6587", "English" });
        languageDropdown.value = 0;
        languageDropdown.RefreshShownValue();
        isChangingDropdownValue = false;
    }

    private static string GetFriendlyLocaleName(string localeCode)
    {
        switch (localeCode)
        {
            case "zh-CN":
                return "\u4E2D\u6587";
            case "en-US":
                return "English";
            default:
                return string.Empty;
        }
    }

    private static bool IsPseudoLocale(Locale locale)
    {
        string code = locale.Identifier.Code;
        string localeName = locale.name;

        return code == "pseudo" ||
            (!string.IsNullOrEmpty(code) && code.Contains("pseudo")) ||
            (!string.IsNullOrEmpty(localeName) && localeName.Contains("Pseudo"));
    }
}
