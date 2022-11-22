using System.Net;
using System.Net.Http;

public class BaseRequestHandler : IRequestHandler {

    protected WebRequest _request;
    public virtual void CreateClientInstance(string baseURL, string token) { }
}
