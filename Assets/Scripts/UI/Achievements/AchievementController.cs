using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementController : MonoBehaviour
{
    
    public GameObject unlocked;

    public Image icon;
    public TMP_Text title;
    public TMP_Text description;

    public TMP_Text progress;
    public Slider progressBar;
    
    public TMP_Text rewardCoins;
    public Image rewardSkin;
    public Image rewardTrail;
    public GameObject rewardOverlay;

    public Animator checkAnimator;

    [HideInInspector] public Achievements.Achievement achievement;
    [HideInInspector] public Achievements.AchievementCategory achievementCategory;
    [HideInInspector] public AchievementsController achievementsController;

    public void Init(Achievements.Achievement achievement, AchievementsController achievementsController, Achievements.AchievementCategory achievementCategory)
    {
        var isUnlocked = Achievements.IsUnlocked(achievement);
        var isCollected = Achievements.IsCollected(achievement);
        
        // meta
        this.achievement = achievement;
        this.achievementsController = achievementsController;
        this.achievementCategory = achievementCategory;
        
        this.icon.sprite = Resources.Load<Sprite>("Icons/Success/" + achievement.icon);
        this.title.text = achievement.name;
        this.description.text = achievement.description;
        
        // progress
        if (this.achievement.type == Achievements.AchievementType.Incremental)
        {
            var current = Achievements.GetProgress(achievement);
            var target = achievement.target;
            var percent = Mathf.Clamp(Mathf.RoundToInt(((float) current / target) * 100f), 0, 100);
            this.progress.text = percent + "%";
            
            this.progressBar.minValue = 0;
            this.progressBar.maxValue = target;
            this.progressBar.value = current;
        } 
        else 
        {
            if (isUnlocked)
            {
                this.progress.text = "Completed";
                this.progressBar.minValue = 0;
                this.progressBar.maxValue = 1;
                this.progressBar.value = 1;
            }
            else
            {
                this.progress.text = "0%";
                this.progressBar.minValue = 0;
                this.progressBar.maxValue = 1;
                this.progressBar.value = 0;
            }
        }
        
        // rewards
        if (achievement.coins > 0) 
            this.rewardCoins.text = "+" + achievement.coins;

        if (achievement.skin > 0)
        {
            var skinMeta = Constants.GetSkinMeta(achievement.skin);
            var sprites = Resources.LoadAll<Sprite>("Player/" + skinMeta.name + "/idle");
            rewardSkin.sprite = sprites[5];
        }
        else
        {
            rewardSkin.gameObject.SetActive(false);
        }
        
        if (achievement.trail > 0)
        {
            var trailMeta = Constants.Trails[achievement.trail];
            rewardTrail.sprite = trailMeta.GetIcon();
        }
        else
        {
            rewardTrail.gameObject.SetActive(false);
        }
        
        unlocked.SetActive(isUnlocked && isCollected);
        rewardOverlay.SetActive(isUnlocked && !isCollected);
    }
    
    public void Collect()
    {
        Achievements.CollectRewards(achievement);
        achievementsController.mainController.RefreshLevelInfos();
        AudioManager.PlaySound("Buy");
        
        this.rewardOverlay.SetActive(false);
        this.unlocked.SetActive(true);
        checkAnimator.Play("CheckGrowing");
        
        achievementsController.achievementGroupButtons[achievementCategory].UpdateRemainingBadge();
        achievementsController.RefreshAchievementsToCollectBadge();
    }
    
}