using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertController : MonoBehaviour
{
    
    public Image icon;
    public TMP_Text messageText;
    
    public void Init(string message, AlertsManager.AlertType type = AlertsManager.AlertType.Info)
    {
        messageText.text = message;
        
        switch (type)
        {
            case AlertsManager.AlertType.Info:
                icon.color = new Color(1, 1, 0);
                break;
            case AlertsManager.AlertType.Warning:
                icon.color = new Color(0, 1, 1);
                break;
            case AlertsManager.AlertType.Error:
                icon.color = new Color(1, 0, 1);
                break;
        }
        
        StartCoroutine(Dismiss());
    }
    
    private IEnumerator<WaitForSeconds> Dismiss()
    {
        yield return new WaitForSeconds(3.5f);
        
        Destroy(gameObject);
    }
    
}