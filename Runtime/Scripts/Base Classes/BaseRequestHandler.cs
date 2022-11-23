using System.Net;
public abstract class BaseRequestHandler : IRequestHandler {

    protected WebRequest _request;
    protected string _contentType = "application/json; charset=UTF-8";
    public abstract void CreateClientInstance();
    public abstract void GET(string route);
    public abstract void POST(API_Data data);

}

public enum RequestMethods {
    GET = 1,
    POST = 2,
}