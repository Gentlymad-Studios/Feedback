using J2N.Collections.Generic;

public abstract class BaseRequestHandler : IRequestHandler {

    public AuthorizationUser User;
    public List<TagPreview> Tags = new List<TagPreview>();
   
    protected string contentType = "application/json; charset=UTF-8";
    public abstract void GetAllData();
    public abstract void PostNewData(RequestData data);
    public abstract void PostUpvoteCount(string id, int count);
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