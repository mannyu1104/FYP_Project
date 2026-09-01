using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connects settings menu sliders to the global audio manager.
/// </summary>
public class AudioVolumeSettingsUI : MonoBehaviour
{
    [SerializeField] private GameAudioManager audioManager;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Awake()
    {
        ResolveAudioManager();
        BindSliders();
    }

    private void OnEnable()
    {
        ResolveAudioManager();
        BindSliders();
    }

    private void OnDisable()
    {
        RemoveSliderListeners();
    }

    private void OnValidate()
    {
        ConfigureSlider(masterVolumeSlider);
        ConfigureSlider(bgmVolumeSlider);
        ConfigureSlider(sfxVolumeSlider);
    }

    private void BindSliders()
    {
        if (audioManager == null)
        {
            return;
        }

        RemoveSliderListeners();

        ConfigureSlider(masterVolumeSlider);
        ConfigureSlider(bgmVolumeSlider);
        ConfigureSlider(sfxVolumeSlider);

        SetSliderValue(masterVolumeSlider, audioManager.MasterVolume);
        SetSliderValue(bgmVolumeSlider, audioManager.BgmVolume);
        SetSliderValue(sfxVolumeSlider, audioManager.SfxVolume);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(audioManager.SetMasterVolume);
        }

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(audioManager.SetBgmVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(audioManager.SetSfxVolume);
        }
    }

    private void RemoveSliderListeners()
    {
        if (audioManager == null)
        {
            return;
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(audioManager.SetMasterVolume);
        }

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.RemoveListener(audioManager.SetBgmVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(audioManager.SetSfxVolume);
        }
    }

    private void ResolveAudioManager()
    {
        if (audioManager == null)
        {
            audioManager = GameAudioManager.Instance != null
                ? GameAudioManager.Instance
                : FindAnyObjectByType<GameAudioManager>();
        }
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void SetSliderValue(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
        }
    }
}
