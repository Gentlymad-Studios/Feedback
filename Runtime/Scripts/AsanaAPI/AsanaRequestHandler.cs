using System;
using System.Collections;
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
    private Cookie _tokenCookie;
    private Action _startAsanaAuthFlow;

    private string redirectURL = "";
    private string userData = "";

    public AsanaRequestHandler(AsanaAPISettings settings) {
        CreateClientInstance();
        _settings = settings;
    }

    public override void CreateClientInstance() {
        _request = default(HttpWebRequest);
    }
    public override void POST(API_Data data) {

        string requestData = BuildTaskData(data);
        _request = (HttpWebRequest)WebRequest.Create(_settings.baseURL + _settings.taskRoute);
        _request.ContentType = _contentType;
        //_request = Authorization((HttpWebRequest)_request);
        _request.Method = RequestMethods.POST.ToString(); ;
        _request.Timeout = 5000;

        byte[] dataBytes11 = Encoding.UTF8.GetBytes(requestData);
        using (Stream postStream = _request.GetRequestStream()) {
            postStream.Write(dataBytes11, 0, dataBytes11.Length);
        }

        HttpWebResponse httpresponse21 = _request.GetResponse() as HttpWebResponse;
        _sr = new StreamReader(httpresponse21.GetResponseStream());
        Debug.Log(_sr.ReadToEnd());
    }
    private string BuildTaskData(API_Data data) {
        string projectId = "";

        string json = File.ReadAllText(_settings.pathToTaskTemplate);
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        projectId = _settings.bugId;
        if (data.dataType is DataType.feedback) { projectId = _settings.feedbackId; }

        jsonObj["data"]["name"] = data.title;
        jsonObj["data"]["notes"] = data.text;
        jsonObj["data"]["projects"] = projectId;
        jsonObj["data"]["workspace"] = _settings.workspaceId;

        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        return output;
    }

    public override void LogIn(Action startAsanaAuthFlow) {
        this._startAsanaAuthFlow = startAsanaAuthFlow;
        startAsanaAuthFlow.Invoke();
    }


    //Asana Authentication Flow 
    public IEnumerator AsanaOAuth() {
        if (redirectURL.Equals(string.Empty)) {
            ConfigureRequest(_settings.ServiceEndPoint, RequestMethods.GET);
            redirectURL = _sr.ReadToEnd();
            _startAsanaAuthFlow.Invoke();
        } else {
            ConfigureRequest(redirectURL, RequestMethods.GET);
            Application.OpenURL(redirectURL);
            while (userData.Contains("html") || userData.Contains(redirectURL) || userData.Equals(string.Empty)) {

                //get generated token cookie
                ConfigureRequest(_settings.ServiceCookieEndpoint, RequestMethods.GET);
                foreach(Cookie cookie in _request.CookieContainer.GetCookies(_request.RequestUri)) {
                    if (cookie.Name.Equals("token")) {
                        _tokenCookie = cookie;
                        _settings.token = _tokenCookie.Value;
                    }
                }
                yield return new WaitForSeconds(1);

                //get user data with access token from cookie
                ConfigureRequest(_settings.ServiceEndPoint, RequestMethods.GET, _tokenCookie);
                userData = _sr.ReadToEnd();
                Debug.Log(userData + " successfully finished authorization flow");
                yield return new WaitForSeconds(1);
            }
            _sr.Close();
        }
    }

    //HttpWebRequest Configuration 
    public void ConfigureRequest(string url, RequestMethods method) {
        _request = (HttpWebRequest)WebRequest.Create(url);
        _request.CookieContainer = new CookieContainer();
        _request.ContentType = _contentType;
        _request.Method = method.ToString();
        _sr = new StreamReader(_request.GetResponse().GetResponseStream());
    }

    //HttpWebRequest Configuration with Cookie
    public void ConfigureRequest(string url, RequestMethods method, Cookie cookie) {
        _request = (HttpWebRequest)WebRequest.Create(url);
        _request.CookieContainer = new CookieContainer();
        _request.ContentType = _contentType;
        _request.CookieContainer.Add(_request.RequestUri, cookie);
        _request.Method = method.ToString();
        _sr = new StreamReader(_request.GetResponse().GetResponseStream());
    }
}



