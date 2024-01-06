using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

static class DatabaseManager
{
    public static readonly string LocalHost = "http://localhost:8070/";
    public static readonly string RemoteHost = "https://itsthesky.net/tombmakerapi-v1/";
    
    public static string LoggedInToken;
    
    public static string _databaseURL = RemoteHost;
    private static readonly HttpClient Client = new ();

    public static async Task<Response> Login(string name, string password)
    {
        var response = await GetRequest<LoginResponse>("login", false, 
            parameters: new Dictionary<string, string>()
            {
                {"username", name},
                {"password", password}
            });
        
        if (response is LoginResponse { token: not null } loginResponse)
        {
            LoggedInToken = loginResponse.token;
            PlayerPrefs.SetString("token", LoggedInToken);
        }
        
        return response ?? new TimeoutResponse();
    }
    
    public static async Task<Response> Register(string name, string password)
    {
       var response = await PostRequest<LoginResponse>("register", false, 
           parameters: new Dictionary<string, string>()
           {
               {"username", name},
               {"password", password}
           });
       
       return response ?? new TimeoutResponse();
    }

    public static async Task<Response> GetLevelComments(int levelId, int page,
        int userId = -1)
    {
        var response = await GetRequest<LevelCommentsResponse>("getLevelComments", false, 
            parameters: new Dictionary<string, string>()
            {
                {"levelId", levelId.ToString()},
                {"page", page.ToString()},
                {"userId", userId.ToString()}
            });
        
        return response ?? new TimeoutResponse();
    }

    public static async Task<Response> PostLevelComment(int levelId, string comment)
    {
        var response = await PostRequest<Response>("postLevelComment", true,
            parameters: new Dictionary<string, string>()
            {
                {"levelId", levelId.ToString()}
            }, body: comment);
        
        return response ?? new TimeoutResponse();
    }

    public static async Task<Response> PinLevelComment(int commentId, bool pin)
    {
        var response = await PostRequest<Response>("pinLevelComment", true,
            parameters: new Dictionary<string, string>()
            {
                {"commentId", commentId.ToString()},
                {"pin", pin.ToString()}
            });
        
        return response ?? new TimeoutResponse();
    }

    public static async Task<Response> PublishLevel(DataManager.CustomLevelInfo cli)
    {
        var json = JsonConvert.SerializeObject(cli.saveData);
        var compressed = Utilities.NewCompress(json);
        
        var response = await PostRequest<LevelUploadResponse>("addLevel", true,
            parameters: new Dictionary<string, string>()
            {
                {"levelName", cli.name},
                {"levelDescription", cli.description}
            }, body: compressed);
        
        return response ?? new TimeoutResponse();
    }

    public static async Task<Response> UpdateLevel(DataManager.CustomLevelInfo cli)
    {
        if (!cli.published)
            return new Response() { status = "error", message = "Level is not published." };
        
        var json = JsonConvert.SerializeObject(cli.saveData);
        var compressed = Utilities.NewCompress(json);
        
        var response = await PostRequest<Response>("updateLevel", true,
            parameters: new Dictionary<string, string>()
            {
                {"levelName", cli.name},
                {"levelDescription", cli.description},
                {"levelId", cli.publishId.ToString()}
            }, body: compressed);
        
        return response ?? new TimeoutResponse();
    }

    public static async Task<Response> PostRequest<T>(string endpoint, bool requireLogin,
        Dictionary<string, string> parameters = null, string body = null, int timeout = 2500)
    {
        parameters ??= new Dictionary<string, string>();
        
        if (requireLogin && !IsLoggedIn())
            return new NotLoggedInResponse();
        
        if (requireLogin)
            parameters.Add("token", LoggedInToken);
        
        // build parameters, as first parameter is preceded by a ?
        foreach (var (key, value) in parameters)
        {
            endpoint += (endpoint.Contains("?") ? "&" : "?") + key + "=" + value;
        }
        
        var request = _databaseURL + endpoint;

        var responseTask = Client.PostAsync(request, new StringContent(body ?? "{}"));
        if (await Task.WhenAny(responseTask, Task.Delay(timeout)) == responseTask) {
            var content = await responseTask.Result.Content.ReadAsStringAsync();
            
            var jsonResponse = JsonConvert.DeserializeObject<T>(content);
        
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
    
    public static async Task<Response> GetRequest<T>(string endpoint, bool requireLogin,
        Dictionary<string, string> parameters = null, int timeout = 2500)
    {
        parameters ??= new Dictionary<string, string>();
        
        if (requireLogin && !IsLoggedIn())
            return new NotLoggedInResponse();
        
        if (requireLogin)
            parameters.Add("token", LoggedInToken);
        
        // build parameters, as first parameter is preceded by a ?
        foreach (var (key, value) in parameters)
        {
            endpoint += (endpoint.Contains("?") ? "&" : "?") + key + "=" + value;
        }
        
        var request = _databaseURL + endpoint;
        Debug.Log(request);

        var responseTask = Client.GetAsync(request);
        if (await Task.WhenAny(responseTask, Task.Delay(timeout)) == responseTask) {
            var content = await responseTask.Result.Content.ReadAsStringAsync();
            
            var jsonResponse = JsonConvert.DeserializeObject<T>(content);
        
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

    private static PersonalAccountInfosResponse _lastCachedInfos;
    public static async Task<PersonalAccountInfosResponse> GetLastCachedInfos()
    {
        if (!IsLoggedIn())
            return null;

        var response = await getPersonalInfos();
        if (response is TimeoutResponse)
            return _lastCachedInfos;
        
        _lastCachedInfos = response as PersonalAccountInfosResponse;
        return _lastCachedInfos;
    }
    
    public static async Task<Response> getPersonalInfos()
    {
        var response = await GetRequest<PersonalAccountInfosResponse>("getPersonalInfos", true);
        
        return response ?? new TimeoutResponse();
    }

    public static async Task<bool> IsLevelPublished(int id)
    {
        var response = await GetRequest<IsLevelPublishedResponse>("isLevelPublished", false,
            parameters: new Dictionary<string, string>()
            {
                {"levelId", id.ToString()}
            });
        
        var published = response as IsLevelPublishedResponse;
        return published?.published ?? false;
    }
    
    public static async Task<LevelDownloadResponse> DownloadLevel(int levelId)
    {
        var request = _databaseURL + "downloadLevel?levelId=" + levelId;
        Debug.Log(request);
        
        var response = await Client.GetAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonResponse = JsonConvert.DeserializeObject<LevelDownloadResponse>(content);
        return jsonResponse;
    }

    public static async Task<String> LikeLevel(int levelId, bool like)
    {
        if (!IsLoggedIn())
            return "not_logged_in";
        
        var request = _databaseURL + "likeLevel?token=" + LoggedInToken + "&levelId=" + levelId + "&like=" + like;
        
        var response = await Client.PostAsync(request, null);
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonResponse = JsonConvert.DeserializeObject<Response>(content);
        return jsonResponse.message;
    }

    public static async void DeleteLevel(int levelId)
    {
        if (!IsLoggedIn())
            return;
        
        var request = _databaseURL + "deleteLevel?token=" + LoggedInToken + "&levelId=" + levelId;
        
        var response = await Client.DeleteAsync(request,  CancellationToken.None);
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonResponse = JsonConvert.DeserializeObject<Response>(content);
        if (jsonResponse.status == "error")
            Debug.LogWarning("Error deleting level: " + jsonResponse.message);
    }
    
    public static async void VerifyLevel(int levelId)
    {
        if (!IsLoggedIn())
            return;
        
        var request = _databaseURL + "verifyLevel?token=" + LoggedInToken + "&levelId=" + levelId;
        
        var response = await Client.PostAsync(request,  null);
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonResponse = JsonConvert.DeserializeObject<Response>(content);
        if (jsonResponse.status == "error")
            Debug.LogWarning("Error verifying level: " + jsonResponse.message);
    }

    private static int _cachedEventSkin;
    private static bool _cachedEventSkinLoaded;

    public static async void CacheEventSkin()
    {
        if (_cachedEventSkinLoaded)
            return;
        
        _cachedEventSkin = await GetEventSkin();
        _cachedEventSkinLoaded = true;
    }
    
    public static int GetCachedEventSkin()
    {
        if (!_cachedEventSkinLoaded)
            CacheEventSkin();
        
        return _cachedEventSkin;
    }
    
    public static async Task<int> GetEventSkin()
    {
        var request = _databaseURL + "eventSkin";

        try
        {
            var response = await Client.GetAsync(request);
            var content = await response.Content.ReadAsStringAsync();
        
            var jsonResponse = JsonConvert.DeserializeObject<EventSkinResponse>(content);
            if (jsonResponse.status == "error")
                return -1;
        
            return jsonResponse.skinId;
        }
        catch (SocketException e)
        {
            Debug.Log("Could not connect to the server.");
            return -1;
        }
    }
    
    public static async void SetEventSkin(int skinId)
    {
        if (!await IsAdmin())
            return;
        
        var request = _databaseURL + "updateEventSkin?token=" + LoggedInToken + "&skinId=" + skinId;
        
        var response = await Client.PostAsync(request,  null);
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonResponse = JsonConvert.DeserializeObject<Response>(content);
        if (jsonResponse.status == "error")
            Debug.LogWarning("Error setting event skin: " + jsonResponse.message);
        
        _cachedEventSkin = skinId;
    }

    public static async void SaveUserProgress()
    {
        if (!IsLoggedIn())
            return;
        
        var request = _databaseURL + "updateUserProgress?token=" + LoggedInToken;
        
        var progress = new LevelProgressArray {
            levels = PlayerStatsManager.GetLevelProgresses()
        };
        var body = JsonConvert.SerializeObject(progress);
        
        var response = await Client.PostAsync(request, new StringContent(body));
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonResponse = JsonConvert.DeserializeObject<Response>(content);
        
        if (jsonResponse.status == "error")
        {
            Debug.LogWarning("Error saving user progress: " + jsonResponse.message);
        }
        
        Debug.Log("Saved user progress ["+ progress.levels.Count +"]. (" + content + ")");
    }

    public static async void LoadUserProgress()
    {
        if (!IsLoggedIn())
            return;
        
        var request = _databaseURL + "getUserProgress?token=" + LoggedInToken;
        
        var response = await Client.GetAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonResponse = JsonConvert.DeserializeObject<LevelProgressResponse>(content);
        
        if (jsonResponse.status == "error")
        {
            Debug.LogWarning("Error loading user progress: " + jsonResponse.message);
            return;
        }
        
        var levels = jsonResponse.levels;
        foreach (var level in levels)
        {
            PlayerStatsManager.SaveLevelProgress(level);
        }
        
        Debug.Log("Loaded user progress ["+ levels.Count +"]. (" + content + ")");
    }

    public static async Task<LevelLeaderboardEntries> GetLevelLeaderboard(int levelId)
    {
        var request = _databaseURL + "getLevelLeaderboard?levelId=" + levelId;
        Debug.Log(request);
        
        var response = await Client.GetAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonResponse = JsonConvert.DeserializeObject<LevelLeaderboardEntries>(content);
        
        if (jsonResponse.status == "error")
        {
            Debug.LogWarning("Error loading level leaderboard: " + jsonResponse.message);
            return null;
        }
        
        return jsonResponse;
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
    
    [Serializable]
    public class LevelLeaderboardEntry
    {
        
        public string username;
        public int dots;
        public int deaths;
        public int stars;
        
    }
    
    [Serializable]
    public class LevelLeaderboardEntries : Response
    {
        
        public List<LevelLeaderboardEntry> entries;
        
    }
    
    [Serializable]
    private class LevelProgressArray
    {
        
        [SerializeField]
        public List<PlayerStatsManager.LevelProgressData> levels;
        
    }
    
    [Serializable]
    private class LevelProgressResponse : Response
    {
        
        [SerializeField]
        public List<PlayerStatsManager.LevelProgressData> levels;
        
    }

    public static void Logout()
    {
        LoggedInToken = null;
        PlayerPrefs.DeleteKey("token");
    }

    [Serializable]
    public class Response
    {

        public string message;
        public string status;
    }
    
    [Serializable]
    public class PaginationResponse : Response
    {
        
        public int page;
        public int pages;
        public int count;

        public bool hasNext;
        public bool hasPrevious;

    }
    
    [Serializable]
    public class LevelCommentsResponse : PaginationResponse
    {

        public List<LevelComment> comments;

    }
    
    [Serializable]
    public class LevelComment
    {

        public int id;
        public bool pinned;
        public int levelId;
        public int userId;
        public string comment;
        public string username;
        public long date;

    }
    
    [Serializable]
    public class IsLevelPublishedResponse : Response
    {
        
        public bool published;
        
    }
    
    [Serializable]
    private class LoginResponse : Response
    {
        public string token;
    }
    
    [Serializable]
    public class PersonalAccountInfosResponse : Response
    {

        public string username;
        public int id;
        public bool admin;
        public string created;
        
        // Since 1.0.0:
        public int publishedLevels;
        public int collectedStars;
        public int completedLevels;

    }
    
    [Serializable]
    public class LevelUploadResponse : Response
    {

        public string author;
        public string name;
        public bool verified;
        public int id;

    }
    
    [Serializable]
    public class LevelDownloadResponse : Response
    {

        public SaveData level;

    }
    
    [Serializable]
    public class EventSkinResponse : Response
    {

        public int skinId;

    }

    public static bool IsLoggedIn()
    {
        if (LoggedInToken == null && PlayerPrefs.HasKey("token"))
            LoggedInToken = PlayerPrefs.GetString("token");
        
        return LoggedInToken != null;
    }

    public static async Task<bool> IsAdmin()
    {
        if (!IsLoggedIn())
            return false;

        var infos = await GetLastCachedInfos();
        if (infos == null)
        {
            Debug.LogWarning("Could not get personal infos."); 
            Logout();
            return false;
        }
        return infos.admin;
    }
}