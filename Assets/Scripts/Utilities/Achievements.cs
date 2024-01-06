using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class Achievements
{
    
    public static Action<Achievement> OnAchievementUnlocked = delegate { };

    #region Achievements

    // Deaths
    public static readonly Achievement FirstDeath = new (0, "First Death", "Die for the first time", "death", AchievementType.Instant, coins: 20, groupId: 0);
    public static readonly Achievement Deaths10 = new (1, "10 Deaths", "Die 10 times", "death", AchievementType.Incremental, 10, coins: 100, groupId: 0);
    public static readonly Achievement Deaths50 = new (2, "50 Deaths", "Die 50 times", "death", AchievementType.Incremental, 50, coins: 500, groupId: 0);
    public static readonly Achievement Deaths100 = new (3, "100 Deaths", "Die 100 times", "death", AchievementType.Incremental, 100, coins: 1000, groupId: 0);
    public static readonly Achievement Deaths500 = new (4, "500 Deaths", "Die 500 times", "death", AchievementType.Incremental, 500, coins: 5000, groupId: 0);
    public static readonly Achievement Deaths1000 = new (5, "1 000 Deaths", "Die 1 000 times", "death", AchievementType.Incremental, 1000, coins: 10000, groupId: 0);
    
    // Completed Levels
    public static readonly Achievement FirstLevel = new (6, "First Level", "Complete your first level", "levels", AchievementType.Instant, coins: 50, groupId: 1);
    public static readonly Achievement Levels10 = new (7, "5 Levels", "Complete 5 levels", "levels", AchievementType.Incremental, 5, coins: 100, groupId: 1);
    public static readonly Achievement Levels50 = new (8, "10 Levels", "Complete 10 levels", "levels", AchievementType.Incremental, 10, coins: 500, groupId: 1);
    public static readonly Achievement Levels100 = new (9, "25 Levels", "Complete 25 levels", "levels", AchievementType.Incremental, 25, coins: 1000, groupId: 1);
    public static readonly Achievement Levels500 = new (10, "50 Levels", "Complete 50 levels", "levels", AchievementType.Incremental, 50, coins: 5000, groupId: 1);
    
    // Dots
    public static readonly Achievement Dots100 = new (11, "100 Dots", "Collect 100 dots", "dots", AchievementType.Incremental, 100, coins: 50, groupId: 2);
    public static readonly Achievement Dots500 = new (12, "500 Dots", "Collect 500 dots", "dots", AchievementType.Incremental, 500, coins: 100, groupId: 2);
    public static readonly Achievement Dots1000 = new (13, "1 000 Dots", "Collect 1 000 dots", "dots", AchievementType.Incremental, 1000, coins: 500, groupId: 2);
    public static readonly Achievement Dots5000 = new (14, "5 000 Dots", "Collect 5 000 dots", "dots", AchievementType.Incremental, 5000, coins: 1000, groupId: 2);
    public static readonly Achievement Dots10000 = new (15, "10 000 Dots", "Collect 10 000 dots", "dots", AchievementType.Incremental, 10000, coins: 5000, groupId: 2);
    
    // Shields
    public static readonly Achievement FirstShield = new (16, "First Shield", "Use your first shield", "shield", AchievementType.Instant, coins: 100, groupId: 3);
    public static readonly Achievement Shields10 = new (17, "10 Shields", "Use 10 shields", "shield", AchievementType.Incremental, 10, coins: 500, groupId: 3);
    public static readonly Achievement Shields25 = new (18, "25 Shields", "Use 25 shields", "shield", AchievementType.Incremental, 25, coins: 1000, groupId: 3);
    public static readonly Achievement Shields50 = new (19, "50 Shields", "Use 50 shields", "shield", AchievementType.Incremental, 50, coins: 5000, groupId: 3);
    
    // User levels
    public static readonly Achievement FirstUserLevel = new (20, "Starter", "Reach user level 3", "userlevel", AchievementType.Instant, coins: 100, groupId: 4);
    public static readonly Achievement UserLevel5 = new (21, "Confirmed", "Reach user level 5", "userlevel", AchievementType.Instant, coins: 500, groupId: 4);
    public static readonly Achievement UserLevel10 = new (22, "Veteran", "Reach user level 10", "userlevel", AchievementType.Instant, coins: 1000, groupId: 4);
    public static readonly Achievement UserLevel20 = new (23, "Expert", "Reach user level 20", "userlevel", AchievementType.Instant, coins: 5000, groupId: 4);
    public static readonly Achievement UserLevel50 = new (24, "God", "Reach user level 50", "userlevel", AchievementType.Instant, coins: 10000, groupId: 4);
    
    // Editor
    public static readonly Achievement CreateLevel = new (25, "Creator", "Create your first level", "world", AchievementType.Instant, coins: 100);
    public static readonly Achievement ShareLevel = new (26, "Worldwide", "Share your first level", "world", AchievementType.Instant, coins: 150);
    
    public static readonly Achievement Place100Blocks = new (27, "Builder", "Place 100 blocks", "editor", AchievementType.Incremental, 100, coins: 100, groupId: 5);
    public static readonly Achievement Place500Blocks = new (28, "Architect", "Place 500 blocks", "editor", AchievementType.Incremental, 500, coins: 500, groupId: 5);
    public static readonly Achievement Place2000Blocks = new (29, "Engineer", "Place 2 000 blocks", "editor", AchievementType.Incremental, 2000, coins: 1000, groupId: 5);

    #endregion
    
    #region Achievement Categories
    
    public static readonly AchievementCategory Deaths = new ("Deaths", new List<Achievement>
    { FirstDeath, Deaths10, Deaths50, Deaths100, Deaths500, Deaths1000 });
    
    public static readonly AchievementCategory Levels = new ("Levels", new List<Achievement>
    { FirstLevel, Levels10, Levels50, Levels100, Levels500 });
    
    public static readonly AchievementCategory Dots = new ("Dots", new List<Achievement>
    { Dots100, Dots500, Dots1000, Dots5000, Dots10000 });
    
    public static readonly AchievementCategory Shields = new ("Shields", new List<Achievement>
    { FirstShield, Shields10, Shields25, Shields50 });
    
    public static readonly AchievementCategory UserLevels = new ("User Levels", new List<Achievement>
    { FirstUserLevel, UserLevel5, UserLevel10, UserLevel20, UserLevel50 });
    
    public static readonly AchievementCategory Editor = new ("Editor", new List<Achievement>
    { CreateLevel, ShareLevel, Place100Blocks, Place500Blocks, Place2000Blocks });
    
    #endregion

    #region Meta

    public static readonly List<AchievementCategory> AchievementCategories = new ()
    {
        Deaths, Levels, Dots, Shields, UserLevels, Editor
    };
    private static readonly List<int> unlockedAchievements = new ();
    private static readonly List<int> collectedAchievements = new ();
    private static readonly Dictionary<int, int> incrementalAchievements = new ();
    
    public class Achievement
    {

        public readonly int id;
        public readonly int groupId = -1;
        public readonly string name;
        public readonly string description;
        public readonly AchievementType type;
        public readonly string icon;
        
        // Incremental
        public readonly int target;
        
        // Rewards
        public readonly int coins;
        public readonly int skin;
        public readonly int trail;

        public Achievement(int id, string name, string description, string icon, AchievementType type, int target = 0,
            int coins = -1, int skin = -1, int trail = -1, int groupId = -1)
        {
            this.id = id;
            this.groupId = groupId;
            this.name = name;
            this.description = description;
            this.icon = icon;
            this.type = type;
            this.target = target;
            this.coins = coins;
            this.skin = skin;
            this.trail = trail;
        }
    }
    
    public enum AchievementType
    {
        Instant,
        Incremental
    }
    
    public class AchievementCategory
    {

        public string name;
        public List<Achievement> achievements;
        
        public AchievementCategory(string name, List<Achievement> achievements)
        {
            this.name = name;
            this.achievements = achievements;
        }

    }
    
    #endregion

    #region I/O

    public class AchievementData
    {
        
        public List<int> unlockedAchievements;
        public Dictionary<int, int> incrementalAchievements;
        public List<int> collectedAchievements;

        public AchievementData(List<int> unlockedAchievements, Dictionary<int, int> incrementalAchievements, List<int> collectedAchievements = null)
        {
            this.unlockedAchievements = unlockedAchievements;
            this.incrementalAchievements = incrementalAchievements;
            this.collectedAchievements = collectedAchievements;
        }
        
    }
    
    private static bool _loaded = false;
    
    public static void SaveAchievements()
    {
        var data = new AchievementData(unlockedAchievements, incrementalAchievements, collectedAchievements);
        var json = JsonConvert.SerializeObject(data);
        var compressed = Utilities.NewCompress(json);
        
        PlayerPrefs.SetString("achievements", compressed);
    }
    
    public static void LoadAchievements()
    {
        if (_loaded)
            return;
        _loaded = true;
        
        var compressed = PlayerPrefs.GetString("achievements");
        if (compressed == "") return;
        
        var json = Utilities.NewDecompress(compressed);
        var data = JsonConvert.DeserializeObject<AchievementData>(json);
        
        unlockedAchievements.Clear();
        incrementalAchievements.Clear();
        collectedAchievements.Clear();
        
        unlockedAchievements.AddRange(data.unlockedAchievements);
        collectedAchievements.AddRange(data.collectedAchievements);
        foreach (var achievement in data.incrementalAchievements)
            incrementalAchievements.Add(achievement.Key, achievement.Value);
    }

    #endregion

    #region Manipulation

    public static void IncrementAchievement(Achievement achievement, int amount = 1, bool saving = true)
    {
        LoadAchievements();
        if (achievement.type != AchievementType.Incremental && !IsUnlocked(achievement))
        {
            UnlockAchievement(achievement, false);
            return;
        }
        
        incrementalAchievements.TryAdd(achievement.id, 0);
        incrementalAchievements[achievement.id] += amount;
        
        if (incrementalAchievements[achievement.id] >= achievement.target)
            UnlockAchievement(achievement, false);
        
        if (saving) SaveAchievements();
    }
    
    public static void IncrementAchievementGroup(int groupId, int amount = 1)
    {
        LoadAchievements();
        foreach (var category in AchievementCategories)
        {
            foreach (var achievement in category.achievements)
            {
                if (achievement.groupId == groupId)
                    IncrementAchievement(achievement, amount, false);
            }
        }
        
        SaveAchievements();
    }
    
    public static void UnlockAchievement(Achievement achievement, bool saving = true)
    {
        LoadAchievements();
        if (unlockedAchievements.Contains(achievement.id))
            return;
        
        unlockedAchievements.Add(achievement.id);
        
        if (achievement.type == AchievementType.Incremental)
            incrementalAchievements[achievement.id] = achievement.target;
        
        if (saving) SaveAchievements();
        
        OnAchievementUnlocked(achievement);
    }
    
    public static bool IsUnlocked(Achievement achievement)
    {
        LoadAchievements();
        
        return unlockedAchievements.Contains(achievement.id);
    }
    
    public static int GetProgress(Achievement achievement)
    {
        LoadAchievements();
        if (achievement.type != AchievementType.Incremental)
            return 0;
        
        incrementalAchievements.TryAdd(achievement.id, 0);
        return incrementalAchievements[achievement.id];
    }

    public static AchievementData GetData()
    {
        LoadAchievements();
        
        return new AchievementData(unlockedAchievements, incrementalAchievements, collectedAchievements);
    }

    public static void SetData(AchievementData saveAchievements)
    {
        unlockedAchievements.Clear();
        incrementalAchievements.Clear();
        collectedAchievements.Clear();
        
        unlockedAchievements.AddRange(saveAchievements.unlockedAchievements);
        collectedAchievements.AddRange(saveAchievements.collectedAchievements);
        foreach (var achievement in saveAchievements.incrementalAchievements)
            incrementalAchievements.Add(achievement.Key, achievement.Value);
    }
    
    public static bool IsCollected(Achievement achievement)
    {
        LoadAchievements();
        
        return collectedAchievements.Contains(achievement.id);
    }

    public static void CollectRewards(Achievement achievement)
    {
        LoadAchievements();
        if (collectedAchievements.Contains(achievement.id))
            return;
        
        if (achievement.coins != -1) PlayerStatsManager.AddCoins(achievement.coins);
        if (achievement.skin != -1) PlayerStatsManager.UnlockSkin(achievement.skin);
        if (achievement.trail != -1) PlayerStatsManager.UnlockTrail(achievement.trail);
        
        collectedAchievements.Add(achievement.id);
        SaveAchievements();
    }
    
    public static void ClearAll()
    {
        LoadAchievements();
        
        unlockedAchievements.Clear();
        incrementalAchievements.Clear();
        collectedAchievements.Clear();
        
        SaveAchievements();
    }

    #endregion
    
}