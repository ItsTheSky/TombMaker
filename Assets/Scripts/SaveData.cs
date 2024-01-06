using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SaveData
{
    /**
     * Version history:
     * 1: Initial version
     * 2: Added level settings outside the spawn block in a separated JSON object
     */
    public const int LevelVersion = 2;
    
    public int version;
    public List<BlockData> Blocks;
    public LevelSettings settings;

    public SaveData(LogicScript logic)
    {
        Blocks = new List<BlockData>();
        settings = logic.levelSettings;
        
        foreach (var layer in logic.layers)
            foreach (var block in layer.Value)
                if (block.Value != null && !block.Value.IsDestroyed()) 
                    Blocks.Add(new BlockData(layer.Key, block.Value.GetComponent<Block>()));
        
        version = LevelVersion;
    }

    public SaveData()
    {
        //Blocks = new List<BlockData> { BlockData.CreateSpawnBlockData() };
        Blocks = new List<BlockData>();
        version = LevelVersion;
    }
    
    public Dictionary<int, Dictionary<Vector2, GameObject>> GetLayers(LogicScript logic)
    {
        var layers = new Dictionary<int, Dictionary<Vector2, GameObject>>();
        foreach (var block in Blocks)
        {
            if (!layers.ContainsKey(block.layer))
                layers.Add(block.layer, new Dictionary<Vector2, GameObject>());
            
            var blockObject = logic.CreateBlock(block.blockType);
            if (blockObject == null)
                continue;
            blockObject.transform.position = new Vector3(block.x, block.y, 0);
            var blockScript = blockObject.GetComponent<Block>();
            
            blockScript.LoadSettings(block.settings ?? new Dictionary<string, object>());
            blockScript.Init();

            blockScript.ready = true;
            layers[block.layer].Add(new Vector2(block.x, block.y), blockObject);
        }
        
        if (layers.Count == 0)
            layers.Add(0, new Dictionary<Vector2, GameObject>());

        return layers;
    }

    public string Compress()
    {
        return Utilities.NewCompress(JsonConvert.SerializeObject(this));
    }

    public LevelSettings GetSettings(LogicScript logic)
    {
        if (version == 1 && settings == null && logic != null) // old levels, using the spawn block to store settings
        {
            var spawn = logic.GetSpawnScript();
            if (spawn == null)
                throw new Exception("No spawn block found");
            settings = new LevelSettings(spawn);
            Debug.Log("Loaded level settings from spawn block as it was not stored in the level file.");
        }

        return settings ??= new LevelSettings();
    }

    public int GetLevelType()
    {
        return settings == null ? 0 : (int) settings.levelType;
    }
    
    [Serializable]
    public class BlockData
    {

        public int layer;
        public float x;
        public float y;
        public int blockType;
        public Dictionary<string, object> settings;
        public bool uniqueLayer;

        public BlockData(int layer, Block block)
        {
            this.layer = layer;
            
            x = block.GetPos().x;
            y = block.GetPos().y;
            
            blockType = block.GetBlockType();
            settings = block.SaveSettings();
            
            uniqueLayer = block.uniqueLayer;
        }

        public static BlockData CreateSpawnBlockData()
        {
            var blockData = new BlockData
            {
                layer = 0,
                x = 0,
                y = 0,
                blockType = 0,
                settings = new Dictionary<string, object>(),
                uniqueLayer = true
            };
            return blockData;
        }
        
        public BlockData()
        {
            
        }

        // Convert to a custom file format: properties are separated by ; and key-value pairs are separated by ,
        public string AsString()
        {
            var str = "";
            str += layer + ";";
            str += x + ";";
            str += y + ";";
            str += blockType + ";";
            str += uniqueLayer + ";";
            foreach (var setting in settings)
            {
                str += setting.Key + "," + setting.Value + ";";
            }

            return str;
        }

        public static BlockData FromString(string str)
        {
            var blockData = new BlockData();
            var split = str.Split(';');
            if (split.Length < 5)
                return null;

            blockData.layer = int.Parse(split[0]);
            blockData.x = float.Parse(split[1]);
            blockData.y = float.Parse(split[2]);
            blockData.blockType = int.Parse(split[3]);
            blockData.uniqueLayer = bool.Parse(split[4]);
            blockData.settings = new Dictionary<string, object>();
            for (var i = 5; i < split.Length; i++)
            {
                var setting = split[i].Split(',');
                blockData.settings.Add(setting[0], setting[1]);
            }

            return blockData;
        }
    }
    
    [Serializable]
    public abstract class BlockSettings
    {

    }
    
    [Serializable]
    public class EmptyBlockSettings : BlockSettings
    {
        
    }

    public bool HasSpawnPoint()
    {
        return CountBlockType(3) > 0;
    }
    
    public int CountBlockType(int blockType)
    {
        var count = 0;
        foreach (var block in Blocks)
        {
            if (block.blockType == blockType && (!block.uniqueLayer || block.layer == 0))
                count++;
        }

        return count;
    }

    public bool IsValid()
    {
        var colored = settings is { levelType: LevelType.Color };
        return CountBlockType(3) == 1 && (colored || CountBlockType(6) == 1);
    }

    public Vector2Int GetSpawnPos()
    {
        foreach (var block in Blocks)
            if (block.blockType == 3 && (!block.uniqueLayer || block.layer == 0))
                return new Vector2Int((int) block.x, (int) block.y);
        
        return new Vector2Int(0, 0);
    }
    
    [Serializable]
    public class InternalSaveData
    {

        public int version;
        public string[] Blocks;

    }
    
    /* public class BlockDataConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var blockData = (BlockData) value;
            writer.WriteValue(blockData.AsString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var blockData = BlockData.FromString((string) reader.Value);
            return blockData;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BlockData);
        }
    } */
    
    [Serializable]
    public class LevelSettings
    {

        public long spawnRotation;
        public bool stickyCamera;
        public long cameraZoom;
        public double cameraX = 0;
        public double cameraY = 0;
        public bool unlockedX = false;
        public bool unlockedY = false;
        public bool showDoor = false;
        public long style = 0;
        public long color = 0;
        public long playerSpeed;
        public LevelType levelType;

        public LevelSettings()
        {
            cameraZoom = 5;
            stickyCamera = true;
            spawnRotation = 1;
            style = 0;
            color = 0;
            playerSpeed = PlayerScript.DefaultPlayerSpeed;
            levelType = LevelType.Normal;
        }

        public LevelSettings(SpawnBlock oldVersion)
        {
            spawnRotation = oldVersion.spawnRotation;
            stickyCamera = oldVersion.stickyCamera;
            cameraZoom = oldVersion.cameraZoom;
            cameraX = oldVersion.cameraX;
            cameraY = oldVersion.cameraY;
            unlockedX = oldVersion.unlockedX;
            unlockedY = oldVersion.unlockedY;
            showDoor = oldVersion.showDoor;
            style = oldVersion.style;
            color = oldVersion.color;
            
            playerSpeed = PlayerScript.DefaultPlayerSpeed;
            levelType = LevelType.Normal;
        }

        public string GetStyle()
        {
            return Constants.GetStyleById(style);
        }
        
        public Color GetColor()
        {
            return Constants.GetBlockColorById(color).color;
        }
    }
}