using UnityEngine;

/// <summary>
/// Triggers a glow effect + appearance change when the mouse hovers over the object.
/// Attach to an object that has a SpriteRenderer and a Collider2D.
/// (OnMouseEnter/Exit relies on Collider2D, no EventSystem required)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class HoverEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [Tooltip("Color in the normal (non-hovered) state")]
    public Color normalColor = Color.white;
    [Tooltip("Color while hovered. RGB values can exceed 1 (combined with HDR + Bloom post-processing this produces a real glow effect)")]
    public Color hoverColor = new Color(1.4f, 1.4f, 1.4f, 1f);
    [Tooltip("Optional: a slightly larger, semi-transparent child object used as a glow outline. Leave empty if not needed")]
    public GameObject glowOutline;

    [Header("Shape/Sprite Change Settings")]
    public Sprite normalSprite;
    public Sprite hoverSprite;
    [Tooltip("Scale multiplier while hovered, 1 = no scaling")]
    public float hoverScale = 1.08f;

    [Header("Transition Speed")]
    public float transitionSpeed = 8f;

    private SpriteRenderer sr;
    private Vector3 normalScale;
    private Vector3 targetScale;
    private Color targetColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        normalScale = transform.localScale;
        targetScale = normalScale;
        targetColor = normalColor;
        sr.color = normalColor;

        if (normalSprite != null) sr.sprite = normalSprite;
        if (glowOutline != null) glowOutline.SetActive(false);
    }

    void Update()
    {
        // Smoothly interpolate to avoid instant snapping
        sr.color = Color.Lerp(sr.color, targetColor, Time.deltaTime * transitionSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
    }

    // Called when the mouse enters the collider
    void OnMouseEnter()
    {
        targetColor = hoverColor;
        targetScale = normalScale * hoverScale;
        if (hoverSprite != null) sr.sprite = hoverSprite;
        if (glowOutline != null) glowOutline.SetActive(true);
    }

    // Called when the mouse exits the collider
    void OnMouseExit()
    {
        targetColor = normalColor;
        targetScale = normalScale;
        if (normalSprite != null) sr.sprite = normalSprite;
        if (glowOutline != null) glowOutline.SetActive(false);
    }
}
