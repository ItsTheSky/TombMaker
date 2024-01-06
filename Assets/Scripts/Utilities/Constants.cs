using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public static class Constants
{
    
    // ########################################
    // Skins constants
    // ########################################
    
    public static string GetPlayerIdleAnimationFromSkin(long skinId)
    {
        return GetSkinMeta(skinId).name + "Idle";
    }

    public static SkinMeta GetSkinMeta(long skin)
    {
        foreach (var skinMeta in SKINS)
        {
            if (skinMeta.id == skin)
                return skinMeta;
        }

        throw new Exception("Invalid skin id " + skin + ".");
    }
    
    public static List<SkinMeta> SKINS = new ()
    {
        new (0, "Default", 0, false),
        new (1, "Cyclop", 250),
        new (2, "Robot", 500),
        new (3, "Rabbit", 300),
        new (4, "Hacker", 400),
        new (5, "Corbac", 100),
        new (6, "Hunter", 450)
    };
    
    public class SkinMeta
    {
        public long id;
        public string name;
        public int price;
        public bool canBuy;
        
        public SkinMeta(long id, string name, int price = 0, bool canBuy = true)
        {
            this.id = id;
            this.name = name;
            this.price = price;
            this.canBuy = canBuy;
        }
    }
    
    // ########################################
    // Trails constants
    // ########################################
    
    public static readonly Dictionary<long, Trail> Trails = new ()
    {
        {
            0, new Trail()
            {
                id = 0,
                name = "Normal",
                color = new Color(1, 1, 0),
                materialName = "DefaultTrail",
                price = 0
            }
        },
        {
            1, new Trail()
            {
                id = 1,
                name = "Double",
                color = new Color(1, 1, 0),
                materialName = "DoubleTrail",
                price = 200
            }
        },
        {
            2, new Trail()
            {
                id = 2,
                name = "USA",
                color = new Color(1, 1, 1),
                materialName = "USATrail",
                price = 1500
            }
        },
        {
            3, new Trail()
            {
                id = 3,
                name = "Flash",
                color = new Color(1, 1, 0),
                materialName = "FlashTrail",
                price = 500
            }
        }
    };
    
    public class Trail
    {

        public int id;
        public string name;
        public Color color;
        public string materialName;
        public int price = -1;
        
        public bool canBuy => price != -1;
        
        [CanBeNull] private Material _material;
        [CanBeNull] private Sprite _icon;
        
        public Material GetMaterial()
        {
            if (_material == null)
                _material = Resources.Load<Material>(materialName);
            
            return _material;
        }
        
        public Sprite GetIcon()
        {
            if (_icon == null)
                _icon = Resources.Load<Sprite>("TrailSprites/" + id);
            
            return _icon;
        }
        
    }
    
    // ########################################
    // Blocks Skins
    // ########################################
    private static Dictionary<int, Sprite> _blockStyleIconCache = new ();

    public static int GetVariants(string blockSkinName)
    {
        switch (blockSkinName)
        {
            case "default":
                return 4;
            case "blocks":
                return 2;
            default:
                return 1;
        }
    }

    public static string GetStyleById(long styleId)
    {
        if (styleId == 0)
            return "default";
        else if (styleId == 1)
            return "bricks";
        else if (styleId == 2)
            return "triangles";
        else if (styleId == 3)
            return "lines";
        else if (styleId == 4)
            return "spikes";
        else if (styleId == 5)
            return "temp_spikes";
        else if (styleId == 6)
            return "blocks";
        
        throw new Exception("Invalid style id " + styleId + ".");
    }
    
    public static Sprite GetBlockStyleIcon(int styleId)
    {
        if (_blockStyleIconCache.TryGetValue(styleId, out var icon))
            return icon;

        var sprite = Resources.Load<Sprite>("blocks/style-" + styleId);
        _blockStyleIconCache.Add(styleId, sprite);
        return sprite;
    }
    
    // ########################################
    // Blocks Colors
    // ########################################

    public static Dictionary<long, BlockColor> COLORS = new()
    {
        {0, new BlockColor(0, Color.magenta)},
        {1, new BlockColor(1, new Color(29f / 255f, 78f / 255f, 216f / 255f))},
        {2, new BlockColor(2, new Color(126f / 255f, 34f / 255f, 206f / 255f))},
        {3, new BlockColor(3, new Color(185f / 255f, 28f / 255f, 28f / 255f))},
        {4, new BlockColor(4, new Color(77f / 255f, 124f / 255f, 15f / 255f))},
    };
    
    public class BlockColor
    {

        public long id;
        public Color color;
        
        public BlockColor(long id, Color color)
        {
            this.id = id;
            this.color = color;
        }
    }
    
    public static BlockColor GetBlockColorById(long colorId)
    {
        if (COLORS.ContainsKey(colorId))
            return COLORS[colorId];
        
        throw new Exception("Invalid color id " + colorId + ".");
    }
    
    // ########################################
    // Upgrades/bonuses
    // ########################################

    public static List<Bonus> BONUSES = new ()
    {
        new Bonus(0, 5, level => 10f + level * 5f,
            "Coins", "Increase the chance to replace dots into coins.", "Coin", 
            level => level * 150, level => (10f + level * 5f) + "%"),
        new Bonus(1, 10, level => 5f + level,
            "Shield Duration", "Increase the duration of the shield.", "Shield",
            level => level * 300, level => (5f + level) + "s"),
    };
    
    public class Bonus
    {
        
        public int id;
        public int maxLevel;
        public Func<int, float> getValue;
        public Func<int, int> getPrice;
        public Func<int, string> getLevelText;

        public string name;
        public string description;
        public string animation;
        
        public int GetPrice(int level)
        {
            return getPrice?.Invoke(level) ?? 0;
        }

        public float GetValue(int level)
        {
            return getValue(level);
        }
        
        public string GetLevelText(int level)
        {
            return getLevelText(level);
        }
        
        public Bonus(int id, int maxLevel, Func<int, float> getValue, string name, string description, string animation,
            Func<int, int> getPrice = null, Func<int, string> getLevelText = null)
        {
            this.id = id;
            this.maxLevel = maxLevel;
            this.getValue = getValue;
            this.name = name;
            this.description = description;
            this.animation = animation;
            this.getPrice = getPrice;
            this.getLevelText = getLevelText;
        }
    }
    
    public static Bonus GetBonusById(int bonusId)
    {
        foreach (var bonus in BONUSES)
        {
            if (bonus.id == bonusId)
                return bonus;
        }

        throw new Exception("Invalid bonus id " + bonusId + ".");
    }

    public static float GetBonusValue(int bonusId)
    {
        return BONUSES[bonusId].GetValue(PlayerStatsManager.GetBonusLevel(bonusId));
    }

    public static Vector2Int DirectionToVector2Int(int direction)
    {
        switch (direction)
        {
            case 0:
                return new Vector2Int(0, 1);
            case 1:
                return new Vector2Int(1, 0);
            case 2:
                return new Vector2Int(0, -1);
            case 3:
                return new Vector2Int(-1, 0);
            default:
                throw new Exception("Invalid direction " + direction + ".");
        }
    }
}