using Newtonsoft.Json;
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

    public Action OnTokenSubmit;

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

    public override void GET() {
        FetchAsanaTasks();
    }
    private async void SaveAsanaTasksFile(StreamReader reader) {
        if (Resources.Load<TextAsset>("AsnaTasks") == null) {
            string result = await reader.ReadToEndAsync();
            TicketModels.AsanaTicketModels setOfTickets = Newtonsoft.Json.JsonConvert.DeserializeObject<TicketModels.AsanaTicketModels>(result);

        }
    }

    //Get a lot asana task (may not all because there could be more than 100 tasks created in a month).
    //Diagnostic method to measure time and file size to fetch and store up to 2500 tasks from asana api.
    //=> This is just for testing! Using this would assume to use a system to iterate through dates and add new dates.
    private async void FetchAsanaTasks() {
        Stopwatch sp = new Stopwatch();
        sp.Start();
        bool write = false;
        if (Resources.Load<TextAsset>("AsanaTasks") == null) { write = true; }

        List<TicketModels.AsanaTicketModel> ticketModels = new List<TicketModels.AsanaTicketModel>();
        TicketModels.AsanaTicketModels setOfTickets = new TicketModels.AsanaTicketModels();
        string result = "";

        string[] dates = new string[] { "2020-09-01", "2020-10-01", "2020-11-01", "2020-12-01",
            "2021-01-01", "2021-02-01", "2021-03-01", "2021-04-01", "2021-05-01", "2021-06-01", "2021-07-01", "2021-08-01", "2021-09-01", "2021-10-01", "2021-11-01", "2021-12-01",
         "2022-01-01", "2022-02-01", "2022-03-01", "2022-04-01", "2022-05-01", "2022-06-01", "2022-07-01", "2022-08-01", "2022-09-01", "2022-10-01", "2022-11-01", "2022-12-01",
         "2023-01-01", "2023-02-01"};


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

#if UNITY_EDITOR
                if (!write) {
                    result = Resources.Load<TextAsset>("AsanaTasks").ToString();
                    result = result.Replace("]}{\"data\":[", ",");

                    setOfTickets = JsonConvert.DeserializeObject<TicketModels.AsanaTicketModels>(result);
                    if (setOfTickets != null) {
                        ticketModels.AddRange(setOfTickets.data);
                    }
                    asanaAPI.FireTicketsCreated(ticketModels);
                    return;
                }
#endif

                result = await reader.ReadToEndAsync();
#if UNITY_EDITOR
                WriteFile(result);
#endif
                setOfTickets = JsonConvert.DeserializeObject<TicketModels.AsanaTicketModels>(result);
                if (setOfTickets != null) {
                    ticketModels.AddRange(setOfTickets.data);
                }
            }
        }
        sp.Stop();
        Debug.Log(sp.Elapsed + " : all tickets fetched : " + ticketModels.Count);
        asanaAPI.FireTicketsCreated(ticketModels);
    }

    //INFO: just a method for testing.
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

    //Build task data out of user inputs
    private string BuildTaskData(RequestData data) {
        string projectId = asanaAPISettings.bugId;
        if (data.dataType is DataType.Feedback) { projectId = asanaAPISettings.feedbackId; }

        NewAsanaTicketRequest newTicketRequest = new NewAsanaTicketRequest();
        newTicketRequest.data.name = data.title;
        newTicketRequest.data.notes = data.text;
        newTicketRequest.data.projects = projectId;
        newTicketRequest.data.workspace = asanaAPISettings.workspaceId;
        string output = JsonConvert.SerializeObject(newTicketRequest, Formatting.Indented);
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
            string url = asanaAPISettings.URL + route + asanaAPISettings.attachmentRoute;
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

    public override void LogOut() {
        asanaAPISettings.token = asanaAPISettings.defaultToken;
        base.user = null;
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
            AuthorizationInfo info = JsonConvert.DeserializeObject<AuthorizationInfo>(data);
            Debug.Log("Login status code: " + ((HttpWebResponse)response).StatusDescription);
            asanaAPISettings.token = info.access_token;
            _authorizationInfo = info;
            user = info.data;
        }
    }


    #endregion

}

#region Response objects 
public class AuthorizationInfo {
    public string access_token;
    public string token_type;
    public string expires_in;
    public AuthorizationUser data;
    public string refresh_token;
    public string id_token;
}

public class AuthorizationUser {
    public string id;
    public string gid;
    public string name;
    public string email;
}
#endregion
