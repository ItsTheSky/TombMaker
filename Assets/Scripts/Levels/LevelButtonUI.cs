using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    
    [FormerlySerializedAs("globalLevelInfo")] public PlayerStatsManager.LevelProgressData progressData;
    public SaveData saveData;

    public int levelNumber;
    
    public GameObject stars;
    public GameObject unlockedDot;
    public Button button;

    public Sprite lockedBackground;

    public GameObject lockIcon;
    public GameObject text;

    public void LoadLevel()
    {
        if (IsLocked())
            return;

        LogicScript.settings = new LogicScript.GameLevelSettings(saveData, false, levelNumber);
        MainController.TargetUI = MainController.UIType.Levels;
        TransitionManager.SwitchToLevelScene();
    }

    public void LoadInfo()
    {
        var currentLevel = GlobalIOManager.CurrentLevel;
        text.GetComponent<TMP_Text>().text = levelNumber.ToString();
        if (levelNumber <= currentLevel) // completed
        {
            stars.SetActive(true);
            
            int starsCount = progressData.collectedStars;
            for (int i = 0; i < 3; i++)
            {
                bool isFilled = starsCount > 0;
                stars.transform.GetChild(i).gameObject.GetComponent<Image>().color = isFilled ? 
                    new Color(1, 1, 0, 1) : new Color(1, 0, 1, 1f);
                starsCount--;
            }

            unlockedDot.SetActive(progressData.collectedDots != saveData.CountBlockType(5)); // todo: add the dots check
            lockIcon.SetActive(false);
        }
        else if (levelNumber == currentLevel + 1) // unlocked, it's the current level
        {
            stars.SetActive(true); // they appears as empty as default
            unlockedDot.SetActive(true);
            lockIcon.SetActive(false);
        }
        else if (IsLocked()) // locked
        {
            button.interactable = false;
            stars.SetActive(false);
            unlockedDot.SetActive(false);
            
            GetComponent<Image>().sprite = lockedBackground;
            
            lockIcon.SetActive(true);
            text.SetActive(false);
        }
    }
    
    public bool IsLocked()
    {
        return levelNumber > GlobalIOManager.CurrentLevel + 1;
    }

    public void Init(SaveData level, int levelNumber)
    {
        saveData = level;
        progressData = PlayerStatsManager.GetLevelProgress(levelNumber, true);
        this.levelNumber = levelNumber;
        
        LoadInfo();
    }
}