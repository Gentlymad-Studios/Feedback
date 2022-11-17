using System.Net;
using System.Net.Http;
using UnityEngine;
public class BaseRequestHandler : IRequestHandler {

    protected HttpClient _httpClient;
    protected WebRequest _request;
    public virtual void CreateClientInstance(string baseURL, string token) { }
    public virtual void HandleRequest(string request) { }
}
