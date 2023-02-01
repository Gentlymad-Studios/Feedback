using System;
using System.Net;
using UnityEngine;

public abstract class BaseRequestHandler : IRequestHandler {

    public AuthorizationUser user;
   
    protected string contentType = "application/json; charset=UTF-8";
    public abstract void POST(RequestData data);
    public abstract void LogIn();
    public abstract void LogOut();
    public abstract void TokenExchange(bool isRefresh = false);
    public abstract void PostScreenshotAsync(string url, Texture2D img);

}

public enum RequestMethods {
    GET = 1,
    POST = 2,
}