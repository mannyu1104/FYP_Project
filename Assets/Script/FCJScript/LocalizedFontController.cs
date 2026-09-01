using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Applies one TMP font asset to all loaded scene TMP texts based on the selected locale.
/// </summary>
[ExecuteAlways]
public class LocalizedFontController : MonoBehaviour
{
    private const string ChineseLocaleCode = "zh-CN";

    [Header("Font Assets")]
    [Tooltip("Font used when the selected locale is Chinese.")]
    [SerializeField] private TMP_FontAsset chineseFontAsset;
    [Tooltip("Font used when the selected locale is not Chinese.")]
    [SerializeField] private TMP_FontAsset englishFontAsset;

    [Header("Fallback")]
    [Tooltip("Adds the Chinese font as a fallback to the English font to prevent missing glyph squares.")]
    [SerializeField] private bool addChineseFallbackToEnglish = true;

    [Header("Scope")]
    [Tooltip("Includes inactive scene objects when applying fonts.")]
    [SerializeField] private bool includeInactiveSceneObjects = true;
    [Tooltip("Applies the preview font while editing in the Unity Editor.")]
    [SerializeField] private bool previewInEditMode = true;

    [Header("Edit Mode Preview")]
    [Tooltip("Font used in Edit Mode before the game has a selected locale.")]
    [SerializeField] private bool previewChineseFont = true;

    private Coroutine initializeCoroutine;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;

        if (Application.isPlaying)
        {
            StartInitializeRoutine();
        }
        else
        {
            ApplyPreviewFont();
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

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ApplyPreviewFont();
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            StartInitializeRoutine();
        }
    }

    [ContextMenu("Apply Font Now")]
    public void ApplyFontNow()
    {
        TMP_FontAsset targetFont = GetFontForCurrentLocale();
        ApplyFont(targetFont);
    }

    private void StartInitializeRoutine()
    {
        if (initializeCoroutine != null)
        {
            return;
        }

        initializeCoroutine = StartCoroutine(InitializeWhenLocalizationIsReady());
    }

    private IEnumerator InitializeWhenLocalizationIsReady()
    {
        yield return LocalizationSettings.InitializationOperation;

        initializeCoroutine = null;
        ApplyFontNow();
    }

    private void HandleSelectedLocaleChanged(Locale selectedLocale)
    {
        ApplyFont(GetFontForLocale(selectedLocale));
    }

    private TMP_FontAsset GetFontForCurrentLocale()
    {
        if (!Application.isPlaying)
        {
            return previewChineseFont ? chineseFontAsset : englishFontAsset;
        }

        return GetFontForLocale(LocalizationSettings.SelectedLocale);
    }

    private TMP_FontAsset GetFontForLocale(Locale locale)
    {
        if (locale != null && locale.Identifier.Code == ChineseLocaleCode)
        {
            return chineseFontAsset;
        }

        return englishFontAsset != null ? englishFontAsset : chineseFontAsset;
    }

    private void ApplyPreviewFont()
    {
        if (!previewInEditMode)
        {
            return;
        }

        ApplyFont(GetFontForCurrentLocale());
    }

    private void ApplyFont(TMP_FontAsset targetFont)
    {
        if (targetFont == null)
        {
            return;
        }

        ConfigureFallbacks();

        TMP_Text[] texts = FindTextObjects();
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || ShouldSkipText(text))
            {
                continue;
            }

            text.font = targetFont;
            text.ForceMeshUpdate();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(text);
            }
#endif
        }
    }

    private TMP_Text[] FindTextObjects()
    {
        if (includeInactiveSceneObjects)
        {
            return Resources.FindObjectsOfTypeAll<TMP_Text>();
        }

        return FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude);
    }

    private bool ShouldSkipText(TMP_Text text)
    {
#if UNITY_EDITOR
        if (!text.gameObject.scene.IsValid() || EditorUtility.IsPersistent(text))
        {
            return true;
        }
#endif

        return false;
    }

    private void ConfigureFallbacks()
    {
        if (!addChineseFallbackToEnglish || englishFontAsset == null || chineseFontAsset == null)
        {
            return;
        }

        if (!englishFontAsset.fallbackFontAssetTable.Contains(chineseFontAsset))
        {
            englishFontAsset.fallbackFontAssetTable.Add(chineseFontAsset);
        }
    }
}
