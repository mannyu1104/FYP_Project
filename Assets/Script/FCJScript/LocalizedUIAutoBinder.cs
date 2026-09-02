using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

/// <summary>
/// Automatically localizes known UI labels in the active scene.
/// </summary>
[DefaultExecutionOrder(-900)]
public class LocalizedUIAutoBinder : MonoBehaviour
{
    private static LocalizedUIAutoBinder instance;

    [SerializeField] private bool forceNormalFontStyle = false;
    [SerializeField] private float rebuildInterval = 0.25f;
    [Header("Button Text Style")]
    [SerializeField] private bool normalizeButtonTextStyle = true;
    [SerializeField] private Color buttonTextColor = new Color(0.12f, 0.12f, 0.12f, 1f);
    [SerializeField] private bool normalizeCompactButtonSize = true;
    [SerializeField] private Vector2 compactButtonMaxSize = new Vector2(180f, 180f);
    [SerializeField] private float compactButtonFontSize = 36f;

    private readonly List<Binding> bindings = new List<Binding>();
    private readonly Dictionary<string, string> textToKey = new Dictionary<string, string>
    {
        { "New Game", "UI_MainMenu_NewGame" },
        { "\u65B0\u6E38\u620F", "UI_MainMenu_NewGame" },
        { "Load Game", "UI_MainMenu_LoadGame" },
        { "\u8BFB\u53D6\u6E38\u620F", "UI_MainMenu_LoadGame" },
        { "Settings", "UI_Common_Settings" },
        { "Setting", "UI_Common_Settings" },
        { "\u8BBE\u7F6E", "UI_Common_Settings" },
        { "Quit Game", "UI_MainMenu_QuitGame" },
        { "\u9000\u51FA\u6E38\u620F", "UI_MainMenu_QuitGame" },
        { "Delete Save", "UI_MainMenu_DeleteSaveTest" },
        { "\u5220\u9664\u5B58\u6863", "UI_MainMenu_DeleteSaveTest" },
        { "Close", "UI_Common_Close" },
        { "\u5173\u95ED", "UI_Common_Close" },
        { "X", "UI_Common_CloseShort" },
        { "\u8FD4\u56DE\u4E3B\u83DC\u5355", "UI_Settings_ReturnToMainMenu" },
        { "Back to Main Menu", "UI_Settings_ReturnToMainMenu" },
        { "---\u8BED\u8A00---", "UI_Settings_LanguageTitle" },
        { "---Language---", "UI_Settings_LanguageTitle" },
        { "---Change Language--", "UI_Settings_LanguageTitle" },
        { "---\u58F0\u97F3---", "UI_Settings_SoundTitle" },
        { "---Sound---", "UI_Settings_SoundTitle" },
        { "---\u5B58\u6863---", "UI_Settings_SaveLoadTitle" },
        { "---Save---", "UI_Settings_SaveLoadTitle" },
        { "---Save / Load---", "UI_Settings_SaveLoadTitle" },
        { "\u4E3B\u97F3\u91CF\uFF1A", "UI_Settings_MasterVolume" },
        { "Master Volume:", "UI_Settings_MasterVolume" },
        { "\u80CC\u666F\u97F3\u91CF\uFF1A", "UI_Settings_BgmVolume" },
        { "BGM Volume:", "UI_Settings_BgmVolume" },
        { "\u97F3\u6548\u97F3\u91CF\uFF1A", "UI_Settings_SfxVolume" },
        { "SFX Volume:", "UI_Settings_SfxVolume" },
        { "\u50A8\u5B58", "UI_Settings_Save" },
        { "Save", "UI_Settings_Save" },
        { "\u8BFB\u53D6", "UI_Settings_Load" },
        { "Load", "UI_Settings_Load" },
        { "\u5386\u53F2\u8BB0\u5F55", "UI_Game_History" },
        { "History", "UI_Game_History" },
        { "\u81EA\u52A8", "UI_Dialogue_Auto" },
        { "Auto", "UI_Dialogue_Auto" },
        { "\u8DF3\u8FC7\u4EBA\u751F", "UI_Dialogue_Skip" },
        { "\u8DF3\u8FC7", "UI_Dialogue_Skip" },
        { "Skip", "UI_Dialogue_Skip" },
        { "\u56DE\u5BB6", "UI_Map_Home" },
        { "Home", "UI_Map_Home" },
        { "\u5B64\u513F\u9662", "UI_Map_Orphanage" },
        { "Orphanage", "UI_Map_Orphanage" },
        { "\u53BB\u798F\u5229\u9662", "UI_Map_WelfareCenter" },
        { "Welfare Center", "UI_Map_WelfareCenter" },
        { "\u516C\u56ED", "UI_Map_Park" },
        { "Park", "UI_Map_Park" },
        { "\u8FDB\u5165\u5165\u53E3\u5904", "UI_001" },
        { "Enter Entrance", "UI_001" },
        { "\u53BB\u804C\u5458\u5BA4", "UI_002" },
        { "Go to Staff Room", "UI_002" },
        { "\u56DE\u5B64\u513F\u9662", "UI_003" },
        { "Back to Orphanage", "UI_003" },
        { "\u56DE\u5165\u53E3\u5904", "UI_004" },
        { "Back to Entrance", "UI_004" }
    };
    private readonly Dictionary<string, string> zhTextByKey = new Dictionary<string, string>
    {
        { "UI_MainMenu_NewGame", "\u65B0\u6E38\u620F" },
        { "UI_MainMenu_LoadGame", "\u8BFB\u53D6\u6E38\u620F" },
        { "UI_MainMenu_QuitGame", "\u9000\u51FA\u6E38\u620F" },
        { "UI_MainMenu_DeleteSaveTest", "\u5220\u9664\u5B58\u6863" },
        { "UI_Common_Settings", "\u8BBE\u7F6E" },
        { "UI_Common_Close", "\u5173\u95ED" },
        { "UI_Common_CloseShort", "X" },
        { "UI_Settings_ReturnToMainMenu", "\u8FD4\u56DE\u4E3B\u83DC\u5355" },
        { "UI_Settings_LanguageTitle", "---\u8BED\u8A00---" },
        { "UI_Settings_SoundTitle", "---\u58F0\u97F3---" },
        { "UI_Settings_SaveLoadTitle", "---\u5B58\u6863---" },
        { "UI_Settings_MasterVolume", "\u4E3B\u97F3\u91CF\uFF1A" },
        { "UI_Settings_BgmVolume", "\u80CC\u666F\u97F3\u91CF\uFF1A" },
        { "UI_Settings_SfxVolume", "\u97F3\u6548\u97F3\u91CF\uFF1A" },
        { "UI_Settings_Save", "\u50A8\u5B58" },
        { "UI_Settings_Load", "\u8BFB\u53D6" },
        { "UI_Game_History", "\u5386\u53F2\u8BB0\u5F55" },
        { "UI_Dialogue_Auto", "\u81EA\u52A8" },
        { "UI_Dialogue_Skip", "\u8DF3\u8FC7" },
        { "UI_Map_Home", "\u56DE\u5BB6" },
        { "UI_Map_Orphanage", "\u5B64\u513F\u9662" },
        { "UI_Map_WelfareCenter", "\u53BB\u798F\u5229\u9662" },
        { "UI_Map_Park", "\u516C\u56ED" },
        { "UI_001", "\u8FDB\u5165\u5165\u53E3\u5904" },
        { "UI_002", "\u53BB\u804C\u5458\u5BA4" },
        { "UI_003", "\u56DE\u5B64\u513F\u9662" },
        { "UI_004", "\u56DE\u5165\u53E3\u5904" }
    };
    private readonly Dictionary<string, string> enTextByKey = new Dictionary<string, string>
    {
        { "UI_MainMenu_NewGame", "New Game" },
        { "UI_MainMenu_LoadGame", "Load Game" },
        { "UI_MainMenu_QuitGame", "Quit Game" },
        { "UI_MainMenu_DeleteSaveTest", "Delete Save" },
        { "UI_Common_Settings", "Settings" },
        { "UI_Common_Close", "Close" },
        { "UI_Common_CloseShort", "X" },
        { "UI_Settings_ReturnToMainMenu", "Back to Main Menu" },
        { "UI_Settings_LanguageTitle", "---Language---" },
        { "UI_Settings_SoundTitle", "---Sound---" },
        { "UI_Settings_SaveLoadTitle", "---Save---" },
        { "UI_Settings_MasterVolume", "Master Volume:" },
        { "UI_Settings_BgmVolume", "BGM Volume:" },
        { "UI_Settings_SfxVolume", "SFX Volume:" },
        { "UI_Settings_Save", "Save" },
        { "UI_Settings_Load", "Load" },
        { "UI_Game_History", "History" },
        { "UI_Dialogue_Auto", "Auto" },
        { "UI_Dialogue_Skip", "Skip" },
        { "UI_Map_Home", "Home" },
        { "UI_Map_Orphanage", "Orphanage" },
        { "UI_Map_WelfareCenter", "Welfare Center" },
        { "UI_Map_Park", "Park" },
        { "UI_001", "Enter Entrance" },
        { "UI_002", "Go to Staff Room" },
        { "UI_003", "Back to Orphanage" },
        { "UI_004", "Back to Entrance" }
    };
    private float nextRebuildTime;
    private bool localizationReady;

    private struct Binding
    {
        public TMP_Text Text;
        public string Key;

        public Binding(TMP_Text text, string key)
        {
            Text = text;
            Key = key;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateRuntimeBinder()
    {
        if (instance != null)
        {
            return;
        }

        GameObject binderObject = new GameObject("LocalizedUIAutoBinder");
        instance = binderObject.AddComponent<LocalizedUIAutoBinder>();
        DontDestroyOnLoad(binderObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;
    }

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
        localizationReady = true;
        RebuildBindings();
        RefreshBindings();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;
    }

    private void LateUpdate()
    {
        if (!localizationReady)
        {
            return;
        }

        if (Time.unscaledTime < nextRebuildTime)
        {
            return;
        }

        nextRebuildTime = Time.unscaledTime + Mathf.Max(0.05f, rebuildInterval);
        RebuildBindings();
        RefreshBindings();
    }

    private void HandleSelectedLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        if (!localizationReady)
        {
            return;
        }

        RebuildBindings();
        RefreshBindings();
    }

    private void RebuildBindings()
    {
        bindings.Clear();

        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || !text.gameObject.scene.IsValid())
            {
                continue;
            }

            string sourceText = Normalize(text.text);
            if (string.IsNullOrEmpty(sourceText) || !textToKey.TryGetValue(sourceText, out string key))
            {
                continue;
            }

            bindings.Add(new Binding(text, key));
        }
    }

    private void RefreshBindings()
    {
        if (!localizationReady)
        {
            return;
        }

        for (int i = 0; i < bindings.Count; i++)
        {
            Binding binding = bindings[i];
            if (binding.Text == null)
            {
                continue;
            }

            if (TryGetLocalizedText(binding.Key, out string localizedText))
            {
                binding.Text.text = localizedText;
                ApplyTextStyle(binding.Text);
            }
        }

        NormalizeButtonTexts();
    }

    private bool TryGetLocalizedText(string key, out string value)
    {
        Dictionary<string, string> table = IsChineseSelected() ? zhTextByKey : enTextByKey;
        return table.TryGetValue(key, out value);
    }

    private bool IsChineseSelected()
    {
        string localeCode = LocalizationSettings.SelectedLocale != null
            ? LocalizationSettings.SelectedLocale.Identifier.Code
            : string.Empty;

        return localeCode.StartsWith("zh");
    }

    private void ApplyTextStyle(TMP_Text text)
    {
        if (!forceNormalFontStyle || text == null)
        {
            return;
        }

        text.fontStyle = FontStyles.Normal;
    }

    private void NormalizeButtonTexts()
    {
        if (!normalizeButtonTextStyle)
        {
            return;
        }

        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || !text.gameObject.scene.IsValid())
            {
                continue;
            }

            ApplyButtonTextStyle(text);
        }
    }

    private void ApplyButtonTextStyle(TMP_Text text)
    {
        Button button = text.GetComponentInParent<Button>(true);
        if (button == null)
        {
            return;
        }

        text.fontStyle = FontStyles.Bold;
        text.fontWeight = FontWeight.Bold;
        text.color = buttonTextColor;

        if (!normalizeCompactButtonSize || !IsCompactButton(button))
        {
            return;
        }

        text.enableAutoSizing = false;
        text.fontSize = compactButtonFontSize;
        text.fontSizeMin = Mathf.Min(text.fontSizeMin, compactButtonFontSize);
        text.fontSizeMax = compactButtonFontSize;
    }

    private bool IsCompactButton(Button button)
    {
        RectTransform rectTransform = button.transform as RectTransform;
        if (rectTransform == null)
        {
            return false;
        }

        Vector2 size = rectTransform.rect.size;
        if (size.x <= 0f || size.y <= 0f)
        {
            size = rectTransform.sizeDelta;
        }

        return size.x <= compactButtonMaxSize.x && size.y <= compactButtonMaxSize.y;
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    }
}
