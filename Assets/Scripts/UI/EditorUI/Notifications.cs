using System;
using TMPro;
using UnityEngine;

public class Notifications : MonoBehaviour
{
 
    public TMP_Text notificationText;
    
    public void ShowNotification(string text)
    {
        notificationText.text = text;
        notificationText.color = new Color(1, 1, 0, 1);
        gameObject.SetActive(true);
        _shouldFade = false;
        
        Invoke(nameof(HideNotification), 3f);
    }
    
    public void ShowError(string text)
    {
        notificationText.text = text;
        notificationText.color = new Color(1, 0, 0, 1);
        gameObject.SetActive(true);
        _shouldFade = false;
        
        Invoke(nameof(HideNotification), 3f);
    }
    
    private bool _shouldFade;
    public void HideNotification()
    {
        _shouldFade = true;
    }
    
    // fade effect using update
    private void Update()
    {
        if (gameObject.activeSelf && _shouldFade && notificationText.color.a > 0)
        {
            var color = notificationText.color;
            color.a -= Time.deltaTime;
            notificationText.color = color;
            
            if (color.a <= 0)
            {
                _shouldFade = false;
                gameObject.SetActive(false);
            }
        }
    }
}