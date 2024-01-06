using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelLeaderboardController : MonoBehaviour
{
    
    private readonly Dictionary<int, List<NewDB.Leaderboard>> LeaderboardCache = new();

    public LevelInfoController controller;
    
    public GameObject parent;
    public GameObject leaderboardEntryPrefab;
    
    public TMP_Text loadingText;
    
    [Header("Pagination")] 
    public GameObject previousPageBtn;
    public GameObject nextPageBtn;
    public TMP_Text pageInfos;

    private int _currentpage = 1;

    public async void LoadLeaderboard(LevelInfoController controller, int page = 1)
    {
        this.controller = controller;
        _currentpage = page;

        var id = controller.level.id;
        if (LeaderboardCache.ContainsKey(id))
            return;
        
        nextPageBtn.gameObject.SetActive(false);
        previousPageBtn.gameObject.SetActive(false);
        pageInfos.text = "Loading ...";
        
        loadingText.text = "Loading leaderboard...";
        loadingText.gameObject.SetActive(true);
        foreach (Transform child in parent.transform)
            Destroy(child.gameObject);
        
        var response = await NewDB.GetLeaderboard(id, page);
        if (response.status != "ok")
        {
            loadingText.text = "An error occured: " + response.message;
            return;
        }
        var entries = response.GetList<NewDB.Leaderboard>("leaderboard");
        var pages = response.Get<NewDB.Pagination>("pagination");
        LeaderboardCache[id] = entries;
        
        loadingText.gameObject.SetActive(false);
        if (entries == null || entries.Count == 0)
        {
            loadingText.text = "No entries found.";
            loadingText.gameObject.SetActive(true);
            
            nextPageBtn.gameObject.SetActive(false);
            previousPageBtn.gameObject.SetActive(false);
            pageInfos.text = "";
        }
        else
        {
            foreach (var entry in entries)
            {
                var obj = Instantiate(leaderboardEntryPrefab, parent.transform);
                var entryController = obj.GetComponent<LevelLeaderboardEntryController>();
                entryController.Init(this, entry);
            }

            nextPageBtn.gameObject.SetActive(pages.hasNext);
            previousPageBtn.gameObject.SetActive(pages.hasPrevious);
            pageInfos.text = $"Showing {pages.size} (page {pages.page}/{pages.pages})";
        }
    }

    public void RefreshLeaderboard()
    {
        LeaderboardCache.Remove(controller.level.id);
        LoadLeaderboard(controller);
    }

}