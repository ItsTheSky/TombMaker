using System;
using TMPro;
using UnityEngine;

public class BrowseLevelsControlle : MonoBehaviour
{
    private enum State
    {
        Browse,
        Search,
        Saved
    }
    private State _state = State.Browse;
    
    public LevelInfoController levelInfoController;
        
    public GameObject levelEntryPrefab;
    public GameObject content;

    public TMP_Text loadingText;
    public TMP_Text titleText;
    public TMP_Text infoText;

    public GameObject deleteAllButton;
    
    public GameObject createPanel;
    public GameObject searchPanel;
    
    public TMP_Text pageText;
    public GameObject previousPageButton;
    public GameObject nextPageButton;
    
    private int _currentPage = 1;

    public void LoadLevels()
    {
        _state = State.Saved;
        
        titleText.text = "Saved";
        loadingText.gameObject.SetActive(false);
        
        _currentPage = 1;
        LoadLevelsAtPage(_currentPage);
        
        deleteAllButton.SetActive(true);
    }

    public readonly int levelsPerPage = 10;
    public void LoadLevelsAtPage(int page)
    {
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);

        var levels = LocalLevelsManager.GetLevels();
        var pagination = new NewDB.Pagination()
        {
            page = page,
            pages = (int) Math.Ceiling((double) levels.Count / levelsPerPage),
            size = Math.Min(levelsPerPage, levels.Count - (page - 1) * levelsPerPage)
        };
        _currentPage = pagination.page;
        
        ShowPagination(pagination.page, pagination.pages, pagination.size);
        
        foreach (var levelId in levels.GetRange((page - 1) * levelsPerPage, Math.Min(levelsPerPage, levels.Count - (page - 1) * levelsPerPage)))
        {
            var level = LocalLevelsManager.GetLevel(levelId);
            var stats = LocalLevelsManager.GetStats(levelId);
            var settings = LocalLevelsManager.GetOnlineSettings(levelId);
            
            if (level == null || stats == null)
                continue;
            
            var levelEntry = Instantiate(levelEntryPrefab, content.transform);
            levelEntry.GetComponent<PublicLevelEntryController>().Init(infoText, this, level, stats, settings, true);
        }
    }

    public void SearchLevels(SearchController.SearchData searchData)
    {
        _state = State.Search;
        
        titleText.text = "Search";
        
        loadingText.gameObject.SetActive(true);
        loadingText.text = "Searching levels...";
        
        deleteAllButton.SetActive(false);
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);
        
        SearchLevelsAsync(searchData, 1);
    }

    private async void SearchLevelsAsync(SearchController.SearchData searchData, int page = 1)
    {
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);
        
        MainController.LastSearchData = searchData;
        
        var response = await NewDB.SearchLevels(searchData, page);
        if (response.status != "ok")
            loadingText.text = "Error: " + response.message;
        else
        {
            loadingText.gameObject.SetActive(false);
            
            var levels = response.GetList<NewDB.LevelAndStats>("levels");
            var pagination = response.Get<NewDB.Pagination>("pagination");
            _currentPage = pagination.page;
            
            ShowPagination(pagination.page, pagination.pages, pagination.size);

            foreach (var levelInfo in levels)
            {
                var levelEntry = Instantiate(levelEntryPrefab, content.transform);
                levelEntry.GetComponent<PublicLevelEntryController>().Init(infoText, this, levelInfo.level, levelInfo.stats, levelInfo.onlineSettings, false);

            }
        }
    }
    
    public void OnDeleteAllButtonClicked()
    {
        LocalLevelsManager.DeleteAllLevels();
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);
    }
    
    public void OnBackButtonClicked()
    {
        switch (_state)
        {
            case State.Browse:
                throw new Exception("Back button should not be active in Browse state");
                break;
            case State.Search:
                searchPanel.SetActive(true);
                gameObject.SetActive(false);
                break;
            case State.Saved:
                createPanel.SetActive(true);
                gameObject.SetActive(false);
                break;
        }
    }
    
    public async void NextPage()
    {
        switch (_state)
        {
            case State.Browse:
                throw new Exception("Next page button should not be active in Browse state");
                break;
            case State.Search:
                SearchLevelsAsync(MainController.LastSearchData, _currentPage + 1);
                break;
            case State.Saved:
                throw new Exception("Next page button should not be active in Saved state");
                break;
        }
    }
    
    public async void PreviousPage()
    {
        switch (_state)
        {
            case State.Browse:
                throw new Exception("Previous page button should not be active in Browse state");
                break;
            case State.Search:
                SearchLevelsAsync(MainController.LastSearchData, _currentPage - 1);
                break;
            case State.Saved:
                throw new Exception("Previous page button should not be active in Saved state");
                break;
        }
    }
    
    public void ShowPagination(int page, int pages, int size)
    {
        nextPageButton.SetActive(page < pages);
        previousPageButton.SetActive(page > 1);
        pageText.text = "Showing " + size + " (page " + page + " / " + pages + ")";
    }
}