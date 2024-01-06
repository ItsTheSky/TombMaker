using System.Diagnostics;
using System.IO;
using CommandTerminal;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Commands
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR || CHEATS
    
    [RegisterCommand(Help = "Save the level as official encoding.", MinArgCount = 1, MaxArgCount = 1)]
    static void ExportLevel(CommandArg[] args) {
        string levelName = args[0].String;
        if (Terminal.IssuedError)
            return; // Error will be handled by Terminal

        var save = new SaveData(EditorValues.PlayerScript.logic);
        DataManager.SaveData(save, levelName);
        Terminal.Log("Level saved as official encoding (level name: " + levelName + ").");
    }
    
    [RegisterCommand(Help = "Open the data save directory.")]
    static void Directory(CommandArg[] args) {
        Process.Start(Application.persistentDataPath);
    }
    
    [RegisterCommand(Help = "Convert the level to a custom level format.", MinArgCount = 1, MaxArgCount = 1)]
    static void ImportLevel(CommandArg[] args) {
        string levelName = args[0].String;
        if (Terminal.IssuedError) 
            return; // Error will be handled by Terminal
        
        var logic = EditorValues.PlayerScript.logic;
        var save = new SaveData(logic);
        var cli = new DataManager.CustomLevelInfo()
        {
            completed = false,
            published = false,
            saveData = save,
            name = levelName,
            publishId = -1
        };

        DataManager.AddLocalLevel(cli);
        Terminal.Log("Level saved as custom level (level name: " + levelName + ").");
    }
    
    [RegisterCommand(Help = "Show how much coins you have.")]
    static void Coins(CommandArg[] args) {
        Terminal.Log("You have " + PlayerStatsManager.GetCoins() + " coins.");
    }
    
    [RegisterCommand(Help = "Change the selected skin ID.", MinArgCount = 1, MaxArgCount = 1)]
    static void Skin(CommandArg[] args) {
        long skinId = args[0].Int;
        if (Terminal.IssuedError) 
            return; // Error will be handled by Terminal
        
        PlayerStatsManager.SetSelectedSkin(skinId);
        Terminal.Log("Skin changed to " + skinId + ". Restart of the level may be needed.");
    }
    
    [RegisterCommand(Help = "Change the selected trail ID.", MinArgCount = 1, MaxArgCount = 1)]
    static void trail(CommandArg[] args) {
        long skinId = args[0].Int;
        if (Terminal.IssuedError) 
            return; // Error will be handled by Terminal
        
        PlayerStatsManager.SetSelectedTrail(skinId);
        Terminal.Log("Trail changed to " + skinId + ". Restart of the level may be needed.");
    }
    
    [RegisterCommand(Help = "Unlock a specific skin.", MinArgCount = 1, MaxArgCount = 1)]
    static void UnlockSkin(CommandArg[] args) {
        long skinId = args[0].Int;
        if (Terminal.IssuedError) 
            return; // Error will be handled by Terminal
        
        PlayerStatsManager.UnlockSkin(skinId);
        Terminal.Log("Skin " + skinId + " unlocked.");
    }
    
    [RegisterCommand(Help = "Lock a specific skin.", MinArgCount = 1, MaxArgCount = 1)]
    static void LockSkin(CommandArg[] args) {
        long skinId = args[0].Int;
        if (Terminal.IssuedError) 
            return; // Error will be handled by Terminal
        
        PlayerStatsManager.LockSkin(skinId);
        Terminal.Log("Skin " + skinId + " locked.");
    }
    
    [RegisterCommand(Help = "Change the amount of coins you have.", MinArgCount = 1, MaxArgCount = 1)]
    static void SetCoins(CommandArg[] args) {
        int coins = args[0].Int;
        if (Terminal.IssuedError) 
            return; // Error will be handled by Terminal
        
        PlayerStatsManager.SetCoins(coins);
        Terminal.Log("Coins set to " + coins + ".");
    }
    
    [RegisterCommand(Help = "Lock all the skins except the default one.")]
    static void LockAllSkins(CommandArg[] args) {
        PlayerStatsManager.LockAllSkins();
        PlayerStatsManager.SetSelectedSkin(0);
        Terminal.Log("All skins locked.");
    }
    
    [RegisterCommand(Help = "Lock all the trails except the default one.")]
    static void LockAllTrails(CommandArg[] args) {
        PlayerStatsManager.LockAllTrails();
        PlayerStatsManager.SetSelectedTrail(0);
        Terminal.Log("All trails locked.");
    }
    
    [RegisterCommand(Help = "Reset bonuses")]
    static void ResetBonuses(CommandArg[] args) {
        PlayerStatsManager.ResetBonusLevels();
        Terminal.Log("Bonuses reset.");
    }
    
    [RegisterCommand(Help = "Reset Levels Reward")]
    static void ResetRewards(CommandArg[] args) {
        PlayerStatsManager.ResetRewards();
        Terminal.Log("Rewards reset.");
    }
    
    [RegisterCommand(Help = "Get current event skin ID.")]
    static async void EventSkin(CommandArg[] args)
    {
        //var skinId = await DatabaseManager.GetEventSkin();
        Terminal.Log("This command is not available.");
    }
    
    [RegisterCommand(Help = "Lock all the levels (except first ofc)")]
    static void LockLevels(CommandArg[] args)
    {
        PlayerStatsManager.ResetLevelProgresses();
        Terminal.Log("All levels locked.");
    }
    
    [RegisterCommand(Help = "Reset player's level and experience")]
    static void ResetLevel(CommandArg[] args)
    {
        PlayerStatsManager.SetLevel(1);
        PlayerStatsManager.SetCurrentDots(0);
        Terminal.Log("Level reset.");
    }
    
    [RegisterCommand(Help = "Make a 'stress' test by adding 1k blocks.", MinArgCount = 1, MaxArgCount = 1)]
    static async void StressTest(CommandArg[] args)
    {
        var count = args[0].Int;
        var logic = EditorValues.LogicScript;
        
        logic.ChangeBlockType(12);
        for (int x = 0; x < count; x++)
        {
            for (int y = 0; y < count; y++)
            {
                logic.PlaceBlock(new Vector2(x, y), logic.blockPlacer.currentPrefab, true);
            }
        }
    }

    [RegisterCommand(Help = "Debug what user stats")]
    public static void DebugStats(CommandArg[] args)
    {
        var save = new NewDB.UserDataSave()
        {
            data = PlayerStatsManager.GetData(),
            levels = DataManager.GetLocalLevels().ConvertAll(level => new NewDB.CompressedCustomLevelInfo(level)),
        };
        var json = JsonConvert.SerializeObject(save);
        Debug.Log(json);
        Terminal.Log("Check console for the data.");
    }
    
    [RegisterCommand(Help = "Reset all player prefs from unity.")]
    public static void ResetPrefs(CommandArg[] args)
    {
        PlayerPrefs.DeleteAll();
        Terminal.Log("Player prefs reset.");
    }

    [RegisterCommand(Help = "Convert old levels encoding to new one (that are not bins)")]
    public static void ConvertAll(CommandArg[] args)
    {
        var saveDatas = LevelsUIManager.levels;
        int i = 0;
        foreach (var save in saveDatas)
        {
            i++;
            var json = JsonConvert.SerializeObject(save);
            var compressed = Utilities.NewCompress(json);
            
            File.WriteAllText(Application.persistentDataPath + "/OfficialLevels/" + i.ToString() + ".lvl", compressed);
        }
        
        Terminal.Log("Converted " + i + " levels.");
    }
    
    [RegisterCommand(Help = "Show an example notification about an achievement.")]
    public static void Achievement(CommandArg[] args)
    {
        AchievementNotifController.ShowAchievementNotifStatic(Achievements.Deaths10);
    }

    [RegisterCommand]
    public static void Pathfinding(CommandArg[] args)
    {
        var possibilities = Pathfinder.CalculatePossibilities(EditorValues.LogicScript);
        Terminal.Log("Possibilities: " + possibilities.Count);
    }
    
#endif
    
}