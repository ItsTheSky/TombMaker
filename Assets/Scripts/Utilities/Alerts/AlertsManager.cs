using Unity.VisualScripting;
using UnityEngine;

public class AlertsManager : MonoBehaviour
{
    
    public static AlertsManager instance;

    private GameObject _alertPrefab;
    private GameObject _alertsContainer;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
    }
    
    public enum AlertType
    {
        Info,
        Warning,
        Error
    }

    public void ShowAlert0(string message, AlertType type = AlertType.Info)
    {
        CheckAlertsContainer();
        CheckAlertPrefab();
        
        var alert = Instantiate(_alertPrefab, _alertsContainer.transform);
        alert.GetComponent<AlertController>().Init(message, type);
    }
    
    private void CheckAlertsContainer()
    {
        if (_alertsContainer == null || _alertsContainer.IsDestroyed())
        {
            _alertsContainer = GameObject.Find("AlertsContainer");
        }
    }
    
    private void CheckAlertPrefab()
    {
        if (_alertPrefab == null || _alertPrefab.IsDestroyed())
        {
            _alertPrefab = Resources.Load<GameObject>("Alert");
        }
    }
    
    // ####################################################################################################
    
    public static void ShowAlert(string message, AlertType type = AlertType.Info)
    {
        instance.ShowAlert0(message, type);
    }
    
    public static void ShowConnectionError()
    {
        ShowAlert("Error while connecting to the server.", AlertType.Error);
    }
}