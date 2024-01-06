using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public abstract class GlobalSetting
{
    
    public string id;
    public string name;
    public string description;
    public SettingType type;
    
    [CanBeNull] public SettingController attachedSettingController;

    public GameObject AddSelf(GameObject container)
    {
        return type.AddSetting(container, this);
    }
    
    public virtual void SetValue(object value)
    {
        attachedSettingController?.SetValue(value);
    }
    
    public abstract string Serialize();
    public abstract void Deserialize(string serialized);

    public virtual void ForceUpdate()
    {
        
    }
    
    public abstract bool ShouldShow();
}

public abstract class SettingType
{
    public abstract GameObject AddSetting(GameObject container, GlobalSetting setting);
}

public abstract class SettingController : MonoBehaviour
{
    
    //public SettingsController attachedController;
    public GlobalSetting attachedOption;

    public void ShowHelp()
    {
        //attachedController.ShowHelp(attachedOption);
    }

    public abstract void SetValue(object value);
}