using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelGroupController : MonoBehaviour
{

    [Header("Level Buttons")] 
    public LevelButtonUI firstLevel;
    public LevelButtonUI secondLevel;
    public LevelButtonUI thirdLevel;
    public LevelButtonUI fourthLevel;
    
    [Header("Level Links")]
    public Image firstToSecond;
    public Image secondToThird;
    public Image thirdToFourth;
    public Image fourthToNextGroup;

    public void Init(List<SaveData> levels, bool isLastGroup, int multiplier = 1) 
    {
        var count = levels.Count;
        
        // links
        firstToSecond.gameObject.SetActive(count >= 2);
        secondToThird.gameObject.SetActive(count >= 3);
        thirdToFourth.gameObject.SetActive(count >= 4);
        fourthToNextGroup.gameObject.SetActive(!isLastGroup);
        
        // buttons
        for (int i = 0; i < 4; i++)
        {
            var level = i < count ? levels[i] : null;
            var levelNumber = i + 1;
            
            var levelButton = levelNumber switch
            {
                1 => firstLevel,
                2 => secondLevel,
                3 => thirdLevel,
                4 => fourthLevel
            };
            
            levelButton.gameObject.SetActive(level != null);
            if (level != null) 
                levelButton.Init(level, (multiplier - 1) * 4 + levelNumber);
        }
    }

}