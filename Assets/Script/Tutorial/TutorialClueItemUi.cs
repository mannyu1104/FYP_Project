using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Tutorial clue item prefab
[RequireComponent(typeof(CustomButtonUi))]
public class TutorialClueItemUi : MonoBehaviour
{
    [SerializeField] private CustomButtonUi customButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image clueImage;

    [SerializeField] private TutorialClueData clueData;
    public TutorialClueData ClueData => clueData;

    private void Reset()
    {
        customButton = GetComponent<CustomButtonUi>();
    }

    
    public void Bind(TutorialPageController owner)
    {
        UnsubscribeFromLocalization();

        if (clueData == null)
        {
            Debug.LogWarning("TutorialClueItemUI has no Clue Data assigned.", this);
            return;
        }

        SubscribeToLocalization();

        if (clueImage != null)
        {
            clueImage.sprite = clueData.ClueImage;
        }

        customButton.onLeftClick.RemoveAllListeners();
        customButton.onLeftClick.AddListener(() => owner.ShowTutorialClueDetail(clueData));
    }

    private void SubscribeToLocalization()
    {
        if (clueData == null) return;
        clueData.TutorialClueName.StringChanged += UpdateNameText;
    }

    private void UnsubscribeFromLocalization()
    {
        if (clueData == null) return;
        clueData.TutorialClueName.StringChanged -= UpdateNameText;
    }

    private void UpdateNameText(string value)
    {
        if (nameText != null) nameText.text = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromLocalization();
    }
}