using UnityEngine;

// Social media account, just icon, name and profile
[CreateAssetMenu(fileName = "NewSocialAccount", menuName = "ScriptableObject/Social Account")]
public class SocialAccountData : ScriptableObject
{
    [SerializeField] private string accountName;
    [SerializeField] private Sprite avatar;
    [SerializeField][TextArea(2, 5)] private string bio;

    public string AccountName => accountName;
    public Sprite Avatar => avatar;
    public string Bio => bio;
}