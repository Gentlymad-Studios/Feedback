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
    private AsanaAPI asanaAPI;
    private AsanaAPISettings asanaAPISettings;
    private AuthorizationInfo _authorizationInfo;
    public AsanaRequestHandler(AsanaAPI asanaAPI) {
        asanaAPISettings = asanaAPI.asanaAPISettings;
        Debug.Log(asanaAPISettings.token);
        this.asanaAPI = asanaAPI;
        request = default(HttpWebRequest);
    }

    public Action OnTokenSubmit;

    //POST a task data object to task list
    public override void POST(RequestData data) {
        try {
            string requestData = BuildTaskData(data);
            string url = asanaAPISettings.baseURL + asanaAPISettings.taskRoute;
            Debug.Log(url);
            ConfigureRequest(url, RequestMethods.POST);
            request.Timeout = 5000;

            byte[] dataBytes11 = Encoding.UTF8.GetBytes(requestData);
            using (Stream postStream = request.GetRequestStream()) {
                postStream.Write(dataBytes11, 0, dataBytes11.Length);
            }
            _sr = new StreamReader(request.GetResponse().GetResponseStream());
            Debug.Log("successfully post new taks to asana project");
        } catch (Exception e) {
            Debug.LogError("An error occured while posting new task: " + e.Message);
        }

    }

    //Build task data out of user inputs
    private string BuildTaskData(RequestData data) {
        string json = File.ReadAllText(asanaAPISettings.pathToTaskTemplate);
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        string projectId = asanaAPISettings.bugId;
        if (data.dataType is DataType.feedback) { projectId = asanaAPISettings.feedbackId; }

        jsonObj["data"]["name"] = data.title;
        jsonObj["data"]["notes"] = data.text;
        jsonObj["data"]["projects"] = projectId;
        jsonObj["data"]["workspace"] = asanaAPISettings.workspaceId;

        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        return output;
    }

    public override AuthorizationUser GetUserData() {
        return user;
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

    //Basic authentication with bearer token
    private void BearerAuthentication(string token = null) {
        if (token == null) {
            token = asanaAPISettings.token;
        }
        request.Headers.Add("Authorization", "Bearer " + token);
    }

    #region OAuth
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