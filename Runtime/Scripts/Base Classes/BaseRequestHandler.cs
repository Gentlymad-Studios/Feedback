using J2N.Collections.Generic;

namespace Feedback {
    public abstract class BaseRequestHandler : IRequestHandler {
        public bool requestRunning = false;
        public bool logginChange = false;
        public AuthorizationUser User;
        public abstract string UniqueId { get; set; }
        public List<TagPreview> Tags = new List<TagPreview>();

        protected string contentType = "application/json; charset=UTF-8";
        public abstract void GetData(bool force);
        public abstract bool PostNewData(RequestData data);
        public abstract AuthorizationUser GetUser();
        public abstract void AddTagToTagList(TagPreview tag);
        public abstract void RemoveTagFromTagList(TagPreview tag);
        public abstract void LogIn();
        public abstract void LogOut();
        public abstract void AbortLogin();
        public abstract void LoadAvatar();
        public abstract void TryGetUserAsync(string id);
    }

    public enum RequestMethods {
        GET = 1,
        POST = 2,
    }
}