using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

//query builder https://developers.asana.com/reference/searchtasksforworkspace

/// <summary>
/// Class that creates an asana client instance.
/// </summary>
public class AsanaRequestHandler : BaseRequestHandler {

    private AsanaAPI asanaAPI;
    private AsanaAPISettings asanaAPISettings;
    private HttpWebRequest request;
    private string uniqueId = string.Empty;

    public AsanaRequestHandler(AsanaAPI asanaAPI) {
        asanaAPISettings = asanaAPI.AsanaSpecificSettings;
        this.asanaAPI = asanaAPI;
        request = default(HttpWebRequest);
    }

    /// <summary>
    /// Get all tasks from AsanaRequestManager, 
    /// fire TicketsRecievedEvent to create lucene index with tickets.
    /// </summary>
    public async override void GetAllData() {

        if (asanaAPI.lastUpdateTime.AddMinutes(1) > DateTime.Now) {
            asanaAPI.FireTasksCreated(asanaAPI.TicketModelsBackup);
            return;
        }

        Task task = new Task(GetAllDataAsync);
        try {
            task.Start();
            await task;
            return;

        } catch (Exception e) {
            Debug.LogWarning(e.Message);
        }

    }

    private async void GetAllDataAsync() {
        string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetAllTaskDataEndpoint}";
        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            string result = await reader.ReadToEndAsync();
            List<TaskModels.AsanaTaskModel> ticketModels = new List<TaskModels.AsanaTaskModel>();
            ticketModels = JsonConvert.DeserializeObject<List<TaskModels.AsanaTaskModel>>(result);
            asanaAPI.TicketModelsBackup.Clear();
            asanaAPI.TicketModelsBackup.AddRange(ticketModels);
            asanaAPI.FireTasksCreated(ticketModels);
            asanaAPI.lastUpdateTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Post the new data object to AsanaRequestManager.
    /// </summary>
    /// <param name="data">Request Data Object. Use @BuildTaskData() to create.</param>
    public override void PostNewData<T1, T2>(RequestData<T1, T2> data) {
   
        string userID = CheckForUserGid();
        string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.PostNewTaskDataEndpoint}{userID}";

        string requestData = BuildTaskData(data);
        Debug.Log(requestData);

        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.POST.ToString();
        request.ContentType = "application/json";
        request.Timeout = 5000;

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
    }

    /// <summary>
    /// Build a task data object out of the user input. 
    /// The object contains a name, notes the project id, the workspace id, a tag list and an attachment.                                                                   
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string BuildTaskData<T1, T2>(RequestData<T1, T2> data) {
        string projectId = asanaAPISettings.BugProjectId;
        if (data.DataType.Equals("Feedback")) { projectId = asanaAPISettings.FeedbackProjectId; }

        List<NewAsanaTicketRequest.Attachment> attachments = new List<NewAsanaTicketRequest.Attachment>();

        foreach (var dictonarySet in data.Attachments) {
            foreach (T1 typOneListEntry in dictonarySet.Key) {
                attachments.Add(BuildAttachment(typOneListEntry));
            }
            foreach (T2 typeTwoListEntry in dictonarySet.Value) {
                attachments.Add(BuildAttachment(typeTwoListEntry));
            }
        }

        BuildCustomFields();

        NewAsanaTicketRequest.NewTicketData newTicketRequest = new NewAsanaTicketRequest.NewTicketData();
        newTicketRequest.name = data.Title;
        newTicketRequest.notes = data.Text;
        newTicketRequest.projects = projectId;
        newTicketRequest.workspace = asanaAPISettings.WorkspaceId;
        newTicketRequest.custom_fields = asanaAPI.CustomFields;
        newTicketRequest.attachments = attachments.ToArray();
        newTicketRequest.html_notes = BuildRichText(data.Text);
        string output = JsonConvert.SerializeObject(newTicketRequest, Formatting.Indented);

        return output;
    }

    private NewAsanaTicketRequest.Attachment BuildAttachment<T>(T attachmentData) {
        string filename = "attachment";
        string fileEnding = ".txt";
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
            fileEnding = ".jpg";
            contentType = "image/jpg";
        }

        NewAsanaTicketRequest.Attachment attachment = new NewAsanaTicketRequest.Attachment() {
            filename = filename + fileEnding,
            contentType = contentType,
            content = content
        };

        return attachment;
    }

    private async void BuildCustomFields() {
        string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetCustomFields}";
        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            try {
                TaskModels.ReportTags reportTags = new TaskModels.ReportTags();
                string result = await reader.ReadToEndAsync();
                reportTags = JsonConvert.DeserializeObject<TaskModels.ReportTags>(result);
                //first member of list is always gid of custom field
                asanaAPI.CustomFields.Add(reportTags.gid);
                foreach (TaskModels.Tags tag in reportTags.enum_options) {
                    foreach (TagPreview stag in Tags) {
                        if (stag.title.ToLower().Equals(tag.name.ToLower())) {
                            asanaAPI.CustomFields.Add(tag.gid);
                        }
                    }
                }
                return;
            } catch (Exception e) {
                Debug.LogWarning("An exception occured while building custom field object " + e.Message);
            }
        }
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
    /// Use AsanaRequestHandler upvote endpoint to send upvotes.
    /// </summary>
    /// <param name="ticket"></param>
    public async override void PostUpvoteCount(string ticketId, int count) {
        string userID = CheckForUserGid();
        string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.UpdateUpvotesEndpoint}{userID}/{ticketId}/{count}";

        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();
        request.ContentType = "application/json";
        request.Timeout = 5000;

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            try {
                List<TaskModels.AsanaTaskModel> ticketModels = new List<TaskModels.AsanaTaskModel>();
                string result = await reader.ReadToEndAsync();
                Debug.Log(result);
                response.Close();
                return;
            } catch (Exception e) {
                Debug.LogError("An error occoured while posting increased upvote count: " + e.Message);
            }
        }
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
        uniqueId = Guid.NewGuid().ToString();
        string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.LoginEndpoint}{uniqueId}";
        Application.OpenURL(url);
    }

    /// <summary>
    /// Send Request to AsanaRequestManagers logout endpoint,
    /// delete client object with passing uniqueId
    /// </summary>
    public override void LogOut() {
        string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.LogoutEndpoint}{uniqueId}";
        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            Debug.Log(reader.ReadToEnd());
        }
        base.User = null;
        uniqueId = "";
    }

    /// <summary>
    /// Search for uniqueId in AsanaRequestManager clientCollection and return the matching client object
    /// set the returning object as user object.
    /// </summary>
    /// <returns></returns>
    public override AuthorizationUser GetUser() {
        string url = $"{asanaAPISettings.BaseUrl}{asanaAPISettings.GetUserWithUniqueId}{uniqueId}";
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

        return base.User;
    }

    private string CheckForUserGid() {
        if (base.User is null) {
            return "0";
        } else {
            return base.User.gid;
        }
    }

    #endregion
}


public class AuthorizationUser {
    public string gid;
    public string name;
    public string email;
}

