using TMPro;
using UnityEngine;
using System;

public class PopupController : MonoBehaviour
{
    public static GameObject PopupPrefab;

    public GameObject yesButton;
    public GameObject noButton;
    public TMP_Text title;
    public TMP_Text message;
    
    public Action yesAction;
    public Action noAction;

    public void Init(string title, string message,
        Action yesAction, Action noAction, bool showYesButton = true, bool showNoButton = true)
    {
        this.title.text = title;
        this.message.text = message;
        this.yesAction = yesAction;
        this.noAction = noAction;
        
        yesButton.SetActive(showYesButton);
        noButton.SetActive(showNoButton);
    }
    
    public void Yes()
    {
        yesAction?.Invoke();
        Destroy(gameObject);
    }
    
    public void No()
    {
        noAction?.Invoke();
        Destroy(gameObject);
    }

    // 
    
    public static void Show(string title, string message,
        Action yesAction, Action noAction, bool showYesButton = true, bool showNoButton = true)
    {
        CheckInitialization();
        
        var popup = Instantiate(PopupPrefab, GameObject.Find("Canvas").transform);
        popup.GetComponent<PopupController>().Init(title, message, yesAction, noAction, showYesButton, showNoButton);
    }
    
    private static void CheckInitialization()
    {
        if (PopupPrefab == null)
            PopupPrefab = Resources.Load<GameObject>("Popup");
    }
}