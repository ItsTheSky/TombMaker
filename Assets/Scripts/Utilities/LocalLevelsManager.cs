using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public static class LocalLevelsManager
{
 
    private static readonly string _levelsPath = Application.persistentDataPath + "/levels/";
    
    private static readonly Dictionary<int, SavedLevel> _levels = new ();
    private static bool _loaded;
    
    [Serializable]
    private class SavedLevel
    {
        
        public NewDB.Level level;
        public NewDB.LevelStats stats;
        public NewDB.LevelOnlineSettings onlineSettings;
        [CanBeNull] public SaveData saveData;
        
    }
    
    // ###########################################################################################
    
    private static void CheckLoaded()
    {
        if (!_loaded)
        {
            LoadLevels();
            _loaded = true;
        }
    }
    
    public static void LoadLevels()
    {
        _levels.Clear();
        
        if (!Directory.Exists(_levelsPath))
            Directory.CreateDirectory(_levelsPath);
        
        foreach (var file in Directory.GetFiles(_levelsPath))
        {
            var savedLevel = JsonConvert.DeserializeObject<SavedLevel>(Utilities.NewDecompress(File.ReadAllText(file)));
            
            _levels.Add(savedLevel.level.id, savedLevel);
        }
    }
    
    public static void SaveLevel(NewDB.Level level, NewDB.LevelStats stats, NewDB.LevelOnlineSettings onlineSettings, SaveData saveData = null)
    {
        CheckLoaded();
        
        var savedLevel = new SavedLevel
        {
            level = level,
            stats = stats,
            onlineSettings = onlineSettings,
            saveData = saveData
        };
        
        var json = JsonConvert.SerializeObject(savedLevel, Formatting.None, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        
        File.WriteAllText(_levelsPath + level.id + ".dat", Utilities.NewCompress(json));
        
        _levels[level.id] = savedLevel;
    }
    
    public static void DeleteLevel(int id)
    {
        CheckLoaded();
        File.Delete(_levelsPath + id + ".dat");
        
        _levels.Remove(id);
    }
    
    public static bool LevelExists(int id)
    {
        CheckLoaded();
        return _levels.ContainsKey(id);
    }
    
    public static List<int> GetLevels()
    {
        CheckLoaded();
        return new List<int>(_levels.Keys);
    }
    
    public static NewDB.Level GetLevel(int id)
    {
        CheckLoaded();
        if (!_levels.ContainsKey(id))
            return null;
        
        return _levels[id].level;
    }
    
    public static SaveData GetSaveData(int id)
    {
        CheckLoaded();
        if (!_levels.ContainsKey(id))
            return null;
        
        return _levels[id].saveData;
    }
    
    public static NewDB.LevelStats GetStats(int id)
    {
        CheckLoaded();
        if (!_levels.ContainsKey(id))
            return null;
        
        return _levels[id].stats;
    }
    
    public static NewDB.LevelOnlineSettings GetOnlineSettings(int id)
    {
        CheckLoaded();
        if (!_levels.ContainsKey(id))
            return null;
        
        return _levels[id].onlineSettings;
    }
    
    public static void DeleteAllLevels()
    {
        CheckLoaded();
        foreach (var file in Directory.GetFiles(_levelsPath))
            File.Delete(file);
        
        _levels.Clear();
    }
    
    public static void SaveLevel(int id, NewDB.Level level, SaveData saveData = null)
    {
        CheckLoaded();
        var savedLevel = new SavedLevel
        {
            level = level,
            saveData = saveData
        };
        
        var json = JsonConvert.SerializeObject(savedLevel, Formatting.None, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        
        File.WriteAllText(_levelsPath + id + ".dat", Utilities.NewCompress(json));
        
        _levels[id] = savedLevel;
    }
    
}