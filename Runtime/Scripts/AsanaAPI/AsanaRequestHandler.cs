using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

/// <summary>
/// Class that creates an asana client instance.
/// </summary>
public class AsanaRequestHandler : BaseRequestHandler {

    private StreamReader _sr;
    private AsanaAPISettings _settings;
    private AuthorizationInfo _authorizationInfo;
    public AsanaRequestHandler(AsanaAPISettings settings) {
        _settings = settings;
        CreateClientInstance();
    }

    public override void CreateClientInstance() {
        _request = default(HttpWebRequest);
    }

    //POST a task data object to task list
    public override void POST(API_Data data) {
        try {
            string requestData = BuildTaskData(data);
            string url = _settings.baseURL + _settings.taskRoute;

            ConfigureRequest(url, RequestMethods.POST);
            _request.Timeout = 5000;

            byte[] dataBytes11 = Encoding.UTF8.GetBytes(requestData);
            using (Stream postStream = _request.GetRequestStream()) {
                postStream.Write(dataBytes11, 0, dataBytes11.Length);
            }
            _sr = new StreamReader(_request.GetResponse().GetResponseStream());
            Debug.Log("successfully post new taks to asana project");
        } catch (Exception e) {
            Debug.LogError("An error occured while posting new task: " + e.Message);
        }

    }

    //Build task data out of user inputs
    private string BuildTaskData(API_Data data) {
        string json = File.ReadAllText(_settings.pathToTaskTemplate);
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        string projectId = _settings.bugId;
        if (data.dataType is DataType.feedback) { projectId = _settings.feedbackId; }

        jsonObj["data"]["name"] = data.title;
        jsonObj["data"]["notes"] = data.text;
        jsonObj["data"]["projects"] = projectId;
        jsonObj["data"]["workspace"] = _settings.workspaceId;

        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        return output;
    }

    public AuthorizationUser GetUserData() {
        return _authorizationInfo.data;

    }
    //Log in and start the asana oauth flow
    public override void LogIn() {
        GetAuthorizationToken();
    }

    //Log out and use PAT to access api TODO: remove access token
    public override void LogOut() {
        _settings.token = _settings.defaultToken;
    }

    //HttpWebRequest Configuration 
    public void ConfigureRequest(string url, RequestMethods method, bool streamReader = false) {
        _request = (HttpWebRequest)WebRequest.Create(url);
        _request.CookieContainer = new CookieContainer();
        _request.ContentType = _contentType;
        _request.Method = method.ToString();
        BearerAuthentication();
        if (streamReader) {
            _sr = new StreamReader(_request.GetResponse().GetResponseStream());
        }
    }

    //Open the auth endpoint in the Asana OAtuh grant flow for nativ applications
    public void OpenAuthorizationEndpoint() {
        string url = string.Format("{0}?client_id={1}&response_type={2}&redirect_uri={3}&scope={4}",
            _settings.asanaAuthorizationEndpoint, _settings.clientId, _settings.responseType, _settings.redirectUri, _settings.scope);
        Application.OpenURL(url);
    }

    //POST request to the toke excachange endpoint 
    private void GetAuthorizationToken() {

        _request = (HttpWebRequest)WebRequest.Create(_settings.asanaTokenExchangeEndpoint);
        _request.CookieContainer = new CookieContainer();
        _request.Method = RequestMethods.POST.ToString();

        string requestData = "grant_type=authorization_code&" +
             "client_id=" + _settings.clientId + "&" +
             "client_secret=" + _settings.clientSecret + "&" +
             "redirect_uri=" + _settings.redirectUri + "&" +
             "code=" + _settings.token;

        byte[] byteArray = Encoding.UTF8.GetBytes(requestData);

        _request.ContentType = "application/x-www-form-urlencoded";
        _request.ContentLength = byteArray.Length;

        using var reqStream = _request.GetRequestStream();
        reqStream.Write(byteArray, 0, byteArray.Length);

        using var response = _request.GetResponse();
        _sr = new StreamReader(response.GetResponseStream());
        string data = _sr.ReadToEnd();
        AuthorizationInfo info = JsonUtility.FromJson<AuthorizationInfo>(data);

        Debug.Log(((HttpWebResponse)response).StatusDescription);
        _settings.token = info.access_token;
        _authorizationInfo = info;
    }

    //Refresh the access token with the refresh token of your user info object
    private void RefreshToken() {
        _request = (HttpWebRequest)WebRequest.Create(_settings.asanaTokenExchangeEndpoint);
        _request.CookieContainer = new CookieContainer();
        _request.Method = RequestMethods.POST.ToString();

        string requestData = "grant_type=refresh_token&" +
             "client_id=" + _settings.clientId + "&" +
             "client_secret=" + _settings.clientSecret + "&" +
             "redirect_uri=" + _settings.redirectUri + "&" +
             "refresh_token=" + _authorizationInfo.refresh_token + "&" +
             "code=" + _settings.token;

        byte[] byteArray = Encoding.UTF8.GetBytes(requestData);

        _request.ContentType = "application/x-www-form-urlencoded";
        _request.ContentLength = byteArray.Length;

        using var reqStream = _request.GetRequestStream();
        reqStream.Write(byteArray, 0, byteArray.Length);

        using var response = _request.GetResponse();
        _sr = new StreamReader(response.GetResponseStream());
        string data = _sr.ReadToEnd();
    }

    //Basic authentication with bearer token
    private void BearerAuthentication(string token = null) {
        if (token == null) {
            token = _settings.token;
        }
        _request.Headers.Add("Authorization", "Bearer " + token);
    }

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