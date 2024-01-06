using TMPro;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.UI;

public class PublicLevelEntryController : MonoBehaviour
{
    public BrowseLevelsControlle controller;

    public TMP_Text levelName;
    public TMP_Text levelAuthor;
    public TMP_Text likes;
    public TMP_Text downloads;
    public TMP_Text infoText;
    public GameObject verifiedCheck;
    
    [HideInInspector] public NewDB.Level level;
    [HideInInspector] public NewDB.LevelStats stats; 
    [HideInInspector] public NewDB.LevelOnlineSettings onlineSettings;
    
    public GameObject statsWindow;
    public GameObject deleteButton;
    
    private bool isLocal;
    
    public void Init(TMP_Text infoText, BrowseLevelsControlle controller, NewDB.Level level, NewDB.LevelStats stats, NewDB.LevelOnlineSettings onlineSettings, bool isLocal)
    {
        this.controller = controller;
        this.isLocal = isLocal;
        
        this.levelName.text = level.name;
        this.levelAuthor.text = "by " + level.author;
        this.likes.text = stats.likes.ToString() + " likes";
        this.downloads.text = stats.downloads.ToString() + " downloads";
        
        verifiedCheck.SetActive(level.verified);
        
        this.level = level;
        this.stats = stats;
        this.onlineSettings = onlineSettings;
        
        statsWindow.SetActive(!isLocal);
        deleteButton.SetActive(isLocal);
    }

    public void OnPlayButtonClicked()
    {
        controller.levelInfoController.InitDownload(this, level, stats, onlineSettings, isLocal);
        controller.levelInfoController.gameObject.SetActive(true);
    }

    public void RefreshStats(NewDB.LevelStats stats)
    {
        this.stats = stats;
        this.likes.text = stats.likes.ToString() + " likes";
        this.downloads.text = stats.downloads.ToString() + " downloads";
    }
    
    /* public async void OnLikeButtonClicked(bool like)
    {
        string reponse = await DatabaseManager.LikeLevel(pli.id, like);
        if (reponse != "success") {
            infoText.text = "Error: " + reponse;
            return;
        }
        
        if (like)
            pli.likes++;
        else
            pli.likes--;
        
        likes.text = pli.likes.ToString() + " likes";
    } */
    
    public void OnDeleteButtonClicked() {
        Destroy(gameObject);
    }
}