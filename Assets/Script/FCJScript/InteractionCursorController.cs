using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class InteractionCursorController : MonoBehaviour
{
    [System.Serializable]
    public class CursorPreset
    {
        public string presetName = "View";
        public Texture2D cursorTexture;
        public Vector2 hotspot;
        public CursorMode cursorMode = CursorMode.Auto;
    }

    [Header("Cursor Presets")]
    [Tooltip("Add, remove, rename, and configure every cursor used by the game here.")]
    public List<CursorPreset> cursorPresets = new List<CursorPreset>();

    [Header("Detection")]
    public Camera worldCamera;
    public bool pauseDuringDialogue = true;
    [Tooltip("When any of these panels are active, the cursor will not react to gameplay objects behind the UI.")]
    [SerializeField] private List<GameObject> blockingUiPanels = new List<GameObject>();
    [SerializeField] private List<string> blockingUiPanelNames = new List<string>
    {
        "SettingsPanel",
        "MainMenuPanel",
        "HistoryPanel"
    };

    private CursorPreset activePreset;
    private bool isPaused;
    private readonly HashSet<Texture2D> warnedTextures = new HashSet<Texture2D>();

    [HideInInspector]
    public string viewPresetName = "View";
    [HideInInspector]
    public string dialoguePresetName = "talk";
    [HideInInspector]
    public int viewPresetIndex;
    [HideInInspector]
    public int dialoguePresetIndex = 1;

    void OnValidate()
    {
        EnsureDefaultPresets();
    }

    void Awake()
    {
        EnsureDefaultPresets();

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        ResetCursor();
    }

    private void EnsureDefaultPresets()
    {
        if (cursorPresets == null)
        {
            cursorPresets = new List<CursorPreset>();
        }

        if (cursorPresets.Count > 0 && cursorPresets[0] != null && string.IsNullOrEmpty(cursorPresets[0].presetName))
        {
            cursorPresets[0].presetName = "View";
        }

        if (cursorPresets.Count > 1 && cursorPresets[1] != null && string.IsNullOrEmpty(cursorPresets[1].presetName))
        {
            cursorPresets[1].presetName = "Talk";
        }

        if (cursorPresets.Count > 2 && cursorPresets[2] != null && string.IsNullOrEmpty(cursorPresets[2].presetName))
        {
            cursorPresets[2].presetName = "Custom";
        }
    }

    void Update()
    {
        if (pauseDuringDialogue)
        {
            DialogueController dialogueController = FindAnyObjectByType<DialogueController>();
            isPaused = dialogueController != null && dialogueController.IsDialogueActive;
        }

        if (IsBlockingUiOpen())
        {
            ResetCursor();
            return;
        }

        bool anyOverlayOpen = WhiteBoard.IsAnyWhiteBoardOpen || MapButton.IsAnyMapOpen;
        if (anyOverlayOpen)
        {
            CursorInteractionTarget uiTarget = FindUiTargetUnderPointer();
            if (uiTarget != null)
            {
                if (uiTarget.GetComponentInParent<Button>() != null || uiTarget.GetComponentInParent<Selectable>() != null)
                {
                    CursorPreset uiPreset = GetPresetForTarget(uiTarget);
                    SetCursor(uiPreset);
                    return;
                }
            }

            ResetCursor();
            return;
        }

        if (isPaused)
        {
            ResetCursor();
            return;
        }

        CursorInteractionTarget target = FindTargetUnderPointer();
        CursorPreset preset = GetPresetForTarget(target);
        SetCursor(preset);
    }

    public void ResetCursor()
    {
        activePreset = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private CursorInteractionTarget FindTargetUnderPointer()
    {
        CursorInteractionTarget uiTarget = FindUiTargetUnderPointer();
        if (uiTarget != null)
        {
            return uiTarget;
        }

        if (worldCamera == null)
        {
            return null;
        }

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D collider = Physics2D.OverlapPoint(worldPosition);
        return collider != null ? collider.GetComponentInParent<CursorInteractionTarget>() : null;
    }

    private CursorInteractionTarget FindUiTargetUnderPointer()
    {
        if (EventSystem.current == null)
        {
            return null;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        for (int i = 0; i < results.Count; i++)
        {
            CursorInteractionTarget target = results[i].gameObject.GetComponentInParent<CursorInteractionTarget>();
            if (target != null)
            {
                return target;
            }

            NPCDialogueTrigger npc = results[i].gameObject.GetComponentInParent<NPCDialogueTrigger>();
            if (npc != null)
            {
                return npc.GetComponent<CursorInteractionTarget>();
            }
        }

        return null;
    }

    private bool IsBlockingUiOpen()
    {
        for (int i = 0; i < blockingUiPanels.Count; i++)
        {
            GameObject panel = blockingUiPanels[i];
            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }

        for (int i = 0; i < blockingUiPanelNames.Count; i++)
        {
            GameObject panel = FindSceneObjectByName(blockingUiPanelNames[i]);
            if (panel != null)
            {
                if (!blockingUiPanels.Contains(panel))
                {
                    blockingUiPanels.Add(panel);
                }

                if (panel.activeInHierarchy)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform target = transforms[i];
            if (target != null && target.name == objectName && target.gameObject.scene.IsValid())
            {
                return target.gameObject;
            }
        }

        return null;
    }

    private CursorPreset GetPresetForTarget(CursorInteractionTarget target)
    {
        if (target == null)
        {
            return null;
        }

        string presetName = !string.IsNullOrWhiteSpace(target.cursorPresetName)
            ? target.cursorPresetName
            : target.customCursorPresetName;

        CursorPreset preset = GetPresetByName(presetName);
        if (preset != null)
        {
            return preset;
        }

        if (target.interactionType == 1)
        {
            preset = GetPresetByName(dialoguePresetName);
            return preset != null ? preset : GetPreset(dialoguePresetIndex);
        }

        preset = GetPresetByName(viewPresetName);
        return preset != null ? preset : GetPreset(viewPresetIndex);
    }

    private CursorPreset GetPreset(int index)
    {
        return index >= 0 && index < cursorPresets.Count ? cursorPresets[index] : null;
    }

    private CursorPreset GetPresetByName(string presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return null;
        }

        for (int i = 0; i < cursorPresets.Count; i++)
        {
            if (cursorPresets[i] != null &&
                string.Equals(cursorPresets[i].presetName, presetName, StringComparison.OrdinalIgnoreCase))
            {
                return cursorPresets[i];
            }
        }

        return null;
    }

    private void SetCursor(CursorPreset preset)
    {
        if (preset == activePreset)
        {
            return;
        }

        activePreset = preset;

        if (preset == null)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        if (preset.cursorTexture == null)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        if (!preset.cursorTexture.isReadable)
        {
            if (warnedTextures.Add(preset.cursorTexture))
            {
                Debug.LogWarning(
                    "Cursor texture is not CPU-readable: " + preset.cursorTexture.name +
                    ". Enable Read/Write in its Texture Import Settings."
                );
            }

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        Cursor.SetCursor(preset.cursorTexture, preset.hotspot, preset.cursorMode);
    }
}
