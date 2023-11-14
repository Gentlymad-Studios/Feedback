using System.Collections.Generic;

namespace Feedback {
    /// <summary>
    /// Generic data structure. 
    /// </summary>
    public class RequestData {

        public string Title;
        public string Text;
        public List<AsanaTicketRequest.Attachment> Attachments;
        public AsanaProject AsanaProject;

        public RequestData(string title, string text, List<AsanaTicketRequest.Attachment> attachments, AsanaProject asanaProject) {
            Title = title;
            Text = text;
            Attachments = attachments;
            AsanaProject = asanaProject;
        }
    }
}