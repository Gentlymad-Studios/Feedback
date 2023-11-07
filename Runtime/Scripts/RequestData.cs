using System.Collections.Generic;

namespace Feedback {
    /// <summary>
    /// Generic data structure. 
    /// </summary>
    public class RequestData<T1, T2> {

        public string Title;
        public string Text;
        public Dictionary<Dictionary<T1, T1>, List<T2>> Attachments;
        public AsanaProject AsanaProject;

        public RequestData(string title, string text, Dictionary<Dictionary<T1, T1>, List<T2>> attachments, AsanaProject asanaProject) {
            Title = title;
            Text = text;
            Attachments = attachments;
            AsanaProject = asanaProject;
        }
    }
}