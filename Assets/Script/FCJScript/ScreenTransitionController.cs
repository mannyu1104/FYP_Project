using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays a simple black screen wipe transition.
/// </summary>
public class ScreenTransitionController : MonoBehaviour
{
    private enum WipeDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    [Header("Overlay")]
    [SerializeField] private RectTransform blackOverlay;
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    [SerializeField] private bool forceBlackOverlayColor = true;
    [SerializeField] private bool useRuntimeOverlay = true;
    [SerializeField] private int runtimeSortingOrder = 32767;

    [Header("Timing")]
    [Min(0.01f)]
    [SerializeField] private float wipeInDuration = 0.35f;
    [Min(0.01f)]
    [SerializeField] private float holdDuration = 0.15f;
    [Min(0.01f)]
    [SerializeField] private float wipeOutDuration = 0.35f;

    [Header("Wipe")]
    [SerializeField] private WipeDirection wipeDirection = WipeDirection.LeftToRight;
    [SerializeField] private AnimationCurve wipeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Range(0.03f, 0.25f)]
    [SerializeField] private float gradientEdgeSize = 0.08f;

    private Coroutine transitionRoutine;
    private Canvas runtimeCanvas;
    private Sprite gradientSprite;

    private void Awake()
    {
        ResolveOverlayReferences();
        HideOverlayInstant();
    }

    public void Configure(RectTransform overlay, CanvasGroup canvasGroup)
    {
        blackOverlay = overlay;
        overlayCanvasGroup = canvasGroup;
        HideOverlayInstant();
    }

    public void PlayTransition(Action onCovered)
    {
        ResolveOverlayReferences();

        if (blackOverlay == null)
        {
            onCovered?.Invoke();
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(PlayTransitionRoutine(onCovered));
    }

    private void ResolveOverlayReferences()
    {
        if (useRuntimeOverlay)
        {
            CreateRuntimeOverlayIfNeeded();
            return;
        }

        if (blackOverlay == null)
        {
            GameObject overlayObject = GameObject.Find("BlackTransitionPanel");
            if (overlayObject == null)
            {
                overlayObject = FindSceneObjectByName("BlackTransitionPanel");
            }

            if (overlayObject != null)
            {
                blackOverlay = overlayObject.transform as RectTransform;
            }
        }

        if (overlayCanvasGroup == null && blackOverlay != null)
        {
            overlayCanvasGroup = blackOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = blackOverlay.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (forceBlackOverlayColor && blackOverlay != null)
        {
            Image overlayImage = blackOverlay.GetComponent<Image>();
            if (overlayImage != null)
            {
                overlayImage.color = Color.black;
            }
        }
    }

    private void CreateRuntimeOverlayIfNeeded()
    {
        if (runtimeCanvas != null && blackOverlay != null && overlayCanvasGroup != null)
        {
            runtimeCanvas.sortingOrder = runtimeSortingOrder;
            return;
        }

        HideAssignedOverlay();

        GameObject canvasObject = new GameObject("RuntimeTransitionCanvas");
        runtimeCanvas = canvasObject.AddComponent<Canvas>();
        runtimeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        runtimeCanvas.overrideSorting = true;
        runtimeCanvas.sortingOrder = runtimeSortingOrder;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        GameObject overlayObject = new GameObject("RuntimeBlackTransitionPanel");
        overlayObject.transform.SetParent(canvasObject.transform, false);

        blackOverlay = overlayObject.AddComponent<RectTransform>();
        blackOverlay.anchorMin = Vector2.zero;
        blackOverlay.anchorMax = Vector2.one;
        blackOverlay.offsetMin = Vector2.zero;
        blackOverlay.offsetMax = Vector2.zero;
        blackOverlay.localScale = Vector3.one;

        Image overlayImage = overlayObject.AddComponent<Image>();
        overlayImage.sprite = CreateGradientSprite(true);
        overlayImage.type = Image.Type.Simple;
        overlayImage.color = Color.white;
        overlayImage.raycastTarget = true;

        overlayCanvasGroup = overlayObject.AddComponent<CanvasGroup>();
        HideOverlayInstant();
    }

    private void HideAssignedOverlay()
    {
        if (blackOverlay == null)
        {
            return;
        }

        CanvasGroup assignedCanvasGroup = overlayCanvasGroup != null
            ? overlayCanvasGroup
            : blackOverlay.GetComponent<CanvasGroup>();

        if (assignedCanvasGroup != null)
        {
            assignedCanvasGroup.alpha = 0f;
            assignedCanvasGroup.blocksRaycasts = false;
            assignedCanvasGroup.interactable = false;
        }
    }

    public void HideOverlayInstant()
    {
        if (blackOverlay == null)
        {
            return;
        }

        ApplyWipePivot();
        SetOverlayVisible(false);
        SetWipePosition(0f);
    }

    private IEnumerator PlayTransitionRoutine(Action onCovered)
    {
        SetOverlayVisible(true);
        ApplyWipePivot();
        SetWipePosition(0f);

        yield return AnimateWipe(0f, 1f, wipeInDuration);

        onCovered?.Invoke();

        if (holdDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(holdDuration);
        }

        yield return AnimateWipe(1f, 2f, wipeOutDuration);

        HideOverlayInstant();
        transitionRoutine = null;
    }

    private IEnumerator AnimateWipe(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            float curvedTime = wipeCurve != null ? wipeCurve.Evaluate(normalizedTime) : normalizedTime;
            SetWipePosition(Mathf.Lerp(from, to, curvedTime));
            yield return null;
        }

        SetWipePosition(to);
    }

    private void SetWipePosition(float progress)
    {
        RectTransform parentRect = blackOverlay.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        float screenWidth = parentRect.rect.width;
        float screenHeight = parentRect.rect.height;
        float solidAreaScale = Mathf.Max(0.1f, 1f - gradientEdgeSize * 2f);
        float overlayWidth = screenWidth / solidAreaScale * 1.08f;
        float overlayHeight = screenHeight / solidAreaScale * 1.08f;
        Vector2 position = Vector2.zero;

        if (wipeDirection == WipeDirection.LeftToRight || wipeDirection == WipeDirection.RightToLeft)
        {
            blackOverlay.sizeDelta = new Vector2(overlayWidth, screenHeight);

            float startX = -(overlayWidth + screenWidth) * 0.5f;
            float endX = (overlayWidth + screenWidth) * 0.5f;

            if (wipeDirection == WipeDirection.RightToLeft)
            {
                startX *= -1f;
                endX *= -1f;
            }

            position.x = progress <= 1f
                ? Mathf.Lerp(startX, 0f, progress)
                : Mathf.Lerp(0f, endX, progress - 1f);
        }
        else
        {
            blackOverlay.sizeDelta = new Vector2(screenWidth, overlayHeight);

            float startY = (overlayHeight + screenHeight) * 0.5f;
            float endY = -(overlayHeight + screenHeight) * 0.5f;

            if (wipeDirection == WipeDirection.BottomToTop)
            {
                startY *= -1f;
                endY *= -1f;
            }

            position.y = progress <= 1f
                ? Mathf.Lerp(startY, 0f, progress)
                : Mathf.Lerp(0f, endY, progress - 1f);
        }

        blackOverlay.anchoredPosition = position;
    }

    private void ApplyWipePivot()
    {
        blackOverlay.anchorMin = new Vector2(0.5f, 0.5f);
        blackOverlay.anchorMax = new Vector2(0.5f, 0.5f);
        blackOverlay.pivot = new Vector2(0.5f, 0.5f);
        blackOverlay.localScale = Vector3.one;
    }

    private void SetOverlayVisible(bool visible)
    {
        blackOverlay.gameObject.SetActive(true);

        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = visible ? 1f : 0f;
            overlayCanvasGroup.blocksRaycasts = visible;
            overlayCanvasGroup.interactable = visible;
        }
    }

    private Sprite CreateGradientSprite(bool rebuild = false)
    {
        if (!rebuild && gradientSprite != null)
        {
            return gradientSprite;
        }

        const int textureWidth = 256;
        const int textureHeight = 4;
        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        int edgePixels = Mathf.Max(1, Mathf.RoundToInt(textureWidth * gradientEdgeSize));

        for (int x = 0; x < textureWidth; x++)
        {
            float alpha = 1f;

            if (x < edgePixels)
            {
                alpha = x / (float)edgePixels;
            }
            else if (x > textureWidth - edgePixels)
            {
                alpha = (textureWidth - x) / (float)edgePixels;
            }

            Color color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));

            for (int y = 0; y < textureHeight; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        gradientSprite = Sprite.Create(texture, new Rect(0f, 0f, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), 100f);
        return gradientSprite;
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform target = transforms[i];
            if (target.name == objectName && target.gameObject.scene.IsValid())
            {
                return target.gameObject;
            }
        }

        return null;
    }
}
