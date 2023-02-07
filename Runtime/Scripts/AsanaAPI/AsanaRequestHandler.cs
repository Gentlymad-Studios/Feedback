using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

//query builder https://developers.asana.com/reference/searchtasksforworkspace

/// <summary>
/// Class that creates an asana client instance.
/// </summary>
public class AsanaRequestHandler : BaseRequestHandler {

    // TODO: alternative event based way to receive data from anywhere
    public delegate void TicketModelsCreated(List<TicketModel> tickets);
    public static event TicketModelsCreated TicketModelsReceivedEvent;

    private StreamReader _sr;
    private AsanaAPI asanaAPI;
    private AsanaAPISettings asanaAPISettings;
    private AuthorizationInfo _authorizationInfo;
    private HttpWebRequest request;
    private HttpWebResponse response;

    private string paginatedCreatedBefore;
    private string paginatedCreatedAfter;
    public AsanaRequestHandler(AsanaAPI asanaAPI) {
        asanaAPISettings = asanaAPI.asanaSpecificSettings;
        this.asanaAPI = asanaAPI;
        request = default(HttpWebRequest);
    }

    public Action OnTokenSubmit;

    public override void GET() {
        GetTasksWithCustomPagination();
    }

    //Get a lot asana task (may not all because there could be more than 100 tasks created in a month).
    //Diagnostic method to measure time and file size to fetch and store up to 2500 tasks from asana api.
    private async void GetTasksWithCustomPagination() {
        // TODO: added list of ticket models here. Evaluate if this makes sense or if it should become a class wide field...
        List<TicketModel> ticketModels = new List<TicketModel>();
        Stopwatch sw = new Stopwatch();
        //TODO: most liekely already on your radar but if this is the way data has to be requested we most liekely need a dynamic solution, that builts these monthly requests automatically and based on the current year?
        string[] dates = new string[] { "2020-09-01", "2020-10-01", "2020-11-01", "2020-12-01",
            "2021-01-01", "2021-02-01", "2021-03-01", "2021-04-01", "2021-05-01", "2021-06-01", "2021-07-01", "2021-08-01", "2021-09-01", "2021-10-01", "2021-11-01", "2021-12-01",
         "2022-01-01", "2022-02-01", "2022-03-01", "2022-04-01", "2022-05-01", "2022-06-01", "2022-07-01", "2022-08-01", "2022-09-01", "2022-10-01", "2022-11-01", "2022-12-01",
         "2023-01-01", "2023-02-01"};
        sw.Start();
        for (int i = 0; i < dates.Length; i++) {

            paginatedCreatedAfter = dates[i];
            if (i != dates.Length - 1) {
                paginatedCreatedBefore = dates[i + 1];
            } else {
                paginatedCreatedBefore = dates[i];
            }

            var url = $"{asanaAPISettings.baseURL}{asanaAPISettings.workspaceRoute}/{asanaAPISettings.workspaceId}/tasks/search?opt_fields=created_at,name,notes&resource_subtype=default_task&" +
                $"created_on.before={paginatedCreatedBefore}&created_on.after={paginatedCreatedAfter}&sort_by=modified_at&sort_ascending=false";

            ConfigureRequest(url, RequestMethods.GET);
            
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                // TODO: Instead of writing responses to disk we just simply deserialize them directly
                // this has the nice benefit that we do not need to do any IO operations as well as do the deserialization while we are in an asynchronous operation aka non-blocking environment (= WIN WIN)
                //WriteFile(result);
                string result = await reader.ReadToEndAsync();
                TicketModels setOfTickets = Newtonsoft.Json.JsonConvert.DeserializeObject<TicketModels>(result);
                if (setOfTickets != null) {
                    ticketModels.AddRange(setOfTickets.data);
                }
            }
        }
        sw.Stop();
        Debug.Log(sw.Elapsed);
        TicketModelsReceivedEvent.Invoke(ticketModels);
    }

    //POST a task data object to task list
    public override void POST(RequestData data) {
        try {
            string requestData = BuildTaskData(data);
            string url = asanaAPISettings.baseURL + asanaAPISettings.taskRoute;

            ConfigureRequest(url, RequestMethods.POST);
            request.Timeout = 5000;

            byte[] dataBytes11 = Encoding.UTF8.GetBytes(requestData);
            using (Stream postStream = request.GetRequestStream()) {
                postStream.Write(dataBytes11, 0, dataBytes11.Length);
            }
            _sr = new StreamReader(request.GetResponse().GetResponseStream());
            response = (HttpWebResponse)request.GetResponse();
            PostScreenshotAsync(response.Headers.GetValues(5)[0], data.screenshot);

            Debug.Log("successfully post new taks to asana project");
        } catch (Exception e) {
            Debug.LogError("An error occured while posting new task: " + e.Message);
        }
    }

    // TODO: asana API is a bit weird so for ticket creation we need to have yet another wrapper class like this
    // this should probably go into its own file so it doesn't convolute this handler class
    public class NewAsanaTicketRequest {
        public class NewTicketData {
            public string name;
            public string notes;
            public string projects;
            public string workspace;
        }

        public NewTicketData data = new NewTicketData();
    }

    //Build task data out of user inputs
    private string BuildTaskData(RequestData data) {
        //TODO: no need to IO here, just use a defined c# model
        // this saves you from:
        // 1. having to load an actual file
        // 2. having to deserialize json
        // 3. no need to do jsonObj string stuff, which can be error prone
        /* OLD CODE:
        string json = File.ReadAllText(asanaAPISettings.pathToTaskTemplate);
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        string projectId = asanaAPISettings.bugId;
        if (data.dataType is DataType.feedback) { projectId = asanaAPISettings.feedbackId; }

        jsonObj["data"]["name"] = data.title;
        jsonObj["data"]["notes"] = data.text;
        jsonObj["data"]["projects"] = projectId;
        jsonObj["data"]["workspace"] = asanaAPISettings.workspaceId;

        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        */

        string projectId = asanaAPISettings.bugId;
        if (data.dataType is DataType.feedback) { projectId = asanaAPISettings.feedbackId; }

        NewAsanaTicketRequest newTicketRequest = new NewAsanaTicketRequest();
        newTicketRequest.data.name = data.title;
        newTicketRequest.data.notes = data.text;
        newTicketRequest.data.projects = projectId;
        newTicketRequest.data.workspace = asanaAPISettings.workspaceId;
        string output = Newtonsoft.Json.JsonConvert.SerializeObject(newTicketRequest, Newtonsoft.Json.Formatting.Indented);
        return output;
    }


    //HttpWebRequest Configuration 
    public void ConfigureRequest(string url, RequestMethods method, bool streamReader = false) {
        request = (HttpWebRequest)WebRequest.Create(url);
        request.CookieContainer = new CookieContainer();
        request.ContentType = contentType;
        request.Method = method.ToString();
        BearerAuthentication();
        if (streamReader) {
            _sr = new StreamReader(request.GetResponse().GetResponseStream());
        }
    }

    //Send Image to Task, use route from response header field location
    public async override void PostScreenshotAsync(string route, Texture2D img) {
        try {
            string url = "https://app.asana.com" + route + "/attachments";

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + asanaAPISettings.token);

            byte[] imageArray = img.EncodeToPNG();
            MultipartFormDataContent form = new MultipartFormDataContent("Upload----");
            form.Add(new ByteArrayContent(imageArray, 0, imageArray.Length), "\"file\"", "\"example.jpg\"");

            HttpResponseMessage response = await httpClient.PostAsync(url, form);

            Debug.Log("successfully post image to: " + route);
        } catch (Exception e) {
            Debug.Log("Cant post image to task. -> " + e.Message);
        }
    }

    #region OAuth and Bearer Auth

    //Basic authentication with bearer token
    private void BearerAuthentication() {
        request.Headers.Add("Authorization", "Bearer " + asanaAPISettings.token);
    }

    //Log out and use PAT to access api TODO: remove access token
    public override void LogOut() {
        asanaAPISettings.token = asanaAPISettings.defaultToken;
    }

    //Open the auth endpoint in the Asana OAtuh grant flow for nativ applications
    public override void LogIn() {
        string url = string.Format("{0}?client_id={1}&response_type={2}&redirect_uri={3}&scope={4}",
            asanaAPISettings.asanaAuthorizationEndpoint, asanaAPISettings.clientId, asanaAPISettings.responseType, asanaAPISettings.redirectUri, asanaAPISettings.scope);
        Application.OpenURL(url);
    }


    //POST request to the toke excachange endpoint 
    public override void TokenExchange(bool isTokenRefresh = false) {
        string requestData = "";
        request = (HttpWebRequest)WebRequest.Create(asanaAPISettings.asanaTokenExchangeEndpoint);
        request.CookieContainer = new CookieContainer();
        request.Method = RequestMethods.POST.ToString();


        if (isTokenRefresh) {
            requestData = "grant_type=refresh_token&" +
              "client_id=" + asanaAPISettings.clientId + "&" +
              "client_secret=" + asanaAPISettings.clientSecret + "&" +
              "redirect_uri=" + asanaAPISettings.redirectUri + "&" +
              "refresh_token=" + _authorizationInfo.refresh_token + "&" +
              "code=" + asanaAPISettings.token;
        } else {
            requestData = "grant_type=authorization_code&" +
            "client_id=" + asanaAPISettings.clientId + "&" +
            "client_secret=" + asanaAPISettings.clientSecret + "&" +
            "redirect_uri=" + asanaAPISettings.redirectUri + "&" +
            "code=" + asanaAPISettings.token;
        }

        byte[] byteArray = Encoding.UTF8.GetBytes(requestData);

        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteArray.Length;

        using var reqStream = request.GetRequestStream();
        reqStream.Write(byteArray, 0, byteArray.Length);

        using var response = request.GetResponse();
        _sr = new StreamReader(response.GetResponseStream());
        string data = _sr.ReadToEnd();

        if (!isTokenRefresh) {
            AuthorizationInfo info = JsonUtility.FromJson<AuthorizationInfo>(data);

            Debug.Log(((HttpWebResponse)response).StatusDescription);
            asanaAPISettings.token = info.access_token;
            _authorizationInfo = info;
            user = info.data;
        }
    }


    #endregion
    /* //TODO: I don't think this is needed, better to deserialize right away and then operate on the deserialized set of data instead of writing the json to disk to just to load it again
    private void WriteFile(string text) {
        string path = Application.dataPath + "/Feedback/Runtime/Resources/AsanaTasks.json";

        if (!File.Exists(path)) {
            using (StreamWriter sw = File.CreateText(path)) {
            }
        } else {
            using (StreamWriter sw = File.AppendText(path)) {
                sw.Write(text);
            }
        }

    }*/

}

#region Response objects 
[System.Serializable]
public class AuthorizationInfo {
    public string access_token;
    public string token_type;
    public string expires_in;
    public AuthorizationUser data;
    public string refresh_token;
    public string id_token;
}


[System.Serializable]
public class AuthorizationUser {
    public string id;
    public string gid;
    public string name;
    public string email;
}

#endregion