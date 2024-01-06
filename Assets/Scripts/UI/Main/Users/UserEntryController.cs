using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserEntryController : MonoBehaviour
{

    [HideInInspector] public UsersController controller;
    
    public Image background;
    
    public TMP_Text username;
    
    public TMP_Text column1;
    public TMP_Text column2;
    public TMP_Text column3;
    
    public void Init(UsersController controller, string username, string column1 = "", string column2 = "", string column3 = "")
    {
        this.controller = controller;
        
        this.username.text = username;
        
        this.column1.text = column1;
        this.column2.text = column2;
        this.column3.text = column3;
    }

    public void SetShadow(bool shadow)
    {
        background.color = shadow ? new Color(215f / 255f, 215f / 255f, 0) : new Color(1, 1, 0);
    }
}