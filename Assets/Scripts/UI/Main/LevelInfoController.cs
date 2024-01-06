using System;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelInfoController : MonoBehaviour
{
    public TMP_Text levelName;
    public TMP_Text levelAuthor;
    public TMP_Text levelDescription;

    public TMP_Text likes;
    public TMP_Text downloads;

    public GameObject notCompletedProgressZone;
    public GameObject completedProgressZone;

    public Slider progress;
    public TMP_Text dots;
    public GameObject star;

    public GameObject leaderboardObject;
    public GameObject commentsObject;

    public TMP_Text id;
    public TMP_Text playText;
    public GameObject playButton;
    public GameObject loadingIcon;
    public GameObject adminButtons;
    public GameObject verifiedCheck;

    public Button cloneLevelButton;
    public Button commentLevelButton;

    public NewDB.Level level;
    public NewDB.LevelStats stats;
    public NewDB.LevelOnlineSettings onlineSettings;
    public SaveData levelData;

    public PublicLevelEntryController publicLevelEntryController;
    
    [Header("Likes")]
    public Button likeButton;
    public Button dislikeButton;
    
    private bool _isLocal;
    private bool _hasLiked = true;

    public async void InitDownload(PublicLevelEntryController entryController, NewDB.Level level, NewDB.LevelStats stats, NewDB.LevelOnlineSettings onlineSettings, bool _isLocal, bool fetchNormalInfos = false)
    {
        RefreshLikesButton();
        publicLevelEntryController = entryController;
        
        this.level = level;
        this.stats = stats;
        this.onlineSettings = onlineSettings;
        this._isLocal = _isLocal;

        var lpd = PlayerStatsManager.GetLevelProgress(level.id);
        
        notCompletedProgressZone.SetActive(!lpd.completed);
        completedProgressZone.SetActive(lpd.completed);

        levelName.text = level.name;
        levelAuthor.text = "by " + level.author;
        levelDescription.text = level.description;
        
        likes.text = stats.likes.ToString() + " likes";
        downloads.text = stats.downloads.ToString() + " downloads";

        cloneLevelButton.interactable = onlineSettings?.allowClone ?? false;
        commentLevelButton.interactable = onlineSettings?.allowComments ?? false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        id.text = "#" + level.id.ToString();
#endif

        try
        {
            if (!(await NewDB.GetPersonalsInfos()).Get<NewDB.User>("user").admin)
                Destroy(adminButtons);
            else
                adminButtons.SetActive(true);
        }
        catch (Exception ignored)
        {
            // ignored
        }

        verifiedCheck.SetActive(level.verified);
        levelData = LocalLevelsManager.GetSaveData(level.id);

        if (levelData != null)
        {
            playButton.SetActive(true);
            loadingIcon.SetActive(false);
        }
        else
        {
            playButton.SetActive(false);
            loadingIcon.SetActive(true);
        
            var response = await NewDB.DownloadLevel(level.id);
            if (response.status != "ok")
            {
                levelDescription.text = "Error downloading level: " + response.message;
                loadingIcon.SetActive(false);
                return;
            }

            var lvlData = response.Get<NewDB.LevelData>("levelData");
            var decompressed = Utilities.NewDecompress(lvlData.compressedData);
            var saveData = JsonConvert.DeserializeObject<SaveData>(decompressed);
            
            LocalLevelsManager.SaveLevel(level, stats, onlineSettings, saveData);
        
            playButton.SetActive(true);
            loadingIcon.SetActive(false);
            
            levelData = saveData;
        }

        if (fetchNormalInfos)
        {
            this.level = LocalLevelsManager.GetLevel(level.id);
            this.stats = LocalLevelsManager.GetStats(level.id);
            this.onlineSettings = LocalLevelsManager.GetOnlineSettings(level.id);
            
            var updatedLevel = await NewDB.GetLevelInfos(level.id);
            if (updatedLevel.status == "ok")
            {
                level = updatedLevel.Get<NewDB.Level>("level");
                stats = updatedLevel.Get<NewDB.LevelStats>("levelStats");
                onlineSettings = updatedLevel.Get<NewDB.LevelOnlineSettings>("onlineSettings");
                
                LocalLevelsManager.SaveLevel(level, stats, onlineSettings, levelData);
                
                levelName.text = level.name;
                levelAuthor.text = "by " + level.author;
                levelDescription.text = level.description;
                
                RefreshStats(stats);
                
                cloneLevelButton.interactable = onlineSettings?.allowClone ?? false;
                commentLevelButton.interactable = onlineSettings?.allowComments ?? false;
                
                verifiedCheck.SetActive(level.verified);
                
                if (id != null) id.text = "#" + level.id.ToString();
            }
        }

        if (levelData.CountBlockType(5) > 0)
        {
            progress.value = lpd.collectedDots / (float)levelData.CountBlockType(5);
            dots.text = lpd.collectedDots.ToString() + "/" + levelData.CountBlockType(5).ToString() + " dots";
            
            progress.gameObject.SetActive(true);
            dots.gameObject.SetActive(true);
        }
        else
        {
            progress.gameObject.SetActive(false);
            dots.gameObject.SetActive(false);
        }

        var spriteEmptyStar = Resources.Load<Sprite>("empty_star");
        var spriteFullStar = Resources.Load<Sprite>("full_empty_star");
        var stars = levelData.CountBlockType(4);
        
        if (stars == 0)
        {
            star.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            star.transform.parent.gameObject.SetActive(true);
            // clear
            foreach (Transform child in star.transform.parent)
            {
                if (child == star.transform) 
                    continue;
                Destroy(child.gameObject);
            }
            
            var starSize = stars > 3 ? 0.8f : 1;
        
            for (int i = 0; i < stars; i++)
            {
                var starObj = i == 0 ? star : Instantiate(star, star.transform.parent);
                
                starObj.GetComponent<Image>().sprite = i < lpd.collectedStars ? spriteFullStar : spriteEmptyStar;
                starObj.transform.localScale = new Vector3(starSize, starSize, 1);
            }
        }
        
        var likedResponse = await NewDB.HasLiked(level.id);
        _hasLiked = likedResponse.status == "ok" && bool.Parse(likedResponse.data["liked"].ToString() ?? "false");
        RefreshLikesButton();
        if (fetchNormalInfos) 
            RefreshNormalInfo();
    }
    
    public void OnCloneButtonClicked()
    {
        var name = level.name + " (Clone)";
        if (!DataManager.IsLocalLevelNameValid(name))
        {
            PopupController.Show("Error", "This level has already been cloned!", () =>
                { }, null, true, false);
            return;
        }

        DataManager.CustomLevelInfo cli = new DataManager.CustomLevelInfo(level, levelData)
        {
            name = name
        };
        DataManager.AddLocalLevel(cli);

        PopupController.Show("Success", "Level cloned successfully!", () =>
            { }, null, true, false);
    }

    public void OnBackButtonPressed()
    {
        gameObject.SetActive(false);
    }

    public void OnPlayButtonPressed()
    {
        if (levelData.version == 0)
        {
            var popupPrefab = Resources.Load<GameObject>("Popup");
            var canvas = FindObjectOfType<Canvas>();
            var popup = Instantiate(popupPrefab, canvas.transform).GetComponent<PopupController>();
            popup.Init("Oh no...", "This level was made using an old editor version, and therefore is not playable anymore.", null, null, true, false);
            return;
        }
        
        LogicScript.settings = new LogicScript.GameLevelSettings(level, stats, onlineSettings, levelData, false, _isLocal);
        MainController.TargetUI = _isLocal ? MainController.UIType.SavedLevelInfo : MainController.UIType.SearchLevelInfo;
        TransitionManager.SwitchToLevelScene();
    }

    public async void OnDeleteAdminButtonPressed()
    {
        await NewDB.DeleteLevel(level.id);
        LocalLevelsManager.DeleteLevel(level.id);
        gameObject.SetActive(false);
    }
    
    public async void OnVerifyAdminButtonPressed()
    {
        await NewDB.VerifyLevel(level.id);
        
        level.verified = true;
        LocalLevelsManager.SaveLevel(level, stats, onlineSettings, levelData);

        verifiedCheck.SetActive(true);
    }

    public void ShowLeaderboard()
    {
        leaderboardObject.GetComponent<LevelLeaderboardController>().LoadLeaderboard(this);
        leaderboardObject.transform.parent.gameObject.SetActive(true);
    }

    public void ShowComments()
    {
        commentsObject.GetComponent<LevelCommentsController>().Init(this);
        commentsObject.transform.parent.gameObject.SetActive(true);
    }
    
    public void HideLeaderboard()
    {
        leaderboardObject.transform.parent.gameObject.SetActive(false);
    }
    
    public void HideComments()
    {
        commentsObject.transform.parent.gameObject.SetActive(false);
    }

    public void OnRefreshButtonPressed()
    {
        LocalLevelsManager.DeleteLevel(level.id);
        InitDownload(publicLevelEntryController, level, stats, onlineSettings, true, true);
    }
    
    // this will refresh level's name, author, description, ...
    public void RefreshNormalInfo()
    {
        level = LocalLevelsManager.GetLevel(level.id);
    }

    public async void LikeLevel(bool like)
    {
        print(_hasLiked);
        if (_hasLiked)
            return;
        
        var response = await NewDB.LikeLevel(level.id, like);
        if (response.status != "ok")
        {
            AlertsManager.ShowAlert("Error: " + response.message, AlertsManager.AlertType.Error);
            return;
        }
        
        _hasLiked = true;
        stats.likes += like ? 1 : -1;
        likes.text = stats.likes.ToString() + " likes";
        RefreshLikesButton();
    }

    public void RefreshStats(NewDB.LevelStats stats)
    {
        this.stats = stats;
        likes.text = stats.likes.ToString() + " likes";
        downloads.text = stats.downloads.ToString() + " downloads";
        
        publicLevelEntryController?.RefreshStats(stats);
    }
    
    private void RefreshLikesButton()
    {
        likeButton.interactable = !_hasLiked;
        dislikeButton.interactable = !_hasLiked;
    }
}