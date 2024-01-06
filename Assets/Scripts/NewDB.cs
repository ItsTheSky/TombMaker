using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

public static class NewDB
{
    
    public static readonly string LocalHost = "http://localhost:8070/";
    public static readonly string RemoteHost = "https://itsthesky.net/tombmakerapi-v1/";
    
    public static string LoggedInToken;
    
    public static string _databaseURL = RemoteHost;
    private static readonly HttpClient Client = new ();
    
    private static int _cachedLoggedInUserId = -1;
    
    public static async Task<int> GetLoggedInUserId()
    {
        if (_cachedLoggedInUserId != -1)
            return _cachedLoggedInUserId;
        
        var response = await GetRequest("users/id", true);
        if (response.status == "ok")
        {
            _cachedLoggedInUserId = int.Parse(response.data["id"].ToString());
            return _cachedLoggedInUserId;
        }

        return -1;
    }

    #region Accounts Endpoints
    
    public static async Task<Response> Login(string username, string password)
    {
        var response = await GetRequest("accounts/login", false, new Dictionary<string, string>()
        {
            {"username", username},
            {"password", password}
        });

        
        if (response is { status: "ok" } success)
        {
            LoggedInToken = success.data["token"].ToString();
            PlayerPrefs.SetString("token", LoggedInToken);
        }

        return response;
    }
    
    public static async Task<Response> Register(string username, string password)
    {
        var response = await PostRequest("accounts/register", false, new Dictionary<string, string>()
        {
            {"username", username},
            {"password", password}
        });

        
        if (response is { status: "ok" })
        {
            var user = response.Get<User>("user");
            Debug.Log(user.username);
        }

        return response;
    }
    
    public static async Task<Response> GetPersonalsInfos()
    {
        return await GetRequest("users/personal_infos", true);
    }

    public static async Task<bool> IsAdmin()
    {
        var response = await GetRequest("users/admin", true);
        if (response.status == "ok")
        {
            var isAdmin = bool.Parse(response.data["admin"].ToString());
            return isAdmin;
        }
        
        return false;
    }

    public static async void SaveUserProgress()
    {
        if (!IsLoggedIn())
            return;
        
        var save = new UserDataSave()
        {
            data = GlobalIOManager.StatsData,
            levels = DataManager.GetLocalLevels().ConvertAll(cli => new CompressedCustomLevelInfo(cli)),
            achievements = Achievements.GetData()
        };
        var json = JsonConvert.SerializeObject(save);
        var compressed = Utilities.NewCompress(json);
        
        var response = await PostRequest("accounts/save_progress", true, 
            new Dictionary<string, string>(), compressed);
        
        if (response.status != "ok")
            Debug.Log(response.message);
    }
    
    public static async void LoadUserProgress(Action<Response> action = null)
    {
        if (!IsLoggedIn())
            return;
        var response = await GetRequest("accounts/load_progress", true);
        
        if (response.status == "ok")
        {
            var json = Utilities.NewDecompress(response.data["data"].ToString());
            var save = JsonConvert.DeserializeObject<UserDataSave>(json);
            
            PlayerStatsManager.SetData(save.data);
            DataManager.SetLocalLevels(save.levels.ConvertAll(cli => new DataManager.CustomLevelInfo(cli)));
            Achievements.SetData(save.achievements);
            
            action?.Invoke(response);
        }
        else
        {
            Debug.Log(response.message);
        }
    }

    #endregion

    #region Levels Endpoints

    public static async Task<Response> PublishLevel(DataManager.CustomLevelInfo cli)
    {
        var compressed = cli.saveData.Compress();
        var response = await PostRequest("levels/publish", true, new Dictionary<string, string>()
        {
            {"name", cli.name},
            {"description", cli.description},
            {"stars", cli.saveData.CountBlockType(4).ToString()},
            {"colored_blocks", cli.coloredBlocksCount.ToString()},
            {"level_type", cli.saveData.GetLevelType().ToString()},
            {"allow_clone", cli.onlineSettings.allowClone.ToString()},
            {"allow_comments", cli.onlineSettings.allowComments.ToString()}
        }, compressed);

        if (response.status == "ok")
        {
            var level = response.Get<Level>("level");
            cli.publishId = level.id;
            cli.published = true;
            DataManager.SetLocalLevel(cli);
        }

        return response;
    }
    
    public static async Task<Response> GetLevelInfos(int levelId)
    {
        return await GetRequest("levels/get", false, new Dictionary<string, string>()
        {
            {"id", levelId.ToString()}
        });
    }

    public static async Task<bool> IsLevelStillPublished(int levelId)
    {
        return (await GetLevelInfos(levelId)).status == "ok";
    }
    
    public static async Task<Response> UpdateLevel(DataManager.CustomLevelInfo cli)
    {
        var compressed = cli.saveData.Compress();
        var response = await PostRequest("levels/update", true, new Dictionary<string, string>()
        {
            {"id", cli.publishId.ToString()},
            {"name", cli.name},
            {"description", cli.description},
            {"stars", cli.saveData.CountBlockType(4).ToString()},
            {"colored_blocks", cli.coloredBlocksCount.ToString()},
            {"level_type", cli.saveData.GetLevelType().ToString()},
            {"allow_clone", cli.onlineSettings.allowClone.ToString()},
            {"allow_comments", cli.onlineSettings.allowComments.ToString()}
        }, compressed);

        if (response.status == "ok")
        {
            var level = response.Get<Level>("level");
            cli.publishId = level.id;
            cli.published = true;
            DataManager.SetLocalLevel(cli);
        }

        return response;
    }
    
    public static async Task<Response> DeleteLevel(int levelId)
    {
        return await PostRequest("levels/delete", true, new Dictionary<string, string>()
        {
            {"id", levelId.ToString()}
        });
    }

    public static async Task<Response> SearchLevels(SearchController.SearchData searchData, int page = 1)
    {
        var json = JsonConvert.SerializeObject(searchData);
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        
        return await GetRequest("levels/search", false, new Dictionary<string, string>()
        {
            {"search", base64},
            {"page", page.ToString()}
        });
    }
    
    public static async Task<Response> DownloadLevel(int levelId)
    {
        return await GetRequest("levels/download", false, new Dictionary<string, string>()
        {
            {"id", levelId.ToString()}
        });
    }
    
    public static async Task<Response> VerifyLevel(int levelId)
    {
        return await PostRequest("levels/verify", true, new Dictionary<string, string>()
        {
            {"id", levelId.ToString()}
        });
    }

    public static async Task<Response> GetLeaderboard(int levelId, int page)
    {
        return await GetRequest("levels/leaderboard", false, new Dictionary<string, string>()
        {
            {"id", levelId.ToString()},
            {"page", page.ToString()}
        });
    }
    
    public static async Task<Response> GetLevelComments(int levelId, int page)
    {
        return await GetRequest("levels/comments", false, new Dictionary<string, string>()
        {
            {"id", levelId.ToString()},
            {"page", page.ToString()}
        });
    }
    
    public static async Task<Response> PostComment(int levelId, string comment)
    {
        return await PostRequest("levels/comment", true, new Dictionary<string, string>()
        {
            {"id", levelId.ToString()},
            {"comment", comment}
        });
    }
    
    public static async Task<Response> PinLevelComment(int commendId)
    {
        return await PostRequest("levels/comment/pin", true, new Dictionary<string, string>()
        {
            {"id", commendId.ToString()}
        });
    }
    
    public static async Task<Response> DeleteLevelComment(int commendId)
    {
        return await PostRequest("levels/comment/delete", true, new Dictionary<string, string>()
        {
            {"id", commendId.ToString()}
        });
    }

    #endregion

    #region Level Likes Endpoints

    public static async Task<Response> HasLiked(int levelId)
    {
        return await GetRequest("levels/likes/hasLiked", true, new Dictionary<string, string>()
        {
            {"levelId", levelId.ToString()}
        });
    }
    
    public static async Task<Response> LikeLevel(int levelId, bool like)
    {
        var endpoint = like ? "levels/likes/like" : "levels/likes/dislike";
        return await PostRequest(endpoint, true, new Dictionary<string, string>()
        {
            {"levelId", levelId.ToString()}
        });
    }

    #endregion

    #region Users Endpoints

    public static async Task<Response> RetrieveUserList(int page, int sortingType)
    {
        return await GetRequest("users/list", false, new Dictionary<string, string>()
        {
            {"page", page.ToString()},
            {"type", sortingType.ToString()}
        });
    }

    #endregion

    #region Global post/get

    private static bool _hasHeaders;
    public static async Task<Response> PostRequest(string endpoint, bool requireLogin,
        Dictionary<string, string> parameters = null, string body = null, int timeout = 2500)
    {
        parameters ??= new Dictionary<string, string>();
        
        if (requireLogin && !IsLoggedIn())
            return new NotLoggedInResponse();
        
        // build parameters, as first parameter is preceded by a ?
        foreach (var (key, value) in parameters)
        {
            endpoint += (endpoint.Contains("?") ? "&" : "?") + key + "=" + value;
        }
        
        var request = _databaseURL + endpoint;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log("POST: " + request);
#endif
        if (!_hasHeaders)
        {
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _hasHeaders = true;
        }
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", LoggedInToken);

        var responseTask = Client.PostAsync(request, new StringContent(body ?? "{}"));

        if (await Task.WhenAny(responseTask, Task.Delay(timeout)) == responseTask) {
            var content = await responseTask.Result.Content.ReadAsStringAsync();
            
            var jsonResponse = JsonConvert.DeserializeObject<Response>(content);
        
            if (jsonResponse is Response { status: "error" } response)
            {
                if (response.message == "invalid_token")
                    Logout();

                return new Response { status = "error", message = response.message };
            }
        
            return jsonResponse as Response;
            
        }
        
        return new TimeoutResponse();
    }
    
    public static async Task<Response> GetRequest(string endpoint, bool requireLogin,
        Dictionary<string, string> parameters = null, int timeout = 2500)
    {
        parameters ??= new Dictionary<string, string>();
        
        if (requireLogin && !IsLoggedIn())
            return new NotLoggedInResponse();

        if (!_hasHeaders)
        {
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _hasHeaders = true;
        }
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", LoggedInToken);
        
        // build parameters, as first parameter is preceded by a ?
        foreach (var (key, value) in parameters)
        {
            endpoint += (endpoint.Contains("?") ? "&" : "?") + key + "=" + value;
        }
        
        var request = _databaseURL + endpoint;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log("GET: " + request);
#endif

        var responseTask = Client.GetAsync(request);
        if (await Task.WhenAny(responseTask, Task.Delay(timeout)) == responseTask) {
            var content = await responseTask.Result.Content.ReadAsStringAsync();
            
            var jsonResponse = JsonConvert.DeserializeObject<Response>(content);
        
            if (jsonResponse is { status: "error" } response)
            {
                if (response.message == "invalid_token")
                    Logout();

                return new Response { status = "error", message = response.message };
            }
        
            return jsonResponse ?? new TimeoutResponse();
        }
        
        return new TimeoutResponse();
    }
    
    public static bool IsLoggedIn()
    {
        if (LoggedInToken == null && PlayerPrefs.HasKey("token"))
            LoggedInToken = PlayerPrefs.GetString("token");
        
        return LoggedInToken != null;
    }
    
    public static void Logout()
    {
        LoggedInToken = null;
        PlayerPrefs.DeleteKey("token");
    }

    #endregion

    #region Response Classes

    [Serializable]
    public class Response
    {

        public string message;
        public string status;

        public Dictionary<string, object> data;
        
        public T Get<T>(string key)
        {
            if (data == null || !data.ContainsKey(key))
                return default;
            
            return JsonConvert.DeserializeObject<T>(data[key].ToString());
        }
        
        public List<T> GetList<T>(string key)
        {
            if (data == null || !data.ContainsKey(key))
                return default;
            
            return JsonConvert.DeserializeObject<List<T>>(data[key].ToString());
        }

        public bool Success()
        {
            return status == "ok";
        }
    }
    
    public class TimeoutResponse : Response
    {
        public TimeoutResponse()
        {
            status = "error";
            message = "Cannot connect to the server.";
        }
    }
    
    public class NotLoggedInResponse : Response
    {
        public NotLoggedInResponse()
        {
            status = "error";
            message = "You are not logged in.";
        }
    }

    #endregion

    #region Entities

    [Serializable]
    public class User
    {

        public int id;
        public string username;
        public long creationDate;
        public bool admin;

    }
    
    [Serializable]
    public class UserWithStars
    {
        
        public User user;
        public int officialStars;
        public int unofficialStars;
        
    }
    
    [Serializable]
    public class UserWithPublishedLevels
    {
        
        public User user;
        public int publishedLevels;
        
    }
    
    [Serializable]
    public class UserWithProgressLevels
    {
            
        public User user;
        public int userLevel;
        public int currentExperience;
            
    }
    
    [Serializable]
    public class Level
    {
        
        public int id;
        public int userId;
        public string name;
        public string author;
        public string description;
        public bool verified;
        public int version;
        [FormerlySerializedAs("coloredBlocksCount")] public int coloredBlocks;
        public int levelType;
        
        public LevelType GetLevelType() => (LevelType) levelType;
    }
    
    [Serializable]
    public class LevelOnlineSettings
    {

        public bool allowClone = true;
        public bool allowComments = true;
        
    }
    
    [Serializable]
    public class LevelStats
    {

        public int levelId;
        public int likes;
        public int dislikes;
        public int downloads;
        
    }
    
    [Serializable]
    public class LevelAndStats
    {
     
        public Level level;
        public LevelStats stats;
        public LevelOnlineSettings onlineSettings;
        
    }
    
    [Serializable]
    public class LevelComment
    {
        
        public int id;
        public int levelId;
        public int userId;
        
        public string comment;
        public string username;
        public int likes;
        public int dislikes;
        public long date;
        public bool pinned;
        
    }
    
    [Serializable]
    public class Leaderboard
    {

        public int id;
        public int userId;
        public int stars;
        public int dots;
        public int deaths;
        public string username;
        
    }
    
    [Serializable]
    public class LevelData
    {
        
        public int levelId;
        public string compressedData;
        public int stars;
        
    }
    
    [Serializable]
    public class Pagination
    {
        
        public int page;
        public int pages;
        
        public int size;
        public int total;

        public bool hasNext;
        public bool hasPrevious;

    }
    
    [Serializable]
    public class UserDataSave
    {
        
        public PlayerStatsManager.StatsData data;
        public List<CompressedCustomLevelInfo> levels;
        public Achievements.AchievementData achievements;
        
    }
    
    [Serializable]
    public class CompressedCustomLevelInfo
    {
        public int id;
        public string name;
        public string description;
        public bool completed; // if the current version of the level is completed, so can be published
        public bool published; // if the level is published
        public int publishId; // the id of the level on the server (if published), else -1
        public string saveData; // compressed save data
        public long lastModified = DateTime.Now.Ticks; // the last modified date of the level
        public long creationDate = DateTime.Now.Ticks; // the creation date of the level
        public bool verified; // if the level is verified by the user, and thus can be published
        public int coloredBlocksCount;
        public LevelOnlineSettings onlineSettings; // the online settings of the level
        
        public CompressedCustomLevelInfo(DataManager.CustomLevelInfo cli)
        {
            id = cli.id;
            name = cli.name;
            description = cli.description;
            completed = cli.completed;
            published = cli.published;
            publishId = cli.publishId;
            saveData = cli.saveData.Compress();
            lastModified = cli.lastModified;
            creationDate = cli.creationDate;
            verified = cli.verified;
            coloredBlocksCount = cli.coloredBlocksCount;
            onlineSettings = cli.onlineSettings ?? new LevelOnlineSettings();
        }

        public CompressedCustomLevelInfo()
        {
            
        }
    }
    
    [Serializable]
    public class ExtendedUserInfos
    {

        public int completedLevels;
        public int publishedLevels;

    }
    
    #endregion
}