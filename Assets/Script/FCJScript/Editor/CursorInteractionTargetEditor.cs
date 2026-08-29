using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CursorInteractionTarget))]
public class CursorInteractionTargetEditor : Editor
{
    private SerializedProperty cursorPresetName;

    void OnEnable()
    {
        cursorPresetName = serializedObject.FindProperty("cursorPresetName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        if (string.IsNullOrWhiteSpace(cursorPresetName.stringValue))
        {
            SerializedProperty legacyName = serializedObject.FindProperty("customCursorPresetName");
            if (legacyName != null && !string.IsNullOrWhiteSpace(legacyName.stringValue))
            {
                cursorPresetName.stringValue = legacyName.stringValue;
            }
        }

        DrawPresetSelector();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPresetSelector()
    {
        InteractionCursorController controller = FindFirstObjectByType<InteractionCursorController>();

        if (controller == null || controller.cursorPresets == null || controller.cursorPresets.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No Cursor Presets found. Add presets to InteractionCursorController first.",
                MessageType.Warning
            );
            return;
        }

        List<string> presetNames = new List<string>();
        int selectedIndex = 0;

        for (int i = 0; i < controller.cursorPresets.Count; i++)
        {
            InteractionCursorController.CursorPreset preset = controller.cursorPresets[i];
            string presetName = preset != null && !string.IsNullOrWhiteSpace(preset.presetName)
                ? preset.presetName
                : "Preset " + i;

            presetNames.Add(presetName);

            if (presetName == cursorPresetName.stringValue)
            {
                selectedIndex = i;
            }
        }

        int newIndex = EditorGUILayout.Popup("Cursor Preset", selectedIndex, presetNames.ToArray());
        cursorPresetName.stringValue = presetNames[newIndex];
    }
}