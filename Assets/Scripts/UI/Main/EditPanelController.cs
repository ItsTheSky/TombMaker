using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPanelController : MonoBehaviour
{
    public GameObject levelSettingsPanel;
    
    public GameObject levelsPanel;
    public GameObject levelEntryPrefab;

    public TMP_InputField levelNameInput;
    public Toggle toggleSelectionMode;

    private bool _initialized;
    private Dictionary<int, LevelEntryController> _levelEntries = new();

    public void InitLevels()
    {
        if (_initialized)
            return;
        _initialized = true;
        
        var levels = DataManager.GetLocalLevels();
        levels.Sort((a, b) => a.lastModified.CompareTo(b.lastModified));
        levels.Reverse();

        foreach (var level in levels)
        {
            var entry = Instantiate(levelEntryPrefab, levelsPanel.transform);
            var entryController = entry.GetComponent<LevelEntryController>();

            entryController.Init(this, level.name, true, level, null);
            entryController.levelSettingsPanel = levelSettingsPanel;
        }
    }

    public void CreateLevel()
    {
        var name = levelNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.Log("Level name is empty");
            return;
        }

        if (!DataManager.IsLocalLevelNameValid(name))
        {
            Debug.Log("Level name is invalid");
            return;
        }

        var level = new DataManager.CustomLevelInfo()
        {
            name = name,
            completed = false, published = false, publishId = -1,
            saveData = new SaveData(),
            creationDate = DateTime.Now.Ticks,
            lastModified = DateTime.Now.Ticks,
            verified = false,
            coloredBlocksCount = -1,
            id = DataManager.NextLocalLevelId()
        };
        DataManager.AddLocalLevel(level);
        
        var entry = Instantiate(levelEntryPrefab, levelsPanel.transform);
        entry.transform.SetSiblingIndex(1); // 0 is the create button
        var entryController = entry.GetComponent<LevelEntryController>();
        
        entryController.Init(this, level.name, true, level, null);
        entryController.levelSettingsPanel = levelSettingsPanel;
        
        levelNameInput.text = "";
        
        Achievements.IncrementAchievement(Achievements.CreateLevel);
    }
}