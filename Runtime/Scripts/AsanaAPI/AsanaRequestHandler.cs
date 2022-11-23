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
    public AsanaRequestHandler(AsanaAPISettings settings) {
        CreateClientInstance();
        this._settings = settings;
    }

    public override void CreateClientInstance() {
        _request = default(HttpWebRequest);
    }

    public override void GET(string route) {
        _request = (HttpWebRequest)WebRequest.Create(route);
        _request.Method = RequestMethods.GET.ToString();
        _request = Authorization((HttpWebRequest)_request);
        _request.ContentType = _contentType;
        _sr = new StreamReader(_request.GetResponse().GetResponseStream());
        Debug.Log(_sr.ReadToEnd());
    }

    public override void POST(API_Data data) {

        string requestData = BuildTaskData(data);
        _request = (HttpWebRequest)WebRequest.Create(_settings.baseURL + _settings.taskRoute);
        _request.ContentType = _contentType;
        _request = Authorization((HttpWebRequest)_request);
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

    public HttpWebRequest Authorization(HttpWebRequest req) {
        string authInfo = _settings.token + Convert.ToString(":");
        req.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(authInfo)));
        return req;
    }

    private string BuildTaskData(API_Data data) {
        string projectId = "";

        string json = File.ReadAllText(_settings.pathToTaskTemplate);
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

        projectId = _settings.bugId;
        if(data.dataType is DataType.feedback) { projectId = _settings.feedbackId; }

        jsonObj["data"]["name"] = data.title;
        jsonObj["data"]["notes"] = data.text;
        jsonObj["data"]["projects"] = projectId;
        jsonObj["data"]["workspace"] = _settings.workspaceId;

        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        return output;
    }

}
