using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
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
        asanaAPISettings = asanaAPI.asanaSpecificSettings;
        this.asanaAPI = asanaAPI;
        request = default(HttpWebRequest);
    }

    /// <summary>
    /// Get all tasks from AsanaRequestManager, 
    /// fire TicketsRecievedEvent to create lucene index wihth tickets.
    /// </summary>
    public async override void GetAllData() {

        //include an update intervall
        if (asanaAPI.lastUpdateTime.AddMinutes(2) > DateTime.Now) {
            asanaAPI.FireTicketsCreated(asanaAPI.ticketModelsBackup);
            return;
        }

        string url = $"{asanaAPISettings.baseUrl}{asanaAPISettings.getAllTaskDataEndpoint}";
        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            try {
                List<TicketModels.AsanaTaskModel> ticketModels = new List<TicketModels.AsanaTaskModel>();
                string result = await reader.ReadToEndAsync();
                ticketModels = JsonConvert.DeserializeObject<List<TicketModels.AsanaTaskModel>>(result);
                asanaAPI.ticketModelsBackup.Clear();
                asanaAPI.ticketModelsBackup.AddRange(ticketModels);
                asanaAPI.FireTicketsCreated(ticketModels);
                asanaAPI.lastUpdateTime = DateTime.Now;
            } catch (Exception e) {
                Debug.LogWarning(e.Message);
            }
        }
    }

   /// <summary>
   /// Post the new data object to AsanaRequestManager
   /// </summary>
   /// <param name="data">Request Data Object. Use @BuildTaskData() to create.</param>
    public async override void PostNewData(RequestData data) {
        string userID = CheckForUserGid();
        string url = $"{asanaAPISettings.baseUrl}{asanaAPISettings.postNewTaskDataEndpoint}{userID}";
        string requestData = BuildTaskData(data);

        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.POST.ToString();
        request.ContentType = "application/json";
        request.Timeout = 5000;

        try {
            byte[] dataBytes11 = Encoding.UTF8.GetBytes(requestData);

            using (Stream postStream = request.GetRequestStream()) {
                postStream.Write(dataBytes11, 0, dataBytes11.Length);
            }

            StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream());
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Debug.Log(sr.ReadToEnd());
            asanaAPI.customFields.Clear();
            tags.Clear();
        } catch (Exception e) {
            Debug.LogError("An error occured while posting new task: " + e.Message);
        }
    }

    /// <summary>
    /// Build a task data object out of the user input. 
    /// The object contains a name, notes the project id and the workspace id.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string BuildTaskData(RequestData data) {
        string projectId = asanaAPISettings.bugProjectId;
        if (data.dataType is DataType.Feedback) { projectId = asanaAPISettings.feedbackProjectId; }

        byte[] imageArray = data.screenshot.EncodeToPNG();
        var result = Convert.ToBase64String(imageArray);
        BuildCustomFields();

        NewAsanaTicketRequest newTicketRequest = new NewAsanaTicketRequest();
        newTicketRequest.data.name = data.title;
        newTicketRequest.data.notes = data.text;
        newTicketRequest.data.projects = projectId;
        newTicketRequest.data.workspace = asanaAPISettings.workspaceId;
        newTicketRequest.data.attachment = result;
        newTicketRequest.data.custom_fields = asanaAPI.customFields;
        string output = JsonConvert.SerializeObject(newTicketRequest, Formatting.Indented);
        
        return output;
    }

    private async void BuildCustomFields() {
        TicketModels.ReportTags reportTags = new TicketModels.ReportTags();
        string url = $"{asanaAPISettings.baseUrl}{asanaAPISettings.getCustomFields}";
        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            try {
                string result = await reader.ReadToEndAsync();
                reportTags = JsonConvert.DeserializeObject<TicketModels.ReportTags>(result);
                asanaAPI.customFields.Add(reportTags.gid); //first member of list is always gid of custom field
                foreach (TicketModels.Tags tag in reportTags.enum_options) {
                    foreach (ScriptableTag stag in tags) {
                        if (stag.tagName.ToLower().Equals(tag.name.ToLower())) {
                            asanaAPI.customFields.Add(tag.gid);
                        }
                    }
                }  
            } catch (Exception e) {
                Debug.LogWarning(e.Message);
            }
        }

       

    }

    /// <summary>
    /// Use AsanaRequestHandler upvote endpoint to send upvotes.
    /// </summary>
    /// <param name="ticket"></param>
    public async override void PostUpvoteCount(string ticketId, int count) {
        string userID = CheckForUserGid();
        string url = $"{asanaAPISettings.baseUrl}{asanaAPISettings.updateUpvotesEndpoint}{userID}/{ticketId}/{count}";

        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();
        request.ContentType = "application/json";
        request.Timeout = 5000;

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            try {
                List<TicketModels.AsanaTaskModel> ticketModels = new List<TicketModels.AsanaTaskModel>();
                string result = await reader.ReadToEndAsync();
                Debug.Log(result);
            } catch (Exception e) {
                Debug.LogWarning(e.Message);
            }
        }
    }

    /// <summary>
    /// Add a tag to the tag list. 
    /// </summary>
    /// <param name="tag"></param>
    public override void AddTagToTagList(ScriptableTag tag) {
        if (!tags.Contains(tag)) {
            tags.Add(tag);
        }
    }

    /// <summary>
    /// Remove a tag from the tag list. 
    /// </summary>
    /// <param name="tag"></param>
    public override void RemoveTagFromTagList(ScriptableTag tag) {
        if (tags.Contains(tag)) {
            tags.Remove(tag);
        }
    }

    #region Authentication with AsanaRequestManager

    /// <summary>
    /// Generate a uniqueId and send a request to AsnaRequestManagers Authentication endpoint, 
    /// start OAuh2.0 Flow and get access to asana api.
    /// </summary>
    public override void LogIn() {
        uniqueId = Guid.NewGuid().ToString();
        string url = $"{asanaAPISettings.baseUrl}{asanaAPISettings.loginEndpoint}{uniqueId}";
        Application.OpenURL(url);
        
    }

    /// <summary>
    /// Send Request to AsanaRequestManagers logout endpoint,
    /// delete client object with passing uniqueId
    /// </summary>
    public override void LogOut() {
        string url = $"{asanaAPISettings.baseUrl}{asanaAPISettings.logoutEndpoint}{uniqueId}";
        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            Debug.Log(reader.ReadToEnd());
        }
        base.user = null;
        uniqueId = "";
    }

    /// <summary>
    /// Search for uniqueId in AsanaRequestManager clientCollection and return the matching client object
    /// set the returning object as user object.
    /// </summary>
    /// <returns></returns>
    public override AuthorizationUser GetUser() {
        string url = $"{asanaAPISettings.baseUrl}{asanaAPISettings.getUserWithUniqueId}{uniqueId}";
        request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = RequestMethods.GET.ToString();

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
            try {
                base.user = JsonConvert.DeserializeObject<AuthorizationUser>(reader.ReadToEnd());
                Debug.Log("<color=green> User: " + base.user.gid + "; " + base.user.name + " successfully logged in. </color>");
            } catch (Exception e) {
                Debug.LogWarning(reader.ReadToEnd());
            }
        }
      
        return base.user;
    }

    private string CheckForUserGid() {
        if(base.user is null) {
            return "0";
        } else {
            return base.user.gid;
        }
    }

    #endregion
}

#region Response objects 
public class AuthorizationInfo {
    public string unique_id;
    public AuthorizationUser data;
}

public class AuthorizationUser {
    public string gid;
    public string name;
    public string email;
}
#endregion
