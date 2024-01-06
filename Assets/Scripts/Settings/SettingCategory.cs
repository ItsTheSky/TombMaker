public class SettingCategory
{

    public readonly string name;
    public readonly GlobalSetting[] settings;
    
    public SettingCategory(string name, GlobalSetting[] settings)
    {
        this.name = name;
        this.settings = settings;
    }
}