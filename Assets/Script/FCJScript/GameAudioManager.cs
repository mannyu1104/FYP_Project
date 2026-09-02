using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps global BGM, SFX, and volume settings available across the main game.
/// </summary>
public class GameAudioManager : MonoBehaviour
{
    private const string MasterVolumeKey = "Audio.MasterVolume";
    private const string BgmVolumeKey = "Audio.BgmVolume";
    private const string SfxVolumeKey = "Audio.SfxVolume";

    public static GameAudioManager Instance { get; private set; }

    [System.Serializable]
    private class PanelBgm
    {
        [Tooltip("The gameplay panel that should trigger this BGM when it is active.")]
        public GameObject panel;
        public AudioClip bgmClip;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default Clips")]
    [SerializeField] private AudioClip defaultButtonSfx;
    [Tooltip("Optional BGM used when no panel BGM matches.")]
    [SerializeField] private AudioClip defaultBgm;

    [Header("Panel BGM")]
    [Tooltip("First active panel in this list decides the current BGM.")]
    [SerializeField] private List<PanelBgm> panelBgms = new List<PanelBgm>();
    [SerializeField] private bool stopBgmWhenNoPanelMatches;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Settings Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private bool isRefreshingSliders;
    private GameObject currentBgmPanel;

    public float MasterVolume => masterVolume;
    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        LoadVolumes();
        BindSliders();
        ApplyVolumes();
    }

    private void Start()
    {
        RefreshPanelBgm(true);
    }

    private void Update()
    {
        RefreshPanelBgm(false);
    }

    private void OnValidate()
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        bgmVolume = Mathf.Clamp01(bgmVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);

        ApplyVolumes();
        RefreshSliders();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        SaveAndApplyVolumes();
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        SaveAndApplyVolumes();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveAndApplyVolumes();
    }

    public void PlayButtonSfx()
    {
        PlaySfx(defaultButtonSfx);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, masterVolume * sfxVolume);
    }

    public void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null)
        {
            return;
        }

        if (clip == null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void BindSliders(Slider masterSlider, Slider bgmSlider, Slider sfxSlider)
    {
        masterVolumeSlider = masterSlider;
        bgmVolumeSlider = bgmSlider;
        sfxVolumeSlider = sfxSlider;
        BindSliders();
    }

    public void RefreshPanelBgm()
    {
        RefreshPanelBgm(true);
    }

    private void RefreshPanelBgm(bool forceRefresh)
    {
        PanelBgm activePanelBgm = FindActivePanelBgm();
        GameObject activePanel = activePanelBgm != null ? activePanelBgm.panel : null;

        if (!forceRefresh && activePanel == currentBgmPanel)
        {
            return;
        }

        currentBgmPanel = activePanel;

        if (activePanelBgm != null)
        {
            PlayBgm(activePanelBgm.bgmClip);
            return;
        }

        if (defaultBgm != null)
        {
            PlayBgm(defaultBgm);
            return;
        }

        if (stopBgmWhenNoPanelMatches)
        {
            StopBgm();
        }
    }

    private PanelBgm FindActivePanelBgm()
    {
        for (int i = 0; i < panelBgms.Count; i++)
        {
            PanelBgm panelBgm = panelBgms[i];
            if (panelBgm != null && panelBgm.panel != null && panelBgm.panel.activeInHierarchy)
            {
                return panelBgm;
            }
        }

        return null;
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = GetOrCreateAudioSource("BGM Source");
        }

        if (sfxSource == null)
        {
            sfxSource = GetOrCreateAudioSource("SFX Source");
        }

        if (bgmSource != null)
        {
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    private AudioSource GetOrCreateAudioSource(string sourceName)
    {
        Transform child = transform.Find(sourceName);
        if (child == null)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform);
            child = sourceObject.transform;
        }

        AudioSource audioSource = child.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = child.gameObject.AddComponent<AudioSource>();
        }

        return audioSource;
    }

    private void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, masterVolume);
        bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, bgmVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume);
    }

    private void SaveAndApplyVolumes()
    {
        ApplyVolumes();
        SaveVolumes();
        RefreshSliders();
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume * bgmVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }

    private void BindSliders()
    {
        BindSlider(masterVolumeSlider, SetMasterVolume);
        BindSlider(bgmVolumeSlider, SetBgmVolume);
        BindSlider(sfxVolumeSlider, SetSfxVolume);
        RefreshSliders();
    }

    private void BindSlider(Slider slider, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.onValueChanged.RemoveListener(callback);
        slider.onValueChanged.AddListener(callback);
    }

    private void RefreshSliders()
    {
        if (isRefreshingSliders)
        {
            return;
        }

        isRefreshingSliders = true;
        SetSliderValue(masterVolumeSlider, masterVolume);
        SetSliderValue(bgmVolumeSlider, bgmVolume);
        SetSliderValue(sfxVolumeSlider, sfxVolume);
        isRefreshingSliders = false;
    }

    private void SetSliderValue(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
        }
    }
}
