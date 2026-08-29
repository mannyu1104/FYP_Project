using System;
using System.Collections.Generic;
using UnityEngine;

// Search-results tab's data. Implements IBrowserPage directly (unlike
public class SearchResultsTabPage : IBrowserPage
{
    public string PageId { get; }
    public string TabTitle { get; }
    public Sprite TabIcon { get; }

    public string Query { get; }
    public List<SearchResultEntryData> Results { get; }

    public SearchResultsTabPage(string query, List<SearchResultEntryData> results, Sprite tabIcon)
    {
        PageId = Guid.NewGuid().ToString();
        Query = query;
        Results = results;
        TabIcon = tabIcon;
        TabTitle = $"搜索: {query}";
    }
}