using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CustomButtonUi : Selectable, IPointerClickHandler
{
    [Header("Click Events")]
    public UnityEvent onLeftClick;
    public UnityEvent onRightClick;
    public UnityEvent onDoubleClick;

    [Header("Double Click")]
    [SerializeField] private float doubleClickWindow = 0.35f;

    private bool forcedHighlight = false;

    private float lastClickTime = -1f;

    protected override void Awake()
    {
        base.Awake();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Image>();
    }

    public void SetForcedHighlight(bool highlighted)
    {
        if (forcedHighlight == highlighted) return;
        forcedHighlight = highlighted;
        RefreshVisualState(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                HandleLeftClick();
                break;
            case PointerEventData.InputButton.Right:
                onRightClick?.Invoke();
                break;
        }
    }

    private void HandleLeftClick()
    {
        float timeSinceLastClick = Time.unscaledTime - lastClickTime;
        lastClickTime = Time.unscaledTime;

        if (timeSinceLastClick <= doubleClickWindow)
        {
            onDoubleClick?.Invoke();
            lastClickTime = -1f;
        }
        else
        {
            onLeftClick?.Invoke();
        }
    }

    private bool hovered = false;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        hovered = true;
        RefreshVisualState(false);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        hovered = false;
        RefreshVisualState(false);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        forcedHighlight = false;
        RefreshVisualState(false);
    }

    private void RefreshVisualState(bool instant)
    {
        SelectionState state;

        if (forcedHighlight)
            state = SelectionState.Selected;
        else if (IsPointerInside())
            state = SelectionState.Highlighted;
        else
            state = SelectionState.Normal;

        DoStateTransition(state, instant);
    }

    private bool IsPointerInside() => hovered;

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        RefreshVisualState(false);
    }
}