using J2N.Collections.Generic;
using System;
using System.Net;
using UnityEngine;

public abstract class BaseRequestHandler : IRequestHandler {

    public AuthorizationUser user;
    public List<TagPreview> tags = new List<TagPreview>();
   
    protected string contentType = "application/json; charset=UTF-8";
    public abstract void GetAllData();
    public abstract void PostNewData(RequestData data);
    public abstract void PostUpvoteCount(string ticketId, int count);
    public abstract AuthorizationUser GetUser();
    public abstract void AddTagToTagList(TagPreview tag);
    public abstract void RemoveTagFromTagList(TagPreview tag);
    public abstract void LogIn();
    public abstract void LogOut();

}

public enum RequestMethods {
    GET = 1,
    POST = 2,
}