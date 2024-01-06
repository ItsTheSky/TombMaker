using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountController : MonoBehaviour
{
    public MainController mainController;
    
    public GameObject loginRegisterPanel;
    public GameObject accountPanel;

    // for login
    public GameObject loginPanel;
    public GameObject registerPanel;
    
    public Button loginButton;
    public Button registerButton;

    public Sprite selectedButtonSprite;
    public Sprite unselectedButtonSprite;
    
    // for account
    public TMP_Text userId;
    public TMP_Text publishedLevels;
    public TMP_Text creationDate;
    public TMP_Text collectedStars;
    public TMP_Text completedLevels;
    public TMP_Text username;
    public GameObject adminBadgeObject;
    public TMP_Text errorText;

    public void RefreshPanel()
    {
        if (NewDB.IsLoggedIn())
        {
            loginRegisterPanel.SetActive(false);
            loginPanel.SetActive(false);
            registerPanel.SetActive(false);
            
            accountPanel.SetActive(true);
            RefreshAccountInfos();
        }
        else
        {
            loginRegisterPanel.SetActive(true);
            accountPanel.SetActive(false);
            errorText.gameObject.SetActive(false);
            
            OnLoginButtonClicked();
        }
    }
    
    public void OnLoginButtonClicked()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        
        loginButton.GetComponent<Image>().sprite = selectedButtonSprite;
        registerButton.GetComponent<Image>().sprite = unselectedButtonSprite;
    }
    
    public void OnRegisterButtonClicked()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        
        loginButton.GetComponent<Image>().sprite = unselectedButtonSprite;
        registerButton.GetComponent<Image>().sprite = selectedButtonSprite;
    }

    public async void RefreshAccountInfos()
    {
        accountPanel.SetActive(false);
        errorText.gameObject.SetActive(true);
        errorText.text = "Fetching account infos...";

        var response = await NewDB.GetPersonalsInfos();
        if (response.status != "ok")
        {
            accountPanel.SetActive(false);
            errorText.gameObject.SetActive(true);
            errorText.text = "Error while loading account infos: " + response.message;
            if (response.status == "invalid_token")
                NewDB.Logout();
            
            return;
        }
        
        errorText.gameObject.SetActive(false);
        accountPanel.SetActive(true);
        var user = response.Get<NewDB.User>("user");
        var userInfos = response.Get<NewDB.ExtendedUserInfos>("infos");

        userId.text = "User ID: " + user.id;
        publishedLevels.text = "Published levels: " + userInfos.publishedLevels;
        creationDate.text = "Creation date: " + new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(user.creationDate).ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
        //collectedStars.text = "Collected stars: " + user.collectedStars;
        completedLevels.text = "Completed levels: " + userInfos.completedLevels;
        username.text = user.username;
        
        adminBadgeObject.SetActive(user.admin);
    }
    
    public void OnLogoutButtonClicked()
    {
        NewDB.Logout();
        RefreshPanel();
    }

    public void OnSaveDataButtonClicked()
    {
        NewDB.SaveUserProgress();
        RefreshAccountInfos();
    }
    
    public void OnLoadDataButtonClicked()
    {
        NewDB.LoadUserProgress(_ =>
        {
            mainController.RefreshLevelInfos();
            RefreshAccountInfos();
            mainController.RefreshOfficialLevels();
        });
    }

    public void OnTokenResetButtonClicked()
    {
        // todo: implement with popup
    }
}