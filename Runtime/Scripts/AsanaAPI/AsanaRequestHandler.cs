using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Feedback.TaskModels;
using Debug = UnityEngine.Debug;

//query builder https://developers.asana.com/reference/searchtasksforworkspace

/// <summary>
/// Class that creates an asana client instance.
/// </summary>
namespace Feedback {
    public class AsanaRequestHandler : BaseRequestHandler {
        private AsanaAPI asanaAPI;
        private AsanaAPISettings asanaAPISettings;
        private HttpWebRequest request;
        private bool killLogin = false;
        private string uniqueId = string.Empty;
        public override string UniqueId {
            get {
                if (!string.IsNullOrEmpty(uniqueId)) {
                    return uniqueId;
                }

                uniqueId = PlayerPrefs.GetString(nameof(AsanaRequestHandler) + "_" + nameof(uniqueId));

                return uniqueId;
            }
            set {
                uniqueId = value;
                PlayerPrefs.SetString(nameof(AsanaRequestHandler) + "_" + nameof(uniqueId), uniqueId);
            }
        }

        public AsanaRequestHandler(AsanaAPI asanaAPI) {
            asanaAPISettings = asanaAPI.AsanaSpecificSettings;
            this.asanaAPI = asanaAPI;
            request = default(HttpWebRequest);
        }

        /// <summary>
        /// Get all tasks from AsanaRequestManager, 
        /// </summary>
        public async override void GetData(bool force) {
            if (!force) {
                if (asanaAPI.lastUpdateTime.AddSeconds(asanaAPISettings.dataFetchCooldown) > DateTime.Now || requestRunning) {
                    if (asanaAPI.ReportTagsBackup != null) {
                        asanaAPI.FireDataCreated(asanaAPI.ReportTagsBackup);
                        return;
                    }
                }
            }

            Task task = new Task(GetDataAsync);
            try {
                task.Start();
                await task;
            } catch (Exception e) {
                Debug.LogWarning($"[FeedbackTool] {e.Message}");
            }
        }

        private async void GetDataAsync() {
            requestRunning = true;

            ReportTags reportTags = new ReportTags();
            reportTags.enum_options = new List<Tags>();

            try {
                using (HttpClient client = new HttpClient()) {
                    client.Timeout = TimeSpan.FromMilliseconds(asanaAPISettings.recieveTimeout);

                    //Report Tags Request
                    string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetReportTags}";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode) {
                        string result = await response.Content.ReadAsStringAsync();
                        reportTags = JsonConvert.DeserializeObject<ReportTags>(result);
                        asanaAPI.ReportTagsBackup = reportTags;
                    } else {
                        Debug.LogWarning($"[FeedbackTool] Something went wrong while getting data, no tags are loaded. Status code: {response.StatusCode}");
                    }
                }
                asanaAPI.lastUpdateTime = DateTime.Now;
            } catch (HttpRequestException e) {
                Debug.LogWarning($"[FeedbackTool] Something went wrong while getting data, no tags are loaded. ({e.GetType()} - {e.Message})");
            } catch (WebException e) {
                Debug.LogWarning($"[FeedbackTool] Something went wrong while getting data, no tags are loaded. ({e.GetType()} - {e.Message})");
            } catch (Exception e) {
                Debug.LogWarning($"[FeedbackTool] Something went wrong while getting data, no tags are loaded. ({e.GetType()} - {e.Message})");
            }

            asanaAPI.FireDataCreated(reportTags);
            requestRunning = false;
        }

        /// <summary>
        /// Post the new data object to AsanaRequestManager.
        /// </summary>
        /// <param name="data">Request Data Object. Use @BuildTaskData() to create.</param>
        public async override void PostNewData(RequestData data) {
            requestRunning = true;
            postRequestRunning = true;

            string userID = CheckForUserGid();
            string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.PostNewTaskDataEndpoint}{userID}";

            string requestData = BuildTaskData(data);

            try {
                using (HttpClient client = new HttpClient()) {
                    client.Timeout = TimeSpan.FromMilliseconds(asanaAPISettings.sendTimeout);

                    StringContent content = new StringContent(requestData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode) {
                        asanaAPI.CustomFields.Clear();
                        Tags.Clear();
                    } else {
                        Debug.LogWarning("[FeedbackTool] An error occured while posting new task: " + response.StatusCode);
                        requestRunning = false;
                        postRequestRunning = false;
                        asanaAPI.FireFeedbackSend(false);
                        return;
                    }
                }
            } catch (Exception e) {
                Debug.LogWarning($"[FeedbackTool] An error occured while posting new task. ({e.GetType()} - {e.Message})");
                requestRunning = false;
                postRequestRunning = false;
                asanaAPI.FireFeedbackSend(false);
                return;
            }

            requestRunning = false;
            postRequestRunning = false;
            asanaAPI.FireFeedbackSend(true);
        }

        /// <summary>
        /// Build a task data object out of the user input. 
        /// The object contains a name, notes the project id, the workspace id, a tag list and an attachment.                                                                   
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string BuildTaskData(RequestData data) {
            string projectId = data.AsanaProject != null ? data.AsanaProject.id : string.Empty;

            AsanaTicketRequest.TicketData newTicketRequest = new AsanaTicketRequest.TicketData();
            newTicketRequest.name = data.Title;
            newTicketRequest.notes = data.Text;
            newTicketRequest.projects = projectId;
            newTicketRequest.workspace = asanaAPISettings.WorkspaceId;
            newTicketRequest.custom_fields = BuildCustomFields(data.AsanaProject);
            newTicketRequest.attachments = data.Attachments.ToArray();
            newTicketRequest.html_notes = BuildRichText(data.Text);
            string output = JsonConvert.SerializeObject(newTicketRequest, Formatting.Indented);

            return output;
        }

        private Dictionary<string, List<string>> BuildCustomFields(AsanaProject projectType) {
            Dictionary<string, List<string>> customFields = new Dictionary<string, List<string>>();
            //Build ReportTags CustomField
            if (asanaAPI.ReportTagsBackup.enum_options != null) {
                List<string> tags = new List<string>();
                foreach (Tags tag in asanaAPI.ReportTagsBackup.enum_options) {
                    foreach (TagPreview stag in Tags) {
                        if (stag.title.ToLower().Equals(tag.name.ToLower())) {
                            tags.Add(tag.gid);
                        }
                    }
                }
                customFields.Add(asanaAPI.ReportTagsBackup.gid, tags);
            }

            //Build Custom CustomFields
            List<CustomData> customData = asanaAPISettings.Adapter.GetCustomFields(projectType);
            if (customData != null) {
                for (int i = 0; i < customData.Count; i++) {
                    if (!string.IsNullOrEmpty(customData[i].gid)) {
                        customFields.Add(customData[i].gid.ToString(), customData[i].values);
                    }
                }
            }

            return customFields;
        }

        private string BuildRichText(string notes) {
            return $"<body>{EscapeCharacters(notes)}</body>";
        }

        private string EscapeCharacters(string notes) {
            notes = notes.Replace("&", "&amp;");
            notes = notes.Replace("<", "&lt;");
            notes = notes.Replace(">", "&gt;");

            return notes;
        }

        /// <summary>
        /// Add a tag to the tag list. 
        /// </summary>
        /// <param name="tag"></param>
        public override void AddTagToTagList(TagPreview tag) {
            if (!Tags.Contains(tag)) {
                Tags.Add(tag);
            }
        }

        /// <summary>
        /// Remove a tag from the tag list. 
        /// </summary>
        /// <param name="tag"></param>
        public override void RemoveTagFromTagList(TagPreview tag) {
            if (Tags.Contains(tag)) {
                Tags.Remove(tag);
            }
        }

        #region Authentication with AsanaRequestManager

        /// <summary>
        /// Generate a uniqueId and send a request to AsnaRequestManagers Authentication endpoint, 
        /// start OAuh2.0 Flow and get access to asana api.
        /// </summary>
        public override void LogIn() {
            UniqueId = Guid.NewGuid().ToString();
            string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.LoginEndpoint}{UniqueId}";
            asanaAPISettings.Adapter.OpenUrl(url);

            TryGetUser();
        }

        /// <summary>
        /// Send Request to AsanaRequestManagers logout endpoint,
        /// delete client object with passing uniqueId
        /// </summary>
        public override void LogOut() {
            requestRunning = true;

            string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.LogoutEndpoint}{UniqueId}";
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = RequestMethods.GET.ToString();
            request.Timeout = asanaAPISettings.loginoutTimeout;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                Debug.Log($"[FeedbackTool] {reader.ReadToEnd()}");
            }
            base.User = null;
            UniqueId = "";

            requestRunning = false;
        }

        /// <summary>
        /// Get User Data Sync
        /// </summary>
        private async void TryGetUser() {
            try {
                bool loginRunning = true;
                int maxTrys = 120; //2 minutes to login
                int trys = 0;

                while (loginRunning) {
                    string id = UniqueId;
                    Task task = new Task(() => TryGetUserAsync(id));

                    task.Start();
                    await task;

                    if (User != null) {
                        loginRunning = false;
                        trys = 0;
                        asanaAPI.FireLoginResult(true);
                    } else if (trys == maxTrys || killLogin) {
                        loginRunning = false;
                        trys = 0;
                        asanaAPI.FireLoginResult(false);
                    } else {
                        trys++;
                        await Task.Delay(1000);
                    }
                }

                killLogin = false;
            } catch (Exception e) {
                Debug.LogException(e);
                asanaAPI.FireLoginResult(false);
            }
        }

        /// <summary>
        /// Get User Data Sync
        /// </summary>
        public override async void TryGetUserAsync(string uniqueId) {
            if (string.IsNullOrEmpty(uniqueId)) {
                asanaAPI.FireGetUserResult();
                return;
            }

            requestRunning = true;
            logginChange = false;

            string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetUserWithUniqueId}{uniqueId}";

            try {
                using (HttpClient client = new HttpClient()) {
                    client.Timeout = TimeSpan.FromMilliseconds(asanaAPISettings.loginoutTimeout);

                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode) {
                        string result = await response.Content.ReadAsStringAsync();
                        AuthorizationUser newUser = JsonConvert.DeserializeObject<AuthorizationUser>(result);

                        if (User == null && newUser != null) {
                            logginChange = true;
                        }

                        User = newUser;
                    } else {
                        logginChange = User != null;
                        User = null;
                    }
                }
            } catch {
                logginChange = User != null;
                User = null;
            }

            asanaAPI.FireGetUserResult();

            requestRunning = false;
        }

        private string CheckForUserGid() {
            if (base.User is null) {
                return "0";
            } else {
                return base.User.gid;
            }
        }

        public override void AbortLogin() {
            killLogin = true;
        }

        public override void LoadAvatar() {
            requestRunning = true;

            if (User != null && !string.IsNullOrWhiteSpace(User.picture)) {
                try {
                    byte[] imageData = new byte[0];

                    // Create a HttpWebRequest with the image URL
                    request = (HttpWebRequest)WebRequest.Create(User.picture);
                    request.Timeout = asanaAPISettings.loginoutTimeout;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (MemoryStream memoryStream = new MemoryStream()) {
                        stream.CopyTo(memoryStream);
                        imageData = memoryStream.ToArray();
                    }

                    Texture2D loadedTexture = new Texture2D(2, 2);
                    loadedTexture.LoadImage(imageData);

                    User.avatar = loadedTexture;
                    asanaAPI.FireAvatarLoaded();
                } catch (Exception e) {
                    if (User != null) {
                        Debug.LogWarning($"[FeedbackTool] tried to fetch avatar picture @ url: {User.picture} and failed!");
                    }
                    Debug.LogWarning($"[FeedbackTool] {e.Message}");
                }
            }

            requestRunning = false;
        }
        #endregion
    }

    public class AuthorizationUser {
        public string gid;
        public string name;
        public string email;
        public string picture;
        public Texture2D avatar = null;
    }
}