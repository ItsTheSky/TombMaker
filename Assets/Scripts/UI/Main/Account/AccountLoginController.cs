using TMPro;
using UnityEngine;

public class AccountLoginController : MonoBehaviour
{
    public AccountController accountController;
        
    public bool isLogin;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;

    public void OnLoginButtonClicked()
    {
        errorText.text = "";
        
        if (usernameInput.text.Length == 0 || passwordInput.text.Length == 0)
        {
            errorText.text = "Please enter a username and password.";
            return;
        }
        
        errorText.text = "Loading...";

        if (isLogin)
        {
            LoginAsync();
        }
        else
        {
            RegisterAsync();
        }
    }

    public async void LoginAsync()
    {
        var response = await NewDB.Login(usernameInput.text, passwordInput.text);
        
        if (response.status != "ok")
        {
            errorText.text = response.message;
            return;
        }

        accountController.RefreshPanel();
        errorText.text = "Login successful!";
    }

    public async void RegisterAsync()
    {
        var response = await NewDB.Register(usernameInput.text, passwordInput.text);
        
        if (response.status == "error")
        {
            errorText.text = response.message;
            return;
        }

        accountController.OnLoginButtonClicked();
        errorText.text = "Registration successful!";
    }
}