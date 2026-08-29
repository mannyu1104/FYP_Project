using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SearchData", menuName = "ScriptableObject/Search Database")]
public class SearchData : ScriptableObject
{
    public List<SearchResultEntryData> allEntries = new List<SearchResultEntryData>();

    // Searches for entries that match the given query and returns a list of matching entries.
    public List<SearchResultEntryData> Search(string query)
    {
        List<SearchResultEntryData> results = new List<SearchResultEntryData>();
        if (string.IsNullOrWhiteSpace(query)) return results;

        foreach (SearchResultEntryData entry in allEntries)
        {
            if (entry != null && entry.MatchesQuery(query))
            {
                results.Add(entry);
            }
        }
        return results;
    }
}