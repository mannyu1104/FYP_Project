using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HoverEffect : MonoBehaviour
{
    [System.Serializable]
    public class HoverParameters
    {
        public Color normalColor = Color.white;
        public Color hoverColor = Color.white;
        public Color glowColor = new Color(1f, 1f, 1f, 0.85f);
        [Min(1f)]
        public float glowScale = 1.08f;
        [Min(1f)]
        public float outlineWidth = 6f;
        public bool autoCalculateGlowRange = true;
        [Range(0.01f, 0.15f)]
        public float glowRangeRatio = 0.04f;
        [Min(0f)]
        public float minimumGlowWidth = 2f;
        [Min(0f)]
        public float maximumGlowWidth = 24f;
        [Min(1f)]
        public float hoverScale = 1f;
        [Min(0f)]
        public float transitionSpeed = 12f;
        public bool autoCreateGlowOutline = true;
    }

    private class HoverState
    {
        public GameObject itemObject;
        public GameObject glowOutline;
        public List<GameObject> glowLayers = new List<GameObject>();
        public Graphic uiGraphic;
        public SpriteRenderer spriteRenderer;
        public Collider2D collider2D;
        public Vector3 normalScale;
        public bool isHovered;
    }

    [Header("Hover Items")]
    public List<GameObject> hoverItems = new List<GameObject>();

    [Header("Hover Parameters")]
    public HoverParameters hoverParameters = new HoverParameters();

    private readonly List<HoverState> hoverStates = new List<HoverState>();

    void Start()
    {
        EnsureHoverParameters();
        RebuildHoverStates();
    }

    void OnValidate()
    {
        EnsureHoverParameters();

        hoverParameters.glowScale = Mathf.Max(1f, hoverParameters.glowScale);
        hoverParameters.outlineWidth = Mathf.Max(1f, hoverParameters.outlineWidth);
        hoverParameters.glowRangeRatio = Mathf.Clamp(hoverParameters.glowRangeRatio, 0.01f, 0.15f);
        hoverParameters.minimumGlowWidth = Mathf.Max(0f, hoverParameters.minimumGlowWidth);
        hoverParameters.maximumGlowWidth = Mathf.Max(hoverParameters.minimumGlowWidth, hoverParameters.maximumGlowWidth);
        hoverParameters.hoverScale = Mathf.Max(1f, hoverParameters.hoverScale);
        hoverParameters.transitionSpeed = Mathf.Max(0f, hoverParameters.transitionSpeed);
    }

    void Update()
    {
        EnsureHoverParameters();
        SyncHoverStatesWithList();

        Vector2 mousePosition = GetMousePosition();

        for (int i = 0; i < hoverStates.Count; i++)
        {
            HoverState state = hoverStates[i];

            if (state.itemObject == null)
            {
                continue;
            }

            state.isHovered = IsPointerOverItem(state, mousePosition);
            ApplyHoverVisual(state);
        }

    }

    private void RebuildHoverStates()
    {
        hoverStates.Clear();

        for (int i = 0; i < hoverItems.Count; i++)
        {
            HoverState state = CreateHoverState(hoverItems[i]);

            if (state != null)
            {
                hoverStates.Add(state);
            }
        }
    }

    private void SyncHoverStatesWithList()
    {
        if (hoverStates.Count != CountValidHoverItems())
        {
            RebuildHoverStates();
            return;
        }

        int stateIndex = 0;

        for (int i = 0; i < hoverItems.Count; i++)
        {
            if (hoverItems[i] == null)
            {
                continue;
            }

            if (stateIndex >= hoverStates.Count || hoverStates[stateIndex].itemObject != hoverItems[i])
            {
                RebuildHoverStates();
                return;
            }

            stateIndex++;
        }
    }

    private int CountValidHoverItems()
    {
        int count = 0;

        for (int i = 0; i < hoverItems.Count; i++)
        {
            if (hoverItems[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    private HoverState CreateHoverState(GameObject itemObject)
    {
        if (itemObject == null)
        {
            return null;
        }

        HoverState state = new HoverState
        {
            itemObject = itemObject,
            uiGraphic = itemObject.GetComponent<Graphic>() ?? itemObject.GetComponentInChildren<Graphic>(),
            spriteRenderer = itemObject.GetComponent<SpriteRenderer>(),
            collider2D = itemObject.GetComponent<Collider2D>(),
            normalScale = itemObject.transform.localScale
        };

        if (state.uiGraphic == null && state.spriteRenderer == null)
        {
            Debug.LogWarning("Hover item needs an Image, Graphic, or SpriteRenderer: " + itemObject.name);
            return null;
        }

        SetItemColorImmediate(state, hoverParameters.normalColor);

        if (hoverParameters.autoCreateGlowOutline)
        {
            if (state.uiGraphic != null)
            {
                Outline existingOutline = state.uiGraphic.GetComponent<Outline>();

                if (existingOutline != null)
                {
                    existingOutline.enabled = false;
                }

                CreateGlowLayers(state);
            }
            else
            {
                state.glowOutline = FindExistingGlowOutline(state) ?? CreateGlowOutline(state);
            }
        }

        if (state.glowOutline != null)
        {
            state.glowOutline.SetActive(false);
        }

        SetGlowLayersActive(state, false);

        return state;
    }

    private void CreateGlowLayers(HoverState state)
    {
        Image sourceImage = state.uiGraphic as Image;

        if (sourceImage == null)
        {
            return;
        }

        RectTransform sourceRect = sourceImage.rectTransform;
        float glowWidth = GetGlowWidth(sourceRect);
        int layerCount = 6;

        for (int i = 0; i < layerCount; i++)
        {
            float progress = (i + 1f) / layerCount;
            float radius = glowWidth * progress;
            GameObject glow = new GameObject(
                state.itemObject.name + " White Glow " + (i + 1),
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            glow.transform.SetParent(sourceImage.transform, false);
            glow.transform.SetAsFirstSibling();

            RectTransform glowRect = glow.GetComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.anchoredPosition = Vector2.zero;
            glowRect.sizeDelta = sourceRect.rect.size + new Vector2(radius * 2f, radius * 2f);
            glowRect.pivot = new Vector2(0.5f, 0.5f);

            Image glowImage = glow.GetComponent<Image>();
            glowImage.sprite = sourceImage.sprite;
            glowImage.type = sourceImage.type;
            glowImage.preserveAspect = sourceImage.preserveAspect;
            glowImage.raycastTarget = false;

            Shader glowShader = Shader.Find("UI/Outer Gradient Glow");

            if (glowShader == null)
            {
                Destroy(glow);
                continue;
            }

            Material glowMaterial = new Material(glowShader);
            float expandedWidth = sourceRect.rect.width + radius * 2f;
            float expandedHeight = sourceRect.rect.height + radius * 2f;
            glowMaterial.SetVector(
                "_InnerBounds",
                new Vector4(
                    radius / expandedWidth,
                    radius / expandedHeight,
                    1f - radius / expandedWidth,
                    1f - radius / expandedHeight
                )
            );
            glowMaterial.SetColor("_GlowColor", Color.white);
            glowMaterial.SetFloat("_GlowAlpha", 0.3f * (1f - progress) + 0.08f);
            glowImage.material = glowMaterial;

            state.glowLayers.Add(glow);
        }
    }

    private float GetGlowWidth(RectTransform sourceRect)
    {
        if (!hoverParameters.autoCalculateGlowRange)
        {
            return hoverParameters.outlineWidth;
        }

        float shortestSide = Mathf.Min(sourceRect.rect.width, sourceRect.rect.height);
        float calculatedWidth = shortestSide * hoverParameters.glowRangeRatio;

        return Mathf.Clamp(
            calculatedWidth,
            hoverParameters.minimumGlowWidth,
            hoverParameters.maximumGlowWidth
        );
    }

    private void SetGlowLayersActive(HoverState state, bool isActive)
    {
        for (int i = 0; i < state.glowLayers.Count; i++)
        {
            if (state.glowLayers[i] != null)
            {
                state.glowLayers[i].SetActive(isActive);
            }
        }
    }

    private GameObject CreateGlowOutline(HoverState state)
    {
        if (state.uiGraphic is Image sourceImage)
        {
            GameObject outline = new GameObject(
                state.itemObject.name + " Glow Outline",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            outline.transform.SetParent(sourceImage.transform, false);
            outline.transform.SetAsFirstSibling();

            RectTransform outlineRect = outline.GetComponent<RectTransform>();
            outlineRect.anchorMin = Vector2.zero;
            outlineRect.anchorMax = Vector2.one;
            outlineRect.offsetMin = Vector2.zero;
            outlineRect.offsetMax = Vector2.zero;
            outlineRect.pivot = sourceImage.rectTransform.pivot;
            outlineRect.localScale = Vector3.one * hoverParameters.glowScale;

            Image outlineImage = outline.GetComponent<Image>();
            outlineImage.sprite = sourceImage.sprite;
            outlineImage.type = sourceImage.type;
            outlineImage.preserveAspect = sourceImage.preserveAspect;
            outlineImage.color = hoverParameters.glowColor;
            outlineImage.raycastTarget = false;

            return outline;
        }

        if (state.spriteRenderer != null)
        {
            GameObject outline = new GameObject(state.itemObject.name + " Glow Outline");
            outline.transform.SetParent(state.itemObject.transform, false);
            outline.transform.localPosition = Vector3.zero;
            outline.transform.localRotation = Quaternion.identity;
            outline.transform.localScale = Vector3.one * hoverParameters.glowScale;

            SpriteRenderer outlineRenderer = outline.AddComponent<SpriteRenderer>();
            outlineRenderer.sprite = state.spriteRenderer.sprite;
            outlineRenderer.color = hoverParameters.glowColor;
            outlineRenderer.sortingLayerID = state.spriteRenderer.sortingLayerID;
            outlineRenderer.sortingOrder = state.spriteRenderer.sortingOrder - 1;

            return outline;
        }

        return null;
    }

    private GameObject FindExistingGlowOutline(HoverState state)
    {
        string outlineName = state.itemObject.name + " Glow Outline";
        Transform parent = state.uiGraphic != null ? state.uiGraphic.transform : state.itemObject.transform;
        Transform existingOutline = parent.Find(outlineName);

        return existingOutline != null ? existingOutline.gameObject : null;
    }

    private bool IsPointerOverItem(HoverState state, Vector2 mousePosition)
    {
        if (state.uiGraphic != null)
        {
            Camera eventCamera = GetEventCamera(state.uiGraphic);
            return RectTransformUtility.RectangleContainsScreenPoint(
                state.uiGraphic.rectTransform,
                mousePosition,
                eventCamera
            );
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return false;
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        if (state.collider2D != null)
        {
            return state.collider2D.OverlapPoint(worldPosition);
        }

        return state.spriteRenderer != null && state.spriteRenderer.bounds.Contains(worldPosition);
    }

    private Camera GetEventCamera(Graphic graphic)
    {
        Canvas canvas = graphic.GetComponentInParent<Canvas>();

        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera;
    }

    private void ApplyHoverVisual(HoverState state)
    {
        Color targetColor = state.isHovered ? hoverParameters.hoverColor : hoverParameters.normalColor;
        Vector3 targetScale = state.isHovered
            ? state.normalScale * hoverParameters.hoverScale
            : state.normalScale;

        SetItemColor(state, targetColor);

        state.itemObject.transform.localScale = Vector3.Lerp(
            state.itemObject.transform.localScale,
            targetScale,
            Time.deltaTime * hoverParameters.transitionSpeed
        );

        if (state.glowOutline != null)
        {
            state.glowOutline.SetActive(state.isHovered);
        }

        SetGlowLayersActive(state, state.isHovered);
    }

    private void SetItemColor(HoverState state, Color targetColor)
    {
        if (state.uiGraphic != null)
        {
            state.uiGraphic.color = Color.Lerp(
                state.uiGraphic.color,
                targetColor,
                Time.deltaTime * hoverParameters.transitionSpeed
            );
        }

        if (state.spriteRenderer != null)
        {
            state.spriteRenderer.color = Color.Lerp(
                state.spriteRenderer.color,
                targetColor,
                Time.deltaTime * hoverParameters.transitionSpeed
            );
        }
    }

    private void SetItemColorImmediate(HoverState state, Color targetColor)
    {
        if (state.uiGraphic != null)
        {
            state.uiGraphic.color = targetColor;
        }

        if (state.spriteRenderer != null)
        {
            state.spriteRenderer.color = targetColor;
        }
    }

    private Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif

        return Input.mousePosition;
    }

    private void EnsureHoverParameters()
    {
        if (hoverParameters == null)
        {
            hoverParameters = new HoverParameters();
        }
    }

}
