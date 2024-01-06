using System;
using UnityEngine;
using UnityEngine.UI;

public class SearchStarsController : MonoBehaviour
{
    
    public Image[] stars;
    public int starsCount;
    
    public Sprite starOn;
    public Sprite starOff;

    private void Start()
    {
        foreach (var star in stars)
        {
            star.gameObject.SetActive(true);
            star.sprite = starOff;
            star.gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                starsCount = Array.IndexOf(stars, star) + 1;
                for (int i = 0; i < starsCount; i++)
                {
                    stars[i].sprite = starOn;
                }
                for (int i = starsCount; i < stars.Length; i++)
                {
                    stars[i].sprite = starOff;
                }
            });
        }
    }
    
    public void ResetStars()
    {
        starsCount = 0;
        foreach (var star in stars)
        {
            star.sprite = starOff;
        }
    }
}