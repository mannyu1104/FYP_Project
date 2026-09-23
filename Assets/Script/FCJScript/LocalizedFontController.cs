using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

// One font owner for scene text, dropdown clones, browser cards and dialogue.
public class LocalizedFontController : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset chineseFontAsset;
    [SerializeField] private TMP_FontAsset englishFontAsset;
    private readonly System.Collections.Generic.HashSet<TMP_Text> pendingTexts = new System.Collections.Generic.HashSet<TMP_Text>();
    private bool applying;
    public static LocalizedFontController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        // Use the project's GameScript asset itself, with its bundled source font.
        if (chineseFontAsset != null)
        {
            chineseFontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            chineseFontAsset.isMultiAtlasTexturesEnabled = true;
        }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(TextChanged);
        SceneManager.sceneLoaded += SceneLoaded;
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return LocalizationSettings.InitializationOperation;
        ApplyFontNow();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(TextChanged);
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

    }

    private void LocaleChanged(Locale locale) => ApplyFontNow();
    private void SceneLoaded(Scene scene, LoadSceneMode mode) => ApplyFontNow();
    private void TextChanged(Object value)
    {
        if (!applying && value is TMP_Text text) pendingTexts.Add(text);
    }

    private void LateUpdate()
    {
        if (pendingTexts.Count == 0) return;
        var texts = new System.Collections.Generic.List<TMP_Text>(pendingTexts);
        pendingTexts.Clear();
        foreach (TMP_Text text in texts) ApplyTo(text);
    }

    public TMP_FontAsset CurrentFont => IsChinese || englishFontAsset == null ? ChineseFont : englishFontAsset;
    private TMP_FontAsset ChineseFont => chineseFontAsset;
    private bool IsChinese
    {
        get
        {
            try
            {
                var locale = LocalizationSettings.SelectedLocale;
                return locale != null && !string.IsNullOrEmpty(locale.Identifier.Code) &&
                       locale.Identifier.Code.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }

    public void ApplyTo(TMP_Text text)
    {
        if (text == null || !text.gameObject.scene.IsValid()) return;
        TMP_FontAsset font = IsChinese || ContainsCjk(text.text) ? ChineseFont : CurrentFont;
        if (font == null) return;
        // A text can already have the right font but retain another atlas material.
        bool wrongMaterial = text.fontSharedMaterial == null ||
            text.fontSharedMaterial.GetTexture("_MainTex") != font.material.GetTexture("_MainTex");
        if (text.font == font && !wrongMaterial) return;
        applying = true;
        text.font = font;
        text.fontSharedMaterial = font.material;
        text.SetAllDirty();
        applying = false;
    }

    [ContextMenu("Apply Font Now")]
    public void ApplyFontNow()
    {
        foreach (TMP_Text text in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
        {
            ApplyTo(text);
            text.SetAllDirty();
            if (text.isActiveAndEnabled) text.ForceMeshUpdate(false, true);
        }
        foreach (Text text in FindObjectsByType<Text>(FindObjectsInactive.Include))
            if (ChineseFont != null && ChineseFont.sourceFontFile != null && (IsChinese || ContainsCjk(text.text)))
                text.font = ChineseFont.sourceFontFile;
    }

    private static bool ContainsCjk(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        foreach (char c in value)
            if ((c >= '\u3000' && c <= '\u9fff') || (c >= '\uff00' && c <= '\uffef')) return true;
        return false;
    }
}

#if UNITY_EDITOR
// Work around the installed Localization package's GameView toolbar initialization
// exception. The game's own Settings language selector remains available.
[UnityEditor.InitializeOnLoad]
internal static class GameViewLocaleToolbarWorkaround
{
    static GameViewLocaleToolbarWorkaround()
    {
        UnityEditor.EditorPrefs.SetBool("Localization-ShowLocaleMenuInGameView", false);
    }
}
#endif
