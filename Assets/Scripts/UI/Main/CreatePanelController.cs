using UnityEngine;

public class CreatePanelController : MonoBehaviour
{

    public GameObject editPanel;
    public GameObject browsePanel;
    public AccountController accountController;

    // button events
    
    public void OpenEditPanel()
    {
        editPanel.GetComponent<EditPanelController>().InitLevels();
        editPanel.SetActive(true);
    }
    
    public void OpenSavedPanel()
    {
        browsePanel.GetComponent<BrowseLevelsControlle>().LoadLevels();
        browsePanel.SetActive(true);
    }
    
    public void OpenAccountPanel()
    {
        accountController.RefreshPanel();
        accountController.gameObject.SetActive(true);
    }

    public void CloseEditPanel()
    {
        editPanel.SetActive(false);
    }
    
    public void CloseAccountPanel()
    {
        accountController.gameObject.SetActive(false);
    }
}