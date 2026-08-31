using System.Collections.Generic;
using UnityEngine;

public class LayerSwitch : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public string layerName;

        [Tooltip("这个图层需要显示的物品")]
        public List<GameObject> objects = new List<GameObject>();
    }

    [Header("Layers")]
    [SerializeField]
    private List<Layer> layers = new List<Layer>();

    [Header("Starting Layer")]
    [SerializeField]
    private int startingLayer = 0;

    private int currentLayer = -1;

    private void Awake()
    {
        ShowLayer(startingLayer);
    }

    public int CurrentLayerIndex => currentLayer;

    public string CurrentLayerName
    {
        get
        {
            if (currentLayer < 0 || currentLayer >= layers.Count)
            {
                return string.Empty;
            }

            return layers[currentLayer].layerName;
        }
    }

    public void ShowLayer(int layerIndex)
    {
        if (layers.Count == 0)
        {
            return;
        }

        if (layerIndex < 0 || layerIndex >= layers.Count)
        {
            Debug.LogWarning(
                $"LayerSwitch: Layer index {layerIndex} is out of range.",
                this
            );
            return;
        }

        for (int i = 0; i < layers.Count; i++)
        {
            bool shouldShow = i == layerIndex;

            foreach (GameObject obj in layers[i].objects)
            {
                if (obj != null)
                {
                    obj.SetActive(shouldShow);
                }
            }
        }

        currentLayer = layerIndex;
    }

    public void ShowLayerByName(string layerName)
    {
        if (string.IsNullOrWhiteSpace(layerName))
        {
            Debug.LogWarning("LayerSwitch: layerName is empty.", this);
            return;
        }

        for (int i = 0; i < layers.Count; i++)
        {
            if (string.Equals(layers[i].layerName, layerName, System.StringComparison.OrdinalIgnoreCase))
            {
                ShowLayer(i);
                return;
            }
        }

        Debug.LogWarning($"LayerSwitch: Could not find layer named '{layerName}'.", this);
    }

    public void GoToOrphanage()
    {
        ShowLayer(startingLayer);
    }

    public void GoToEntrance()
    {
        ShowLayerByName("入口");
    }

    public void GoToStaffRoom()
    {
        ShowLayerByName("职员室");
    }

    public void OpenLayer()
    {
        if (layers.Count > 1)
        {
            ShowLayer(1);
            return;
        }

        ShowLayer(startingLayer);
    }

    public void BackToOriginalLayer()
    {
        GoToOrphanage();
    }
}