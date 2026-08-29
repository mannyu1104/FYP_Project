using System.Collections.Generic;
using UnityEngine;

public class SearchManager : MonoBehaviour
{
    public static SearchManager Instance { get; private set; }

    [SerializeField]
    [Tooltip("The single SearchDatabase asset containing every searchable entry in the game.")]
    private SearchData searchData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Search against the database and returns the matching entries.
    public List<SearchResultEntryData> PerformSearch(string query)
    {
        if (searchData == null)
        {
            Debug.LogWarning("[SearchManager] No SearchData assigned. Did you forget to link it in the Inspector?");
            return new List<SearchResultEntryData>();
        }

        return searchData.Search(query);
    }
}