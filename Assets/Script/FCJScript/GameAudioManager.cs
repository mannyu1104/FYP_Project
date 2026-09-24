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

    [Header("Simple Game Audio")]
    [SerializeField] private AudioClip menuBgm;
    [SerializeField] private AudioClip tutorialBgm;
    [SerializeField] private AudioClip locationFootstepsSfx;
    [Tooltip("Footstep loudness multiplier. 1 = original, 2 = twice the amplitude. Still follows Master and SFX volume.")]
    [Range(0f, 4f)] [SerializeField] private float locationFootstepsVolume = 1f;
    [SerializeField] private AudioClip dialogueTypingSfx;
    [SerializeField] private AudioClip doorKnockSfx;
    [Range(-1f, 1f)] [SerializeField] private float doorKnockPan = .7f;
    [Range(0f, 1f)] [SerializeField] private float doorKnockVolume = 1f;
    private AudioSource doorKnockSource;

    public void PlayDoorKnock()
    {
        if (doorKnockSfx == null) return;
        if (doorKnockSource == null) doorKnockSource = GetOrCreateAudioSource("Door Knock Source");
        doorKnockSource.playOnAwake = false;
        doorKnockSource.loop = false;
        doorKnockSource.spatialBlend = 0f;
        doorKnockSource.panStereo = doorKnockPan;
        doorKnockSource.volume = masterVolume * sfxVolume * doorKnockVolume;
        doorKnockSource.clip = doorKnockSfx;
        doorKnockSource.Play();
    }

    [SerializeField] private bool autoButtonSounds = true;
    public AudioClip DialogueTypingSfx => dialogueTypingSfx;
    private AudioSource typingSource;
    public void PlayDialogueTyping(AudioClip clip)
    {
        if (clip == null) return;
        if (typingSource == null) typingSource = GetOrCreateAudioSource("Dialogue Typing Source");
        if (typingSource.isPlaying && typingSource.clip == clip) return;
        typingSource.Stop();
        typingSource.playOnAwake = false;
        typingSource.loop = false;
        typingSource.spatialBlend = 0f;
        typingSource.volume = masterVolume * sfxVolume;
        typingSource.clip = clip;
        typingSource.Play();
    }
    public void StopDialogueTyping()
    {
        if (typingSource != null) typingSource.Stop();
    }

    private float nextButtonScan;
    private AudioClip selectedBgm;
    public void PlayLocationFootsteps()
    {
        if (locationFootstepsSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(locationFootstepsSfx, locationFootstepsVolume);
    }

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
        // Scene-owned references are rebuilt on new game; volumes persist in PlayerPrefs.

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
        if (autoButtonSounds && Time.unscaledTime >= nextButtonScan)
        {
            nextButtonScan = Time.unscaledTime + .5f;
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include))
                if (button.GetComponent<ButtonSoundPlayer>() == null && button.GetComponent<CustomButtonUi>() == null)
                    button.gameObject.AddComponent<ButtonSoundPlayer>();
        }
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

        sfxSource.PlayOneShot(clip);
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
        var flow = InvestigationFlowController.Instance;
        AudioClip desired = null;
        if (flow != null)
        {
            if (flow.CurrentStage == InvestigationFlowController.Stage.Menu) desired = menuBgm;
            else if (flow.CurrentStage == InvestigationFlowController.Stage.Introduction ||
                     flow.CurrentStage == InvestigationFlowController.Stage.AwaitTutorial ||
                     flow.CurrentStage == InvestigationFlowController.Stage.Tutorial) desired = tutorialBgm;
        }
        var panel = FindActivePanelBgm();
        if (desired == null) desired = panel != null ? panel.bgmClip : defaultBgm;
        if (!forceRefresh && selectedBgm == desired) return;
        selectedBgm = desired;
        if (desired != null) PlayBgm(desired);
        else if (stopBgmWhenNoPanelMatches) StopBgm();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private PanelBgm FindActivePanelBgm()
    {
        for (int i = 0; i < panelBgms.Count; i++)
        {
            PanelBgm panelBgm = panelBgms[i];
            if (panelBgm != null && panelBgm.panel != null && panelBgm.panel.activeInHierarchy && panelBgm.bgmClip != null)
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
        if (typingSource != null) typingSource.volume = masterVolume * sfxVolume;
        if (doorKnockSource != null) doorKnockSource.volume = masterVolume * sfxVolume * doorKnockVolume;
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
