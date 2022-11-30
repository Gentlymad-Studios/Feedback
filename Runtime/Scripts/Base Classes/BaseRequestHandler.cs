using System;
using System.Net;

public abstract class BaseRequestHandler : IRequestHandler {

    protected HttpWebRequest _request;
    protected string _contentType = "application/json; charset=UTF-8";
    public abstract void CreateClientInstance();
    public abstract void POST(API_Data data);
    public abstract void LogIn(Action courutine);

}

public enum RequestMethods {
    GET = 1,
    POST = 2,
}