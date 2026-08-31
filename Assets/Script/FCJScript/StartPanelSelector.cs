using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Selects which panel is visible when the scene starts.
/// </summary>
public class StartPanelSelector : MonoBehaviour
{
    [Header("Start Panel")]
    [Tooltip("The panel index that will be opened when the game starts.")]
    [Min(0)]
    [SerializeField] private int startingPanelIndex;

    [Tooltip("If enabled, the selected panel will be opened in Start().")]
    [SerializeField] private bool openOnStart = true;

    [Header("Panels")]
    [Tooltip("Panels that can be opened. The list index is the panel number.")]
    [SerializeField] private List<GameObject> panels = new List<GameObject>();

    private void Start()
    {
        if (openOnStart)
        {
            OpenStartingPanel();
        }
    }

    private void OnValidate()
    {
        startingPanelIndex = Mathf.Max(0, startingPanelIndex);
    }

    public void OpenStartingPanel()
    {
        OpenPanel(startingPanelIndex);
    }

    public void OpenPanel(int panelIndex)
    {
        if (panels.Count == 0)
        {
            return;
        }

        int clampedIndex = Mathf.Clamp(panelIndex, 0, panels.Count - 1);

        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(i == clampedIndex);
            }
        }
    }
}
