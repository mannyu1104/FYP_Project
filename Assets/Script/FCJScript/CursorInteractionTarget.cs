using UnityEngine;

public class CursorInteractionTarget : MonoBehaviour
{
    [Header("Cursor Selection")]
    [Tooltip("Choose a preset from the Cursor Presets list in CursorManager.")]
    public string cursorPresetName;

    [HideInInspector]
    public string customCursorPresetName;
    [HideInInspector]
    public int customCursorPresetIndex;
    [HideInInspector]
    public int interactionType;
    [HideInInspector]
    public bool useCustomPreset;
    [HideInInspector]
    public int cursorPresetIndex;

}
