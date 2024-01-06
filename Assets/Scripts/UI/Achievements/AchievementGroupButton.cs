using TMPro;
using UnityEngine;

public class AchievementGroupButton : MonoBehaviour
{
 
    public TMP_Text title;
    public GameObject remainingBadge;
    public TMP_Text remainingBadgeText;
    
    [HideInInspector] public AchievementsController achievementsController;
    [HideInInspector] public Achievements.AchievementCategory achievementCategory;
    
    public void Init(Achievements.AchievementCategory achievementCategory, AchievementsController achievementsController)
    {
        this.title.text = achievementCategory.name;
        
        this.achievementsController = achievementsController;
        this.achievementCategory = achievementCategory;
        
        UpdateRemainingBadge();
    }
    
    public void OnClick()
    {
        achievementsController.OnAchievementGroupBtnClick(this);
    }

    public void Enable()
    {
        title.color /* set it to 0, 255, 255 */ = new Color(0f, 1f, 1f);
    }
    
    public void Disable()
    {
        title.color /* set it to 255, 255, 255 */ = new Color(1f, 1f, 0f);
    }
    
    public void UpdateRemainingBadge()
    {
        int remaining = 0;
        foreach (var achievement in achievementCategory.achievements)
        {
            if (!Achievements.IsCollected(achievement) && Achievements.IsUnlocked(achievement))
                remaining++;
        }
        
        remainingBadge.SetActive(true);
        remainingBadgeText.text = remaining.ToString();
        if (remaining == 0)
            remainingBadge.SetActive(false);
    }
    
}