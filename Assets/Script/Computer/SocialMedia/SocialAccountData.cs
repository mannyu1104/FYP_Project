using System;
using UnityEngine;
using UnityEngine.Localization;

// Social media account, just icon, name and profile
[CreateAssetMenu(fileName = "NewSocialAccount", menuName = "ScriptableObject/Social Account")]
public class SocialAccountData : ScriptableObject
{
    [Header("Account Details")]
    [SerializeField] private LocalizedString accountName;
    [SerializeField] private Sprite avatar;
    //[SerializeField][TextArea(2, 5)] private string bio;
    [SerializeField] private LocalizedString bio;

    public LocalizedString AccountName => accountName;
    public Sprite Avatar => avatar;
    public LocalizedString Bio => bio;
}