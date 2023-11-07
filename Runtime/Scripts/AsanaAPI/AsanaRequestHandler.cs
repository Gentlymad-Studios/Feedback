using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
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
        public string UniqueId {
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
        /// fire TicketsRecievedEvent to create lucene index with tickets.
        /// </summary>
        public async override void GetData(bool force) {
            if (!force) {
                if (asanaAPI.lastUpdateTime.AddMinutes(1) > DateTime.Now || requestRunning) {
                    if (asanaAPI.PlayerTicketModelsBackup != null && asanaAPI.PlayerTicketModelsBackup.Count != 0 && asanaAPI.ReportTagsBackup != null) {
                        asanaAPI.FireDataCreated(asanaAPI.PlayerTicketModelsBackup, asanaAPI.DevTicketModelsBackup, asanaAPI.ReportTagsBackup);
                        return;
                    }
                }
            }

            Task task = new Task(GetDataAsync);
            try {
                task.Start();
                await task;
            } catch (Exception e) {
                Debug.LogWarning(e.Message);
            }
        }

        private async void GetDataAsync() {
            requestRunning = true;

            List<AsanaTaskModel> devTicketModels = new List<AsanaTaskModel>();
            List<AsanaTaskModel> playerTicketModels = new List<AsanaTaskModel>();
            ReportTags reportTags = new ReportTags();

            string url = "";
            try {
                if (asanaAPISettings.enablePlayerProjects) {
                    url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetPlayerTaskDataEndpoint}";
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = RequestMethods.GET.ToString();
                    request.Timeout = 5000;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream)) {
                        string result = await reader.ReadToEndAsync();
                        playerTicketModels = JsonConvert.DeserializeObject<List<AsanaTaskModel>>(result);
                        asanaAPI.PlayerTicketModelsBackup.Clear();
                        if (playerTicketModels != null) {
                            asanaAPI.PlayerTicketModelsBackup.AddRange(playerTicketModels);
                            asanaAPI.lastUpdateTime = DateTime.Now;
                        }
                    }
                }

                if (base.User != null && asanaAPISettings.enableDevProjects) {
                    url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetDevTaskDataEndpoint}";
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = RequestMethods.GET.ToString();
                    request.Timeout = 3000;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream)) {
                        string result = await reader.ReadToEndAsync();
                        devTicketModels = JsonConvert.DeserializeObject<List<AsanaTaskModel>>(result);
                        asanaAPI.DevTicketModelsBackup.Clear();
                        if (devTicketModels != null) {
                            asanaAPI.DevTicketModelsBackup.AddRange(devTicketModels);
                            asanaAPI.lastUpdateTime = DateTime.Now;
                        }
                    }
                }

                url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetReportTags}";
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = RequestMethods.GET.ToString();
                request.Timeout = 3000;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream)) {
                    string result = await reader.ReadToEndAsync();
                    reportTags = JsonConvert.DeserializeObject<ReportTags>(result);
                    asanaAPI.ReportTagsBackup = reportTags;
                }

                if (playerTicketModels != null && devTicketModels != null && reportTags != null) {
                    asanaAPI.FireDataCreated(playerTicketModels, devTicketModels, reportTags);
                }
            } catch (Exception e) {
                Debug.LogError("Something went wrong while getting data...");
                Debug.LogException(e);
                asanaAPI.FireDataCreated(null, null, null);
            }

            requestRunning = false;
        }

        /// <summary>
        /// Post the new data object to AsanaRequestManager.
        /// </summary>
        /// <param name="data">Request Data Object. Use @BuildTaskData() to create.</param>
        public override void PostNewData<T1, T2>(RequestData<T1, T2> data) {
            requestRunning = true;

            string userID = CheckForUserGid();
            string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.PostNewTaskDataEndpoint}{userID}";

            string requestData = BuildTaskData(data);
            Debug.Log(requestData);

            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = RequestMethods.POST.ToString();
            request.ContentType = "application/json";
            request.Timeout = 10000;

            try {
                byte[] dataBytes11 = Encoding.UTF8.GetBytes(requestData);

                using (Stream postStream = request.GetRequestStream()) {
                    postStream.Write(dataBytes11, 0, dataBytes11.Length);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                Debug.Log(reader.ReadToEnd());
                response.Close();

                asanaAPI.CustomFields.Clear();
                Tags.Clear();
                return;

            } catch (Exception e) {
                Debug.LogError("An error occured while posting new task: " + e.Message);
            }

            requestRunning = false;
        }

        /// <summary>
        /// Build a task data object out of the user input. 
        /// The object contains a name, notes the project id, the workspace id, a tag list and an attachment.                                                                   
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string BuildTaskData<T1, T2>(RequestData<T1, T2> data) {
            string projectId = data.AsanaProject != null ? data.AsanaProject.id : string.Empty;

            List<NewAsanaTicketRequest.Attachment> attachments = new List<NewAsanaTicketRequest.Attachment>();

            foreach (var dictonarySet in data.Attachments) {
                //Dictionary with filename and file text representation
                Dictionary<T1, T1> typeOneFileSets = dictonarySet.Key;
                foreach (var fileSet in typeOneFileSets) {
                    attachments.Add(BuildAttachment(fileSet.Value, fileName: fileSet.Key.ToString()));
                }

                foreach (T2 typeTwoListEntry in dictonarySet.Value) {
                    attachments.Add(BuildAttachment(typeTwoListEntry));
                }
            }

            NewAsanaTicketRequest.NewTicketData newTicketRequest = new NewAsanaTicketRequest.NewTicketData();
            newTicketRequest.name = data.Title;
            newTicketRequest.notes = data.Text;
            newTicketRequest.projects = projectId;
            newTicketRequest.workspace = asanaAPISettings.WorkspaceId;
            newTicketRequest.custom_fields = BuildCustomFields(data.AsanaProject);
            newTicketRequest.attachments = attachments.ToArray();
            newTicketRequest.html_notes = BuildRichText(data.Text);
            string output = JsonConvert.SerializeObject(newTicketRequest, Formatting.Indented);

            return output;
        }

        private NewAsanaTicketRequest.Attachment BuildAttachment<T>(T attachmentData, string fileName = "") {
            string name = fileName;
            string ending = "";
            if (fileName.Equals("")) {
                name = "attachment";
                ending = ".txt";
            }
            string contentType = "text/plain";
            string content = "";

            using (var ms = new MemoryStream()) {
                if (attachmentData is Texture2D tex) {
                    content = Convert.ToBase64String(tex.EncodeToJPG());
                } else {
                    try {
                        new BinaryFormatter().Serialize(ms, attachmentData);
                        byte[] byteArray = ms.ToArray();
                        content = Convert.ToBase64String(byteArray, Base64FormattingOptions.None);
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }

            if (attachmentData is Texture2D) {
                ending = ".jpg";
                contentType = "image/jpg";
            }

            NewAsanaTicketRequest.Attachment attachment = new NewAsanaTicketRequest.Attachment() {
                filename = name + ending,
                contentType = contentType,
                content = content
            };

            return attachment;
        }

        private Dictionary<string, List<string>> BuildCustomFields(AsanaProject projectType) {
            Dictionary<string, List<string>> customFields = new Dictionary<string, List<string>>();
            //Build ReportTags CustomField
            List<string> tags = new List<string>();
            foreach (Tags tag in asanaAPI.ReportTagsBackup.enum_options) {
                foreach (TagPreview stag in Tags) {
                    if (stag.title.ToLower().Equals(tag.name.ToLower())) {
                        tags.Add(tag.gid);
                    }
                }
            }
            customFields.Add(asanaAPI.ReportTagsBackup.gid, tags);

            //Build Custom CustomFields
            if (projectType.CustomDataCallback != null) {
                List<AsanaProject.CustomData> customData = projectType.CustomDataCallback.Invoke();
                for (int i = 0; i < customData.Count; i++) {
                    if (!string.IsNullOrEmpty(customData[i].gid)) {
                        customFields.Add(customData[i].gid.ToString(), customData[i].values);
                    }
                }
            }

            return customFields;
        }

        private string BuildRichText(string notes) {
            string front = "<body>";
            string middle = "";
            string back = "</body>";

            if (asanaAPI.Mentions.Count != 0) {
                front += "<strong>Mentions</strong><ul>";

                foreach (string id in asanaAPI.Mentions) {
                    middle += $"<li><a data-asana-gid=\"{id}\"/></li>";
                }
                middle += "</ul>";
            }
            middle += notes;
            return front + middle + back;
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
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                Debug.Log(reader.ReadToEnd());
            }
            base.User = null;
            UniqueId = "";

            requestRunning = false;
        }

        /// <summary>
        /// Search for uniqueId in AsanaRequestManager clientCollection and return the matching client object
        /// set the returning object as user object.
        /// </summary>
        /// <returns></returns>
        public override AuthorizationUser GetUser() {
            requestRunning = true;

            string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetUserWithUniqueId}{UniqueId}";
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = RequestMethods.GET.ToString();

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                try {
                    base.User = JsonConvert.DeserializeObject<AuthorizationUser>(reader.ReadToEnd());
                    Debug.Log("<color=green> User: " + base.User.gid + "; " + base.User.name + " successfully logged in. </color>");
                } catch (Exception e) {
                    Debug.LogError("An error occured while logging in user: " + e);
                }
            }

            requestRunning = false;
            return base.User;
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
                    Task task = new Task(TryGetUserAsync);

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
        public override async void TryGetUserAsync() {
            if (string.IsNullOrEmpty(UniqueId)) {
                return;
            }

            requestRunning = true;

            string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetUserWithUniqueId}{UniqueId}";
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = RequestMethods.GET.ToString();
            request.Timeout = 3000;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                try {
                    string result = await reader.ReadToEndAsync();
                    User = JsonConvert.DeserializeObject<AuthorizationUser>(result);
                } catch { }
            }

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

        public async override void LoadAvatar() {
            requestRunning = true;

            if (!string.IsNullOrWhiteSpace(User.picture)) {
                try {
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(User.picture);
                    request.timeout = 2000;

                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                    while (!operation.isDone) {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success) {
                        User.avatar = ((DownloadHandlerTexture)request.downloadHandler).texture;
                        asanaAPI.FireAvatarLoaded();
                    }
                } catch (Exception e) {
                    Debug.LogWarning($"tried to fetch avatar picture @ url: {User.picture} and failed!");
                    Debug.LogWarning(e.Message);
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