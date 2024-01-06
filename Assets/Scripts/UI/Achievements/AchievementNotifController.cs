using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AchievementNotifController : MonoBehaviour
{
    
    public Animator notifAnimator;
    public TMP_Text achievementTitle;
    public TMP_Text achievementProgressText;

    private bool _shown;
    private Coroutine _hideCoroutine;
    
    private void Start()
    {
        Achievements.OnAchievementUnlocked += ShowAchievementNotif;
    }

    public void ShowAchievementNotif(Achievements.Achievement achievement)
    {
        print("Called");
        if (notifAnimator.IsDestroyed())
            return;
        achievementTitle.text = achievement.name;

        achievementProgressText.text = achievement.type == Achievements.AchievementType.Instant 
            ? "Completed!" : $"{achievement.target}/{achievement.target}";
        AudioManager.PlaySound("Achievement");

        if (_shown)
        { 
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = StartCoroutine(HideAchievementNotif());
        }
        else
        {
            _shown = true;
            notifAnimator.Play("AchievementNotifShow");
            _hideCoroutine = StartCoroutine(HideAchievementNotif());
        }
    }
    
    private System.Collections.IEnumerator HideAchievementNotif()
    {
        yield return new WaitForSeconds(5f);
        notifAnimator.Play("AchievementNotifHide");
        _shown = false;
    }
    
    public static void ShowAchievementNotifStatic(Achievements.Achievement achievement)
    {
        var notif = FindObjectOfType<AchievementNotifController>();
        if (notif == null)
            return;
        notif.ShowAchievementNotif(achievement);
    }
    
}