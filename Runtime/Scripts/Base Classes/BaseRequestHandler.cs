using J2N.Collections.Generic;

public abstract class BaseRequestHandler : IRequestHandler {
    public bool requestRunning = false;
    public AuthorizationUser User;
    public List<TagPreview> Tags = new List<TagPreview>();
   
    protected string contentType = "application/json; charset=UTF-8";
    public abstract void GetAllData();
    public abstract void PostNewData<T1, T2>(RequestData<T1, T2> data);
    public abstract AuthorizationUser GetUser();
    public abstract void AddTagToTagList(TagPreview tag);
    public abstract void RemoveTagFromTagList(TagPreview tag);
    public abstract void LogIn();
    public abstract void LogOut();
    public abstract void AbortLogin();
    public abstract void LoadAvatar();
}

public enum RequestMethods {
    GET = 1,
    POST = 2,
}