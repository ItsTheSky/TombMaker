using System;
using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEntryController : MonoBehaviour
{
    public GameObject levelSettingsPanel;
    
    public GameObject canvas;

    public GameObject editButton;
    public TMP_Text levelName;
    public Toggle selectedToggle;
    
    public SaveData saveData;
    public EditPanelController panelController;
    [CanBeNull] public DataManager.CustomLevelInfo cli;
    
    private bool allowEdition;

    public void Init(EditPanelController panelController, string levelName, bool allowEdition, [CanBeNull] DataManager.CustomLevelInfo cli, SaveData saveData)
    {
        this.levelName = GetComponentInChildren<TMP_Text>();
        
        this.panelController = panelController;
        this.levelName.text = levelName;
        this.cli = cli;
        this.saveData = cli == null ? saveData : cli.saveData;
        this.allowEdition = allowEdition;
        
        editButton.SetActive(allowEdition);
        selectedToggle.gameObject.SetActive(false);
    }

    public void PlayLevel()
    {
        if (cli == null || cli.saveData == null || !cli.saveData.HasSpawnPoint())
        {
            PopupController.Show("Empty Level", "This level is empty! Should we open the editor?",
                EditLevel, () => { });
            return;
        }

        if (!cli.verified && cli.saveData.settings?.levelType == LevelType.Color)
        {
            PopupController.Show("Unverified", "You must <color=purple>verify colors levels</color> before playing them! Open editor?", 
                EditLevel, () => { });
            
            return;
        }
        
        LogicScript.settings = new LogicScript.GameLevelSettings(cli, false);
        MainController.TargetUI = MainController.UIType.CreateEditor;
        TransitionManager.SwitchToLevelScene();
    }

    public void ToggleSelectionBox(bool allowSelection)
    {
        selectedToggle.gameObject.SetActive(allowSelection);
    }

    public void OnTogglePressed()
    {
        this.panelController.OnTogglePressed(this);
    }

    public void EditLevel()
    {
        if (!allowEdition)
            return;
        
        LogicScript.settings = new LogicScript.GameLevelSettings(cli, true);
        MainController.TargetUI = MainController.UIType.CreateEditor;
        TransitionManager.SwitchToLevelScene();
    }

    public void LevelSettings()
    {
        levelSettingsPanel.GetComponent<LevelEntrySettingsController>().Init(cli, this);
        levelSettingsPanel.SetActive(true);
    }
    
    public void CloseLevelSettings()
    {
        levelSettingsPanel.SetActive(false);
    }
    
    public void DeleteLevel()
    {
        PopupController.Show("Confirmation", "Are you sure you want to delete this level?",
            () =>
            {
                CloseLevelSettings();
                DataManager.RemoveLocalLevel(cli.id);
                Destroy(gameObject);
            },
            () =>
            { });
    }

    public void CloneLevel()
    {
        var newName = levelName.text + " (Clone)";
        if (!DataManager.IsLocalLevelNameValid(newName))
        {
            PopupController.Show("Error", "You have cloned the level too many times.",
                () => { }, () => { });
            return;
        }
        
        var newLevel = new DataManager.CustomLevelInfo(cli)
        {
            name = newName,
            publishId = -1,
            published = false,
            completed = false,
            verified = false,
            creationDate = DateTime.Now.Ticks,
            lastModified = DateTime.Now.Ticks,
            id = DataManager.NextLocalLevelId()
        };
        DataManager.AddLocalLevel(newLevel);
        
        var newEntry = Instantiate(gameObject, transform.parent);
        newEntry.GetComponent<LevelEntryController>().Init(newName, allowEdition, newLevel, saveData);
        newEntry.transform.SetSiblingIndex(transform.GetSiblingIndex());
        
        CloseLevelSettings();
    }
}