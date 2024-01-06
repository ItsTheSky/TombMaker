using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public static class DataManager
{
    public static readonly string DataFileExtension = ".bin";

    public static void SaveData(SaveData saveData, string name)
    {
        string path = Application.persistentDataPath + "/"+ FormatString(name) +".bytes";

        // To JSON
        var json = JsonConvert.SerializeObject(saveData);
        var compressed = Utilities.NewCompress(json);

        // save
        File.WriteAllText(path, compressed);
    }

    public static SaveData LoadDataFromFile(TextAsset asset)
    {
        var json = Utilities.NewDecompress(asset.text);
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        
        if (saveData == null)
            throw new Exception("Save data is null");

        return saveData;
    }

    public static string FormatString(string str)
    {
        return str.Replace(" ", "_").ToLower();
    }

    public static void CheckFolders()
    {
    }
    
    // Local levels
    private static readonly string _localLevelsPath = Application.persistentDataPath + "/localLevels.bin";
    
    private static readonly Dictionary<int, CustomLevelInfo> _localLevels = new ();
    private static int _localLevelsId = 0;

    private static bool _localLevelsInitialized;
    
    public static void SaveLocalLevels(bool overwrite = false)
    {
        if (overwrite) File.Delete(_localLevelsPath);
        
        var json = JsonConvert.SerializeObject(new CustomLevels() { customLevels = _localLevels.Values.ToArray() });
        var compressed = Utilities.NewCompress(json); 
        
        File.WriteAllText(_localLevelsPath, compressed);
    }

    public static void LoadLocalLevels()
    {
        _localLevelsInitialized = true;
        
        if (File.Exists(_localLevelsPath))
        {
            if (!File.Exists(_localLevelsPath))
                File.WriteAllText(_localLevelsPath, Utilities.NewCompress(JsonConvert.SerializeObject(new CustomLevels() { customLevels = Array.Empty<CustomLevelInfo>() })));
           
            var stream = new FileStream(_localLevelsPath, FileMode.Open);
            var formatter = new BinaryFormatter();
            CustomLevels levels = null;
            try
            {
                levels = formatter.Deserialize(stream) as CustomLevels;
            }
            catch (Exception _)
            {
                // ignored
            }
            
            stream.Close();

            if (levels == null) // it's using the 'uncompressed' format from older version.
            {
                var json = Utilities.NewDecompress(File.ReadAllText(_localLevelsPath));
                levels = JsonConvert.DeserializeObject<CustomLevels>(json);
            }
            
            if (levels == null)
                throw new Exception("Saved levels are null");

            bool needToSave = false;
            _localLevels.Clear();
            foreach (var level in levels.customLevels)
            {
                if (level.id == 0) 
                {
                    level.id = ++_localLevelsId;
                    needToSave = true;
                }
                
                _localLevels.Add(level.id, level);
                if (level.id > _localLevelsId)
                    _localLevelsId = level.id;
            }

            if (needToSave)
            {
                SaveLocalLevels(true);
            }
        }
        else
        {
            _localLevels.Clear();
        }

        Debug.Log("Loaded " + _localLevels.Count + " local levels:");
        foreach (var level in _localLevels)
        {
            Debug.Log(" - " + level.Value.name + " (" + level.Value.id + ")");
        }
    }
    
    public static CustomLevelInfo GetLocalLevel(int localId)
    {
        if (!_localLevelsInitialized)
            LoadLocalLevels();
        
        return _localLevels[localId];
    }
    
    public static void SetLocalLevels(List<CustomLevelInfo> levels)
    {
        CheckFolders();
        if (!_localLevelsInitialized)
            LoadLocalLevels();
        
        _localLevels.Clear();
        foreach (var level in levels)
        {
            if (level.id == 0)
                level.id = ++_localLevelsId;
            
            _localLevels.Add(level.id, level);
            if (level.id > _localLevelsId)
                _localLevelsId = level.id;
        }
        SaveLocalLevels();
    }
    
    public static void AddLocalLevel(CustomLevelInfo level)
    {
        CheckFolders();
        if (!_localLevelsInitialized)
            LoadLocalLevels();
        
        _localLevels.Add(++_localLevelsId, level);
        SaveLocalLevels();
    }

    public static void SetLocalLevel(CustomLevelInfo level)
    {
        CheckFolders();
        if (!_localLevelsInitialized)
            LoadLocalLevels();
        
        _localLevels[level.id] = level;
        SaveLocalLevels();
    }

    public static void RemoveLocalLevel(int id)
    {
        CheckFolders();
        if (!_localLevelsInitialized)
            LoadLocalLevels();
        
        _localLevels.Remove(id);
        SaveLocalLevels();
    }
    
    public static List<CustomLevelInfo> GetLocalLevels()
    {
        CheckFolders();
        if (!_localLevelsInitialized)
            LoadLocalLevels();
        
        return _localLevels.Values.ToList();
    }

    public static bool IsLocalLevelNameValid(string name)
    {
        CheckFolders();
        if (!_localLevelsInitialized)
            LoadLocalLevels();
        if (name.Length >= 50) // max length
            return false;

        return true;
    }

    // Global datas

    /**
     * Custom level info
     */
    [Serializable]
    public class CustomLevelInfo
    {

        public int id; // the id of the level in the local database
        public string name;
        public string description;
        public bool completed; // if the current version of the level is completed, so can be published
        public bool published; // if the level is published
        public int publishId; // the id of the level on the server (if published), else -1
        [SerializeField]
        public SaveData saveData; // the save data of the level
        
        public long lastModified = DateTime.Now.Ticks; // the last modified date of the level
        public long creationDate = DateTime.Now.Ticks; // the creation date of the level
        
        public bool verified; // if the level is verified by the user, and thus can be published
        public int coloredBlocksCount; // when verified, the number of colored blocks in the level
        
        public NewDB.LevelOnlineSettings onlineSettings = new (); // the online settings of the level

        public CustomLevelInfo(NewDB.CompressedCustomLevelInfo compressedCustomLevelInfo)
        {
            id = compressedCustomLevelInfo.id;
            name = compressedCustomLevelInfo.name;
            description = compressedCustomLevelInfo.description;
            completed = compressedCustomLevelInfo.completed;
            published = compressedCustomLevelInfo.published;
            publishId = compressedCustomLevelInfo.publishId;
            lastModified = compressedCustomLevelInfo.lastModified;
            creationDate = compressedCustomLevelInfo.creationDate;
            verified = compressedCustomLevelInfo.verified;
            coloredBlocksCount = compressedCustomLevelInfo.coloredBlocksCount;
            onlineSettings = compressedCustomLevelInfo.onlineSettings;
            
            var json = Utilities.NewDecompress(compressedCustomLevelInfo.saveData);
            saveData = JsonConvert.DeserializeObject<SaveData>(json);
        }

        public CustomLevelInfo(CustomLevelInfo other)
        {
            id = other.id;
            name = other.name;
            description = other.description;
            completed = other.completed;
            published = other.published;
            publishId = other.publishId;
            lastModified = other.lastModified;
            creationDate = other.creationDate;
            verified = other.verified;
            coloredBlocksCount = other.coloredBlocksCount;
            saveData = other.saveData;
            onlineSettings = other.onlineSettings;
        }

        public CustomLevelInfo()
        {
            onlineSettings ??= new NewDB.LevelOnlineSettings();
        }
        
        public CustomLevelInfo(NewDB.Level level, SaveData saveData)
        {
            name = level.name;
            description = level.description;
            completed = false;
            published = false;
            publishId = -1;
            lastModified = DateTime.Now.Ticks;
            creationDate = DateTime.Now.Ticks;
            verified = false;
            coloredBlocksCount = 0;
            this.saveData = saveData;
            onlineSettings = new NewDB.LevelOnlineSettings();
        }
    }

    [Serializable]
    public class CustomLevels
    {
        
        public CustomLevelInfo[] customLevels;
        
    }
    
    public static int NextLocalLevelId()
    {
        return ++_localLevelsId;
    }
    
}
