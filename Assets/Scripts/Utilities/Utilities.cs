using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

public static class Utilities
{
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);
    
    public struct POINT
    {
        public int X;
        public int Y;
    }

    public static POINT GetCenterPosition()
    {
        var res = new POINT();
        res.X = Screen.width / 2;
        res.Y = Screen.height / 2;
        return res;
    }

    public static string[] GetAvailableResolutionsAsString()
    {
        var resolutions = GetAvailableResolutions();
        var resList = new List<string>();
        
        foreach (var res in resolutions)
        {
            resList.Add(res.width + "x" + res.height);
        }
        
        return resList.ToArray();
    }
    
    public static Resolution[] GetAvailableResolutions()
    {
        var resolutions = Screen.resolutions;
        var resList = new List<Resolution>();
        
        foreach (var res in resolutions)
        {
            if (res.width / res.height == 16 / 9)
            {
                for (int i = 0; i < resList.Count; i++)
                {
                    if (resList[i].width == res.width && resList[i].height == res.height)
                    {
                        resList.RemoveAt(i);
                        break;
                    }
                }
                
                resList.Add(res);
            }
        }
        
        return resList.ToArray();
    }
    
    public static Resolution GetResolutionByIndex(int index)
    {
        var resolutions = GetAvailableResolutions();
        if (index < 0 || index >= resolutions.Length)
            return GetBestResolution();
        return resolutions[index];
    }
    
    public static Resolution GetBestResolution()
    {
        var resolutions = GetAvailableResolutions();
        
        var best = resolutions[^1];
        
        foreach (var res in resolutions)
        {
            var fps = res.refreshRateRatio.value;
            if (fps > best.refreshRateRatio.value)
                best = res;
        }
        
        return best;
    }

    public static bool IsDesktopPlatform()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer
               || Application.platform == RuntimePlatform.WindowsEditor
               || Application.platform == RuntimePlatform.LinuxPlayer
               || Application.platform == RuntimePlatform.LinuxEditor;

    }
    
    public static string NewCompress(string value)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(value);
        using var ms = new MemoryStream();
        using (var sw = new GZipStream(ms, CompressionMode.Compress))
        {
            sw.Write(byteArray, 0, byteArray.Length);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string NewDecompress(string value)
    {
        byte[] byteArray = Convert.FromBase64String(value);
        using var ms = new MemoryStream(byteArray);
        using var sr = new GZipStream(ms, CompressionMode.Decompress);
        using var reader = new StreamReader(sr);
        return reader.ReadToEnd();
    }
    
    public static string NewDecompress(byte[] value)
    {
        using var ms = new MemoryStream(value);
        using var sr = new GZipStream(ms, CompressionMode.Decompress);
        using var reader = new StreamReader(sr);
        return reader.ReadToEnd();
    }
    
    public static string Decompress(string input)
    {
        byte[] compressed = Convert.FromBase64String(input);
        byte[] decompressed = Decompress(compressed);
        return Encoding.UTF8.GetString(decompressed);
    }

    public static string Compress(string input)
    {
        byte[] encoded = Encoding.UTF8.GetBytes(input);
        byte[] compressed = Compress(encoded);
        return Convert.ToBase64String(compressed);
    }
    
    public static byte[] Decompress(byte[] input)
    {
        using (var source = new MemoryStream(input))
        {
            byte[] lengthBytes = new byte[4];
            source.Read(lengthBytes, 0, 4);

            var length = BitConverter.ToInt32(lengthBytes, 0);
            using (var decompressionStream = new GZipStream(source,
                       CompressionMode.Decompress))
            {
                var result = new byte[length];
                decompressionStream.Read(result, 0, length);
                return result;
            }
        }
    }

    public static byte[] Compress(byte[] input)
    {
        using (var result = new MemoryStream())
        {
            var lengthBytes = BitConverter.GetBytes(input.Length);
            result.Write(lengthBytes, 0, 4);

            using (var compressionStream = new GZipStream(result,
                       CompressionMode.Compress))
            {
                compressionStream.Write(input, 0, input.Length);
                compressionStream.Flush();

            }
            return result.ToArray();
        }
    }

    public static string XOREncryptDecrypt(string data, int key = 16334)
    {
        StringBuilder input = new StringBuilder(data);
        StringBuilder output = new StringBuilder(data.Length);

        for (int i = 0; i < data.Length; i++)
        {
            var character = input[i];
            character = (char) (character ^ key);
            output.Append(character);
        }

        return output.ToString();
    }

    /**
     * <summary>
     * Saves all data to the disk, should be called when the game is closde or the player finish a level.
     * </summary>
     * <returns></returns>
     */
    public static void SaveAll()
    {
        Achievements.SaveAchievements();
    }

    #region GameObject Modifications

    public static Block GetBlock(this GameObject obj)
    {
        return obj.GetComponent<Block>();
    }
    
    public static bool IsBlock(this GameObject obj, int blockId)
    {
        var block = obj.GetBlock();
        if (block == null)
            return false;
        
        return block.blockType == blockId;
    }

    #endregion
}