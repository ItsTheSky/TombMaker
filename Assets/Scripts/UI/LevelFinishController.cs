using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelFinishController : MonoBehaviour
{
    public LogicScript logic;
    
    public int collectedStars;
    public int collectedDots;
    public long collectedExperience;
    
    public List<GameObject> stars;
    public ProgressBar dotsProgressBar;
    
    public Animator coinAnimator;
    public GameObject fullBarObject;
    public GameObject rewardObject;
    
    public TMP_Text deathsText;
    public TMP_Text expText;

    public GameObject nextLevelButton;
    
    public Sprite starCollectedSprite;
    public Sprite starNotCollectedSprite;
    
    public void ResetScore()
    {
        collectedStars = 0;
        collectedDots = 0;
        collectedExperience = 0;
    }

    public void RefreshScore(bool isCustomLevel, int deaths, int collectedExperience)
    {
        // Exp
        expText.text = collectedExperience > 0 ? $"+{collectedExperience} EXP" : "";
        
        // Stars
        for (int i = 0; i < stars.Count; i++)
        {
            stars[i].GetComponent<Image>().sprite = i < collectedStars ? starCollectedSprite : starNotCollectedSprite;
        }
        
        // Dots
        _waitingForCoinAnimation = false;
        coinAnimator.enabled = false;
        dotsProgressBar.ResetProgress();
        dotsProgressBar.SetMaxProgress(1);
        dotsProgressBar.ExecuteOnComplete(ProgressBarFull);
        float result = (float)collectedDots / LogicScript.settings.saveData.CountBlockType(5);
        dotsProgressBar.SetProgress(result);
        
        // Deaths
        deathsText.text = deaths.ToString();
        ResetScore(); // Reset score after refresh
        
        if (!HasNextLevel() || isCustomLevel)
            nextLevelButton.SetActive(false);
    }
    
    public void LeaveLevel()
    {
        logic.LeaveLevel(false);
    }
    
    public void RestartLevel()
    {
        logic.RestartLevel();
        gameObject.SetActive(false);
    }

    public void NextLevel()
    {
        logic.NextLevel();
    }

    public static bool HasNextLevel()
    {
        return LogicScript.settings.levelIndex < LevelsUIManager.levels.Count;
    }

    private bool _waitingForCoinAnimation;
    public void ProgressBarFull()
    {
        if (_waitingForCoinAnimation) 
            return;

        if (PlayerStatsManager.HasRewardCollected(LogicScript.settings.levelIndex)
            || LogicScript.settings.levelIndex == -1) // custom level
            return;
        
        coinAnimator.enabled = true;
        coinAnimator.Play("CoinShining");
        _waitingForCoinAnimation = true;
    }

    private void Update()
    {
        if (_waitingForCoinAnimation)
        {
            if (coinAnimator.GetCurrentAnimatorStateInfo(0).IsName("CoinShining") && coinAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                _waitingForCoinAnimation = false;
                coinAnimator.enabled = false;
                
                if (PlayerStatsManager.HasRewardCollected(LogicScript.settings.levelIndex))
                    return;
                PlayerStatsManager.CollectReward(LogicScript.settings.levelIndex);
                
                // Show reward
                fullBarObject.SetActive(false);
                rewardObject.SetActive(true);
                
                // give reward
                PlayerStatsManager.AddCoins(25);
            }
        }
    }
}