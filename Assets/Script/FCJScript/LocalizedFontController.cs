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

#if UNITY_EDITOR
    [Header("Editor Cleanup")]
    [Tooltip("Uses the Chinese font as the primary font for scene texts that already contain CJK characters.")]
    [SerializeField] private bool applyChineseFontToCjkTextInEditMode = true;
    [Tooltip("Removes generated TMP submesh objects from the open scene while editing.")]
    [SerializeField] private bool cleanGeneratedSubMeshesInEditMode = true;
#endif

    private Coroutine initializeCoroutine;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;
            StartInitializeRoutine();
        }
#if UNITY_EDITOR
        else if (applyChineseFontToCjkTextInEditMode || cleanGeneratedSubMeshesInEditMode)
        {
            EditorApplication.delayCall += RepairEditModeTextMeshes;
        }
#endif
    }

    private void OnDisable()
    {
        if (initializeCoroutine != null)
        {
            StopCoroutine(initializeCoroutine);
            initializeCoroutine = null;
        }

        if (Application.isPlaying)
        {
            LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;
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

#if UNITY_EDITOR
    [ContextMenu("Clean Generated TMP SubMeshes")]
    public void CleanGeneratedSubMeshes()
    {
        TMP_SubMeshUI[] subMeshes = Resources.FindObjectsOfTypeAll<TMP_SubMeshUI>();
        for (int i = subMeshes.Length - 1; i >= 0; i--)
        {
            TMP_SubMeshUI subMesh = subMeshes[i];
            if (subMesh == null || ShouldSkipObject(subMesh.gameObject))
            {
                continue;
            }

            Undo.DestroyObjectImmediate(subMesh.gameObject);
        }
    }

    [ContextMenu("Repair CJK Text Fonts And Clean SubMeshes")]
    public void RepairCjkTextFontsAndCleanSubMeshes()
    {
        ApplyChineseFontToCjkTextsInEditMode();
        CleanGeneratedSubMeshes();
    }
#endif

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

            if (Application.isPlaying)
            {
                text.ForceMeshUpdate();
            }

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
        if (ShouldSkipObject(text.gameObject))
        {
            return true;
        }
#endif

        return false;
    }

#if UNITY_EDITOR
    private bool ShouldSkipObject(GameObject target)
    {
        return target == null || !target.scene.IsValid() || EditorUtility.IsPersistent(target);
    }

    private void RepairEditModeTextMeshes()
    {
        if (this == null || Application.isPlaying)
        {
            return;
        }

        if (applyChineseFontToCjkTextInEditMode)
        {
            ApplyChineseFontToCjkTextsInEditMode();
        }

        if (cleanGeneratedSubMeshesInEditMode)
        {
            CleanGeneratedSubMeshes();
        }
    }

    private void ApplyChineseFontToCjkTextsInEditMode()
    {
        if (chineseFontAsset == null)
        {
            return;
        }

        TMP_Text[] texts = FindTextObjects();
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || ShouldSkipText(text) || !ContainsCjkCharacter(text.text))
            {
                continue;
            }

            if (text.font == chineseFontAsset)
            {
                continue;
            }

            Undo.RecordObject(text, "Apply CJK TMP Font");
            text.font = chineseFontAsset;
            EditorUtility.SetDirty(text);
        }
    }

    private bool ContainsCjkCharacter(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];
            if ((character >= '\u3400' && character <= '\u9FFF') ||
                (character >= '\uF900' && character <= '\uFAFF'))
            {
                return true;
            }
        }

        return false;
    }
#endif

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
