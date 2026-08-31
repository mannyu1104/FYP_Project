using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

// Search-results tab's data. Implements IBrowserPage directly (unlike
public class SearchResultsTabPage : IBrowserPage
{
    public string PageId { get; }
    public LocalizedString TabTitle { get; }
    public Sprite TabIcon { get; }
    public string Query { get; }
    public List<SearchResultEntryData> Results { get; }


    public SearchResultsTabPage(string query, List<SearchResultEntryData> results, Sprite tabIcon, LocalizedString tabTitleFormat)
    {
        PageId = Guid.NewGuid().ToString();
        Query = query;
        Results = results;
        TabIcon = tabIcon;

        TabTitle = new LocalizedString(tabTitleFormat.TableReference, tabTitleFormat.TableEntryReference)
        {
            Arguments = new object[] { query }
        };

    }

}