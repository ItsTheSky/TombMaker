using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;

public static class PlayerStatsManager // Manage player's stats internally
{

    #region Fields
    
    private static int coins;
    private static int shields;
    private static long selectedSkin;
    private static long selectedTrail;
    
    private static List<long> unlockedSkins = new () {0};
    private static List<long> unlockedTrails = new () {0};
    private static List<long> collectedRewards = new ();
    
    private static Dictionary<int, int> bonusLevels = new ();
    
    private static Dictionary<int, LevelProgressData> levelProgress = new ();
    private static Dictionary<int, LevelProgressData> officialLevelProgress = new ();

    private static long level = 1;
    private static long currentDots = 0;
    private static long requiredDots => GetRequiredExperience(level);
    
    #endregion
    
    #region Public Methods
    
    public static int GetRequiredExperience(long level)
    {
        return Mathf.CeilToInt(Mathf.Pow((level / 0.1f), 2));
    }
    
    public static void CollectCoin()
    {
        coins++;
        
        SaveStats();
    }
    
    public static int GetCoins()
    {
        return coins;
    }
    
    public static void RemoveCoins(int amount)
    {
        coins -= amount;
        
        SaveStats();
    }
    
    public static void SetCoins(int amount)
    {
        coins = amount;
        
        SaveStats();
    }
    
    public static void AddCoins(int amount)
    {
        coins += amount;
        
        SaveStats();
    }
    
    public static int GetShields()
    {
        return shields;
    }
    
    public static void SetShields(int amount)
    {
        shields = amount;
        
        SaveStats();
    }
    
    public static void AddShields(int amount)
    {
        shields += amount;
        
        SaveStats();
    }
    
    public static void SetSelectedSkin(long skin)
    {
        selectedSkin = skin;
        
        SaveStats();
    }
    
    public static long GetSelectedSkin()
    {
        return selectedSkin;
    }
    
    public static void SetSelectedTrail(long trail)
    {
        selectedTrail = trail;
        
        SaveStats();
    }
    
    public static long GetSelectedTrail()
    {
        return selectedTrail;
    }

    public static bool HasSkinUnlocked(long skin)
    {
        return unlockedSkins.Contains(skin);
    }
    
    public static bool HasTrailUnlocked(long trail)
    {
        return unlockedTrails.Contains(trail);
    }
    
    public static void UnlockSkin(long skin)
    {
        unlockedSkins.Add(skin);
        
        SaveStats();
    }
    
    public static void UnlockTrail(long trail)
    {
        unlockedTrails.Add(trail);
        
        SaveStats();
    }
    
    public static void LockSkin(long skin)
    {
        unlockedSkins.Remove(skin);
        
        SaveStats();
    }
    
    public static int GetCurrentHighestLevel()
    {
        return officialLevelProgress.Count > 0 ? officialLevelProgress.Keys.Max() : 1;
    }
    
    public static void LockAllSkins()
    {
        unlockedSkins = new List<long>() {0};
        
        SaveStats();
    }
    
    public static void LockAllTrails()
    {
        unlockedTrails = new List<long>() {0};
        
        SaveStats();
    }
    
    public static bool HasRewardCollected(long level)
    {
        return collectedRewards.Contains(level);
    }
    
    public static void CollectReward(long level)
    {
        collectedRewards.Add(level);
        
        SaveStats();
    }
    
    public static void ResetRewards()
    {
        collectedRewards = new List<long>();
        
        SaveStats();
    }
    
    public static int GetBonusLevel(int bonusId)
    {
        return bonusLevels.ContainsKey(bonusId) ? bonusLevels[bonusId] : 0;
    }
    
    public static void SetBonusLevel(int bonusId, int level)
    {
        bonusLevels[bonusId] = level;
        
        SaveStats();
    }
    
    public static void ResetBonusLevels()
    {
        bonusLevels = new Dictionary<int, int>();
        
        SaveStats();
    }
    
    public static void IncreaseBonusLevel(int bonusId)
    {
        if (!bonusLevels.ContainsKey(bonusId))
            bonusLevels[bonusId] = 0;
        
        bonusLevels[bonusId]++;
        
        SaveStats();
    }
    
    public static void SaveLevelProgress(LevelProgressData progress)
    {
        if (progress.official)
            officialLevelProgress[progress.levelId] = progress;
        else 
            levelProgress[progress.levelId] = progress;
        
        SaveStats();
    }
    
    public static LevelProgressData GetLevelProgress(int levelId, bool official = false)
    {
        var source = official ? officialLevelProgress : levelProgress;
        var value = source.TryGetValue(levelId, out var v) ? v : new LevelProgressData(levelId, official);
        if (value.official != official)
        {
            value.official = official;
            SaveLevelProgress(value);
        }
        
        return value;
    }
    
    public static void ResetLevelProgresses()
    {
        officialLevelProgress = new Dictionary<int, LevelProgressData>() { [1] = new (1) };
        levelProgress = new Dictionary<int, LevelProgressData>();

        SaveStats();
    }
    
    public static List<LevelProgressData> GetLevelProgresses()
    {
        
        var progresses = new List<LevelProgressData>(officialLevelProgress.Values);
        progresses.AddRange(levelProgress.Values);
        
        return progresses;
    }
    
    public static bool HasLevelProgress(int levelId, bool official = false)
    {
        var source = official ? officialLevelProgress : levelProgress;
        return source.ContainsKey(levelId);
    }
    
    public static int GetLevel()
    {
        return (int) level;
    }
    
    public static void SetLevel(int level)
    {
        PlayerStatsManager.level = level;
        
        SaveStats();
    }
    
    public static long GetCurrentDots()
    {
        return currentDots;
    }
    
    public static void SetCurrentDots(long dots)
    {
        currentDots = dots;
        
        SaveStats();
    }
    
    public static long GetRequiredDots()
    {
        return requiredDots;
    }
    
    public static void AddCollectedDots(long dots)
    {
        currentDots += dots;
        
        if (currentDots >= requiredDots)
        {
            currentDots -= requiredDots;
            level++;
            
            Achievements.IncrementAchievement(Achievements.FirstUserLevel);
            if (level >= 5) Achievements.IncrementAchievement(Achievements.UserLevel5);
            if (level >= 10) Achievements.IncrementAchievement(Achievements.UserLevel10);
            if (level >= 20) Achievements.IncrementAchievement(Achievements.UserLevel20);
            if (level >= 50) Achievements.IncrementAchievement(Achievements.UserLevel50);
        }
        
        SaveStats();
    }
    
    #endregion

    #region Serialization
    
    private static bool isInitialized;
    
    [Serializable]
    public class StatsData
    {
        public int coins;
        public int shields;
        public long selectedSkin;
        public long selectedTrail;
        public List<long> unlockedSkins;
        public List<long> unlockedTrails;
        public List<long> collectedRewards;
        public Dictionary<int, int> bonusLevels;
        public Dictionary<int, LevelProgressData> levelProgress;
        public Dictionary<int, LevelProgressData> officialLevelProgress;
        public long level;
        public long currentDots;
        public long requiredDots;
        
        public StatsData()
        {
            coins = 0;
            shields = 0;
            selectedSkin = 0;
            selectedTrail = 0;
            unlockedSkins = new List<long>() {0};
            unlockedTrails = new List<long>() {0};
            collectedRewards = new List<long>();
            bonusLevels = new Dictionary<int, int>();
            levelProgress = new Dictionary<int, LevelProgressData>();
            officialLevelProgress = new Dictionary<int, LevelProgressData>() { [1] = new (1) };
            level = 1;
            currentDots = 0;
            requiredDots = GetRequiredExperience(level);
        }
    }
    
    [Serializable]
    public class LevelProgressData
    {

        public int levelId;

        public int collectedDots;
        public int collectedStars;
        public int totalDeaths;
        public bool completed;
        public long date;

        public bool official;
        
        public LevelProgressData(int levelId, bool official = false)
        {
            this.levelId = levelId;
            
            collectedDots = 0;
            collectedStars = 0;
            totalDeaths = 0;
            completed = false;
            date = 0;
            this.official = official;
        }
        
        public void Complete()
        {
            completed = true;
            
            if (date == 0) 
                date = DateTimeOffset.Now.ToUnixTimeSeconds();
            
            NewDB.SaveUserProgress();
        }
        
        public void SetCollectedDots(int dots)
        {
            if (dots > collectedDots)
                collectedDots = dots;
        }
        
        public void SetCollectedStars(int stars)
        {
            if (stars > collectedStars)
                collectedStars = stars;
        }
        
        public void AddDeaths(int deaths)
        {
            totalDeaths += deaths;
        }
    }

    public static void SaveStats()
    {
        GlobalIOManager.Save();
        /*
        // We'll encode the class as binary data
        StatsData data = GetData();
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/stats.dat", FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close(); // now we use gzip
        
        string json = JsonConvert.SerializeObject(data);
        string compressed = Utilities.NewCompress(json);
        File.WriteAllText(Application.persistentDataPath + "/stats.dat", compressed); */
    }

    public static StatsData GetData()
    {
        return new ()
        {
            coins = coins,
            selectedSkin = selectedSkin,
            selectedTrail = selectedTrail,
            unlockedSkins = unlockedSkins,
            unlockedTrails = unlockedTrails,
            collectedRewards = collectedRewards,
            bonusLevels = bonusLevels,
            levelProgress = levelProgress,
            level = level,
            currentDots = currentDots,
            requiredDots = requiredDots,
            shields = shields,
            officialLevelProgress = officialLevelProgress
        };
    }
    
    public static void SetData(StatsData data)
    {
        coins = data.coins;
        selectedSkin = data.selectedSkin;
        selectedTrail = data.selectedTrail;
        unlockedSkins = data.unlockedSkins;
        unlockedTrails = data.unlockedTrails;
        collectedRewards = data.collectedRewards;
        bonusLevels = data.bonusLevels ?? new Dictionary<int, int>();
        levelProgress = data.levelProgress ?? new Dictionary<int, LevelProgressData>();
        level = data.level;
        currentDots = data.currentDots;
        shields = data.shields;
        officialLevelProgress = data.officialLevelProgress ?? new Dictionary<int, LevelProgressData>() { [1] = new (1) };
        
        SaveStats();
    }
    
    #endregion
}