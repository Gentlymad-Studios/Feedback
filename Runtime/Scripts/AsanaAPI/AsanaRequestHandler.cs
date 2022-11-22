using System;
using System.IO;
using System.Net;
using System.Text;

/// <summary>
/// Class that creates an asana client instance.
/// </summary>
public class AsanaRequestHandler : BaseRequestHandler {

    public AsanaRequestHandler(APISettings settings) {
        CreateClientInstance(settings.baseURL, settings.token);
    }

    public override void CreateClientInstance(string baseURL, string token) {

        string authInfo = token + Convert.ToString(":");

        _request = default(HttpWebRequest);

        _request = (HttpWebRequest)WebRequest.Create(baseURL + "/users");
        _request.Method = "GET";
        _request.ContentType = "application/json";

        _request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(authInfo)));
        
        StreamReader sr = new StreamReader(_request.GetResponse().GetResponseStream());
        
    }
}
