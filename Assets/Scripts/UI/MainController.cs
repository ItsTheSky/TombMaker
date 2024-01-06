using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public enum UIType
    {
        Main,
        Settings,
        Levels,
        Create,
        CreateEditor,
        SearchLevelInfo,
        SavedLevelInfo,
    }
    
    public static UIType TargetUI = UIType.Main;
    public static SearchController.SearchData LastSearchData;
    
    public TMP_Text versionText;
    
    [Header("UI Panels")]
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject levelsMenu;
    public GameObject createMenu;
    public GameObject editMenu;
    public GameObject levelInfo;
    public GameObject levelList;
    public GameObject shopMenu;
    public GameObject creditsMenu;
    public GameObject searchMenu;
    public GameObject achievementsMenu;
    public GameObject usersMenu;
    
    public List<Transform> UIPanels = new();

    [Header("Top Infos")] 
    public TMP_Text coinsText;
    public TMP_Text levelText;
    public TMP_Text shieldsText;
    public TMP_Text levelDotsText;
    public Slider levelProgressSlider;
    
    [Header("Locked Levels Features")]
    public Button shopButton;
    public GameObject shopLock;
    public Button createButton;
    public GameObject createLock;
    public GameObject accountButton; // hidden above level 5
    
    [Header("Other Controllers")]
    public LevelsUIManager levelsUIManager;
    public AchievementsController achievementsController;
    public HelpTextsManager helpTextsManager;

    // Start is called before the first frame update
    void Start()
    {
        helpTextsManager.Init();
        versionText.text = "v" + Application.version;
        
        ClientSettingsManager.LoadAndInitSettings();
        UpdateLocalScale();
        
        print(Utilities.NewCompress("you found me :D"));
        
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        levelsMenu.SetActive(false);
        createMenu.SetActive(false);
        editMenu.SetActive(false);
        levelInfo.SetActive(false);
        levelList.SetActive(false);
        shopMenu.SetActive(false);
        creditsMenu.SetActive(false);
        searchMenu.SetActive(false);
        usersMenu.SetActive(false);
        
        RefreshLevelInfos();

        switch (TargetUI)
        {
            case UIType.Main:
                mainMenu.SetActive(true);
                break;
            case UIType.Settings:
                settingsMenu.SetActive(true);
                break;
            case UIType.Levels:
                levelsMenu.SetActive(true);
                break;
            case UIType.Create:
                createMenu.SetActive(true);
                break;
            case UIType.CreateEditor:
                editMenu.GetComponent<EditPanelController>().InitLevels();
                editMenu.SetActive(true);
                createMenu.SetActive(true);
                break;
            case UIType.SearchLevelInfo:
            case UIType.SavedLevelInfo:
                createMenu.SetActive(true);
                var levelInfoController = levelInfo.GetComponent<LevelInfoController>();
                levelInfoController.InitDownload(null, LogicScript.settings.level, LogicScript.settings.levelStats, LogicScript.settings.levelOnlineSettings,
                    TargetUI == UIType.SavedLevelInfo);
                levelInfo.SetActive(true);

                var comp = levelList.GetComponent<BrowseLevelsControlle>();
                if (LogicScript.settings.wasFromSaved || LastSearchData == null)
                    comp.LoadLevels();
                else
                    comp.SearchLevels(LastSearchData);
                levelList.SetActive(true);
                LastSearchData = null;
                
                break;
        }

        AudioManager.StopSound("LevelTheme");
        AudioManager.PlaySound("MenuTheme");
        
        achievementsController.RefreshAchievementsToCollectBadge();
    }
    
    public void RefreshLevelInfos()
    {
        var level = PlayerStatsManager.GetLevel();
        
        coinsText.text = PlayerStatsManager.GetCoins().ToString();
        shieldsText.text = PlayerStatsManager.GetShields().ToString();
        levelText.text = "level\n" + level.ToString();
        levelDotsText.text = PlayerStatsManager.GetCurrentDots().ToString() + "/" + PlayerStatsManager.GetRequiredDots().ToString();
        levelProgressSlider.maxValue = PlayerStatsManager.GetRequiredDots();
        levelProgressSlider.value = PlayerStatsManager.GetCurrentDots();

        var hasUnlockedShop = level >= 2;
        var hasUnlockedCreate = level >= 3;
        
        shopButton.interactable = hasUnlockedShop;
        shopLock.SetActive(!hasUnlockedShop);
        
        createButton.interactable = hasUnlockedCreate;
        createLock.SetActive(!hasUnlockedCreate);
        
        accountButton.SetActive(!hasUnlockedCreate);
    }

    public void RefreshOfficialLevels()
    {
        levelsUIManager.RefreshOfficialLevels();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }
    }

    public void UpdateLocalScale()
    {
        foreach (var panel in UIPanels)
        {
            EditorValues.UpdateScale(panel, false);
        }
        
        EditorValues.UpdateScale(Resources.Load<GameObject>("Popup").transform, false);
    }
    
    // Actions
    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
        settingsMenu.transform.SetAsLastSibling();
    }
    
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
    }
    
    public void OpenAchievements()
    {
        achievementsMenu.SetActive(true);
        achievementsMenu.transform.SetAsLastSibling();
    }
    
    public void CloseAchievements()
    {
        achievementsMenu.SetActive(false);
    }
    
    public void OpenLevels()
    {
        levelsMenu.SetActive(true);
    }
    
    public void CloseLevels()
    {
        levelsMenu.SetActive(false);
    }

    public void OpenEditor()
    {
        createMenu.SetActive(true);
    }
    
    public void CloseEditor()
    {
        createMenu.SetActive(false);
    }
    
    public void OpenShop()
    {
        shopMenu.SetActive(true);
        shopMenu.GetComponent<ShopController>().Init(this);
    }
    
    public void CloseShop()
    {
        shopMenu.SetActive(false);
    }
    
    public void OpenCredits()
    {
        creditsMenu.SetActive(true);
    }
    
    public void CloseCredits()
    {
        creditsMenu.SetActive(false);
    }
    
    public void OpenSearch()
    {
        searchMenu.SetActive(true);
    }
    
    public void CloseSearch()
    {
        searchMenu.SetActive(false);
    }

    public void OpenDocumentation()
    {
        Application.OpenURL("https://itsthesky.net/tombmaker/docs/");
    }
    
    public void OpenUsers()
    {
        usersMenu.SetActive(true);
    }
    
    public void CloseUsers()
    {
        usersMenu.SetActive(false);
    }
    
}
