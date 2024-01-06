using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public static class GlobalIOManager
{
    
    public static PlayerStatsManager.StatsData StatsData => GetStatsData();
    public static Dictionary<string, string> Settings => GetSettings();
    public static int CurrentLevel => GetLevel();
    private static Dictionary<string, string> _settings;
    
    private static bool _isInitialized = false;
    
    public static void CheckInitialization()
    {
        if (!_isInitialized)
            Initialize();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public static void Save()
    {
        CheckInitialization();
        
        // Save the data
        var json = JsonConvert.SerializeObject(new Data
        {
            statsData = PlayerStatsManager.GetData(),
            settings = _settings
        });
        var compressed = Utilities.NewCompress(json);
        var encrypted = Utilities.XOREncryptDecrypt(compressed);
        
        var path = Application.persistentDataPath + "/global.dat";
        File.WriteAllBytes(path, Encoding.UTF8.GetBytes(encrypted));
    }

    public static void Initialize()
    {
        _isInitialized = true;
        
        // Load the data
        var path = Application.persistentDataPath + "/global.dat";
        if (!File.Exists(path))
        {
            PlayerStatsManager.SetData(new PlayerStatsManager.StatsData());
            _settings = new Dictionary<string, string>();
            return;
        }
        
        var compressed = Utilities.XOREncryptDecrypt(File.ReadAllText(path));
        var json = Utilities.NewDecompress(compressed);
        var data = JsonConvert.DeserializeObject<Data>(json);
        
        // Load the data
        _settings = data.settings;
        PlayerStatsManager.SetData(data.statsData);
        
        // Check if the data is valid
        _settings ??= new Dictionary<string, string>();
    }
    
    private static PlayerStatsManager.StatsData GetStatsData()
    {
        CheckInitialization();
        return PlayerStatsManager.GetData();
    }
    
    private static Dictionary<string, string> GetSettings()
    {
        CheckInitialization();
        return _settings;
    }
    
    private static int GetLevel()
    {
        CheckInitialization();
        return PlayerStatsManager.GetCurrentHighestLevel();
    }
    
    [Serializable]
    private class Data
    {
        
        public PlayerStatsManager.StatsData statsData;
        public Dictionary<string, string> settings;
        
    }

}