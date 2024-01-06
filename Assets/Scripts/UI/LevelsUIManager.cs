using System.Collections.Generic;
using UnityEngine;

public class LevelsUIManager : MonoBehaviour
{
    public static List<SaveData> levels = new();
    public GameObject levelGroupPrefab;
    public Transform levelGroupsParent;

    void Start()
    {
        levels.Clear();
        
        foreach (var level in Resources.LoadAll<TextAsset>("Levels"))
        {
            var saveData = DataManager.LoadDataFromFile(level);
            levels.Add(saveData);
        }
        
        // now show the map using groups of 4 levels.
        var amountOfGroups = 0;
        var lastGroupList = new List<SaveData>();
        for (int levelCount = 0; levelCount < levels.Count; levelCount++)
        {
            lastGroupList.Add(levels[levelCount]);
            
            if (levelCount % 4 == 3 || levelCount == levels.Count - 1)
            {
                var group = Instantiate(levelGroupPrefab, levelGroupsParent);
                var groupController = group.GetComponent<LevelGroupController>();
                var isLastGroup = levelCount == levels.Count - 1;
                groupController.Init(lastGroupList, isLastGroup, amountOfGroups + 1);
                lastGroupList = new List<SaveData>();
                amountOfGroups++;
            }
        }
    }

    public void RefreshOfficialLevels()
    {
        foreach (Transform child in levelGroupsParent)
            Destroy(child.gameObject);
        
        // now show the map using groups of 4 levels.
        var amountOfGroups = 0;
        var lastGroupList = new List<SaveData>();
        for (int levelCount = 0; levelCount < levels.Count; levelCount++)
        {
            lastGroupList.Add(levels[levelCount]);
            
            if (levelCount % 4 == 3 || levelCount == levels.Count - 1)
            {
                var group = Instantiate(levelGroupPrefab, levelGroupsParent);
                var groupController = group.GetComponent<LevelGroupController>();
                var isLastGroup = levelCount == levels.Count - 1;
                groupController.Init(lastGroupList, isLastGroup, amountOfGroups + 1);
                lastGroupList = new List<SaveData>();
                amountOfGroups++;
            }
        }
    }
}
