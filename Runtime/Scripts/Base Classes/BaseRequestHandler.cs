using J2N.Collections.Generic;
using System;
using System.Net;
using UnityEngine;

public abstract class BaseRequestHandler : IRequestHandler {

    public AuthorizationUser user;
    public List<ScriptableTag> tags = new List<ScriptableTag>();
   
    protected string contentType = "application/json; charset=UTF-8";
    public abstract void GetAllData();
    public abstract void PostNewData(RequestData data);
    public abstract void PostUpvoteCount(string ticketId, int count);
    public abstract AuthorizationUser GetUser();
    public abstract void AddTagToTagList(ScriptableTag tag);
    public abstract void RemoveTagFromTagList(ScriptableTag tag);
    public abstract void LogIn();
    public abstract void LogOut();

}

public enum RequestMethods {
    GET = 1,
    POST = 2,
}