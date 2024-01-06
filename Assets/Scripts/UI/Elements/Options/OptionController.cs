using UnityEngine;
using UnityEngine.UI;

public abstract class OptionController : MonoBehaviour
{
        
    public Button helpButton;
    
    public BlockSettingController attachedController;
    public BlockSettingController.BlockOption attachedOption;

    public void ShowHelp()
    {
        attachedController.ShowHelp(attachedOption);
    }

    public abstract void SetValue(object value);
}