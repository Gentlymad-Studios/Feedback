using System;
using System.Net;

public abstract class BaseRequestHandler : IRequestHandler {

    public AuthorizationUser user;


    protected HttpWebRequest request;
    protected string contentType = "application/json; charset=UTF-8";
    public abstract void POST(RequestData data);
    public abstract void LogIn();
    public abstract void LogOut();
    public abstract AuthorizationUser GetUserData();
    public abstract void TokenExchange(bool isRefresh = false);

}

public enum RequestMethods {
    GET = 1,
    POST = 2,
}