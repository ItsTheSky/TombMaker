using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelLeaderboardEntryController : MonoBehaviour
{
    
    public TMP_Text username;
    public TMP_Text deaths;
    public TMP_Text stars;
    
    public TMP_Text dots;
    public Slider dotsProgress;

    public void Init(LevelLeaderboardController controller, NewDB.Leaderboard entry)
    {
        this.username.text = entry.username;
        this.deaths.text = entry.deaths.ToString();
        this.stars.text = entry.stars.ToString();
     
        float maxDots = controller.controller.levelData.CountBlockType(5);
        if (maxDots > 0)
        {
            dots.text = entry.dots.ToString() + "/" + maxDots.ToString();
            dotsProgress.value = entry.dots / maxDots;
        }
        else
        {
            dots.gameObject.SetActive(false);
            dotsProgress.gameObject.SetActive(false);
        }
    }

}