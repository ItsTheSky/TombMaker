using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelEntrySettingsController : MonoBehaviour
{
    public Button closeButton;
        
    public TMP_InputField levelNameInput;
    public TMP_InputField levelDescriptionInput;
    
    public TMP_Text publishedText;
    public TMP_Text levelId;
    public TMP_Text infosText;
    
    [Header("Level Infos")]
    public TMP_Text lastModifiedText;
    public TMP_Text creationDateText;
    
    [Header("Online Settings")]
    public Toggle allowCloneToggle;
    public Toggle allowCommentsToggle;

    private LevelEntryController _levelEntryController;
    private DataManager.CustomLevelInfo _cli;
    
    public void Init(DataManager.CustomLevelInfo cli, LevelEntryController levelEntryController)
    {
        _levelEntryController = levelEntryController;
        _cli = cli;
        
        levelNameInput.text = cli.name;
        levelDescriptionInput.text = cli.description;
        
        publishedText.text = cli.published ? "Update" : "Publish";
        if (cli.published) 
            levelId.text = "Level published. ID: "+cli.publishId.ToString();
        else 
            levelId.text = "Level not published.";
        
        closeButton.onClick.AddListener(CloseLevelSettings);
        
        levelNameInput.onEndEdit.AddListener(delegate { SaveLevelSettings(); });
        levelDescriptionInput.onEndEdit.AddListener(delegate { SaveLevelSettings(); });
        
        allowCloneToggle.isOn = cli.onlineSettings.allowClone;
        allowCommentsToggle.isOn = cli.onlineSettings.allowComments;
        allowCloneToggle.onValueChanged.AddListener(delegate { OnOnlineSettingsChanged(); });
        allowCommentsToggle.onValueChanged.AddListener(delegate { OnOnlineSettingsChanged(); });
        
        infosText.text = "";
        
        lastModifiedText.transform.parent.gameObject.GetComponentInChildren<HoverWindowed>().Text = "Last modified: " + new DateTime(cli.lastModified).ToString("dd/MM/yyyy HH:mm:ss");
        creationDateText.transform.parent.gameObject.GetComponentInChildren<HoverWindowed>().Text = "Creation date: " + new DateTime(cli.creationDate).ToString("dd/MM/yyyy HH:mm:ss");
        lastModifiedText.text = new DateTime(cli.lastModified).ToString("dd/MM/yyyy");
        creationDateText.text = new DateTime(cli.creationDate).ToString("dd/MM/yyyy");
    }

    public async void OnPublishUpdateButtonClick()
    {
        infosText.text = "";
        if (!_cli.saveData.IsValid())
        {
            infosText.text = "This level is <color=#FF00FF>not valid</color>!";
            return;
        }

        if (!_cli.verified)
        {
            infosText.text = "This level is <color=#FF00FF>not verified</color>!";
            return;
        }
        
        if (_cli.published)
        {
            var stillPublished = await NewDB.IsLevelStillPublished(_cli.publishId);
            
            if (stillPublished)
            {
                var response = await NewDB.UpdateLevel(_cli);
                
                if (response.status == "error")
                {
                    infosText.text = "Error: " + response.message;
                    return;
                }
                
                levelId.text = "Level published. ID: " + _cli.publishId.ToString();
                infosText.text = "Level updated!";
            }
            else
            {
                levelId.text = "Level not published.";
                publishedText.text = "Publish";
                
                _cli.published = false;
                _cli.publishId = -1;
                DataManager.SetLocalLevel(_cli);
            }
        }
        else
        {
            var response = await NewDB.PublishLevel(_cli);
            if (!response.Success())
            {
                infosText.text = "Cannot publish: " + response.message;
                return;
            }
        
            publishedText.text = "Update";
            levelId.text = "Level published. ID: "+_cli.publishId.ToString();
            Achievements.IncrementAchievement(Achievements.ShareLevel);
        }
    }
    
    public void SaveLevelSettings()
    {
        if (_cli == null) 
            return;
        if (_cli.name == levelNameInput.text && _cli.description == levelDescriptionInput.text) 
            return;
        if (string.IsNullOrEmpty(levelNameInput.text) || !DataManager.IsLocalLevelNameValid(levelNameInput.text))
        {
            levelNameInput.text = _cli.name;
            return;
        }
        
        _cli.name = levelNameInput.text;
        _cli.description = levelDescriptionInput.text;
        
        _levelEntryController.levelName.text = _cli.name;

        DataManager.SetLocalLevel(_cli);
    }
    
    public void OnOnlineSettingsChanged()
    {
        _cli.onlineSettings.allowClone = allowCloneToggle.isOn;
        _cli.onlineSettings.allowComments = allowCommentsToggle.isOn;
        
        DataManager.SetLocalLevel(_cli);
    }
    
    public void OnDeleteButtonClick()
    {
        _levelEntryController.DeleteLevel();
    }
    
    public void CloseLevelSettings()
    {
        gameObject.SetActive(false);
    }
    
    public void OnCloneLevel()
    {
        _levelEntryController.CloneLevel();
    }
}