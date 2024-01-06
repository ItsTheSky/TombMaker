using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementsController : MonoBehaviour
{
    public MainController mainController;
 
    public GameObject achievementPrefab;
    public GameObject achievementGroupBtnPrefab;
    
    public Transform achievementsContainer;
    public Transform achievementGroupsContainer;

    public GameObject achievementsToCollectBadge;
    public TMP_Text achievementsToCollectBadgeText;
    
    [HideInInspector] public Dictionary<Achievements.AchievementCategory, AchievementGroupButton> achievementGroupButtons = new();

    private void Start()
    {
        AchievementGroupButton first = null;
        foreach (var achievementCategory in Achievements.AchievementCategories)
        {
            var achievementGroupBtn = Instantiate(achievementGroupBtnPrefab, achievementGroupsContainer);
            var achievementGroupButton = achievementGroupBtn.GetComponent<AchievementGroupButton>();
            achievementGroupButtons.Add(achievementCategory, achievementGroupButton);
            achievementGroupButton.Init(achievementCategory, this);
            
            if (first == null)
                first = achievementGroupButton;
        }

        if (first != null)
            OnAchievementGroupBtnClick(first);
        
        RefreshAchievementsToCollectBadge();
    }
    
    public void OnAchievementGroupBtnClick(AchievementGroupButton achievementGroupButton)
    {
        foreach (var btn in achievementGroupButtons)
            btn.Value.Disable();
        
        achievementGroupButton.Enable();
        
        foreach (Transform child in achievementsContainer)
            Destroy(child.gameObject);
        
        foreach (var achievement in achievementGroupButton.achievementCategory.achievements)
        {
            var achievementObj = Instantiate(achievementPrefab, achievementsContainer);
            var achievementScript = achievementObj.GetComponent<AchievementController>();
            achievementScript.Init(achievement, this, achievementGroupButton.achievementCategory);
        }
    }
    
    public void RefreshAchievementsToCollectBadge()
    {
        var count = 0;
        foreach (var category in Achievements.AchievementCategories)
        {
            foreach (var achievement in category.achievements)
            {
                if (Achievements.IsUnlocked(achievement) && !Achievements.IsCollected(achievement))
                    count++;
            }
        }

        achievementsToCollectBadge.SetActive(count > 0);
        achievementsToCollectBadgeText.text = count.ToString();
    }
}