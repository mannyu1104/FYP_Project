using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optional click sound helper for UI buttons.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundPlayer : MonoBehaviour
{
    [SerializeField] private bool playSound = true;
    [SerializeField] private bool useDefaultButtonSfx = true;
    [SerializeField] private AudioClip customClickSfx;

    private Button button;

    private void Awake()
    {
        BindButton();
    }

    private void OnEnable()
    {
        BindButton();
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    public void SetPlaySound(bool enabled)
    {
        playSound = enabled;
    }

    public void PlayClickSound()
    {
        if (!playSound || GameAudioManager.Instance == null)
        {
            return;
        }

        if (useDefaultButtonSfx)
        {
            GameAudioManager.Instance.PlayButtonSfx();
            return;
        }

        GameAudioManager.Instance.PlaySfx(customClickSfx);
    }

    private void BindButton()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(PlayClickSound);
        button.onClick.AddListener(PlayClickSound);
    }
}
