using UnityEngine;

// Data entry that can appear in search results.
[CreateAssetMenu(fileName = "NewSearchResultEntry", menuName = "ScriptableObject/Search Result Entry")]
public class SearchResultEntryData : ScriptableObject
{
    [Header("Search Matching")]
    public string[] keywords;

    [Header("Search Result List Display")]
    public string resultTitle;

    [Header("Destination")]
    public WebPageDataScriptableObject customPageData;

    public NewsArticleData newsArticleRef; // Will open the News app then jump straight to this article.
    public SocialAccountData socialProfileRef; // Will open the Social app then jump straight to this profile.

    /// <summary>
    /// Returns true if this entry should show up for the given search query.
    /// Uses simple two-way substring matching so both partial typing and
    /// keyword-contains-query cases work (e.g. query "孤儿院" matches keyword "希望孤儿院").
    /// </summary>
    public bool MatchesQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || keywords == null) return false;

        string normalizedQuery = query.Trim().ToLowerInvariant();
        if (normalizedQuery.Length == 0) return false;

        foreach (string keyword in keywords)
        {
            if (string.IsNullOrWhiteSpace(keyword)) continue;
            string normalizedKeyword = keyword.Trim().ToLowerInvariant();

            if (normalizedKeyword.Contains(normalizedQuery) || normalizedQuery.Contains(normalizedKeyword))
            {
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        int filledCount = 0;
        if (customPageData != null) filledCount++;
        if (newsArticleRef != null) filledCount++;
        if (socialProfileRef != null) filledCount++;

        if (filledCount == 0)
        {
            Debug.LogWarning($"[SearchResultEntryData] '{name}' has no destination assigned (customPageData / newsArticleRef / socialProfileRef are all empty).", this);
        }
        else if (filledCount > 1)
        {
            Debug.LogWarning($"[SearchResultEntryData] '{name}' has more than one destination assigned - only fill in ONE of customPageData / newsArticleRef / socialProfileRef.", this);
        }
    }
#endif
}