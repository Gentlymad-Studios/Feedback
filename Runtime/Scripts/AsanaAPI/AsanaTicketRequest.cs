using System.Collections.Generic;

namespace Feedback {
    public class AsanaTicketRequest {
        public static class ContentTypes {
            public const string Image = "image/jpeg";
            public const string Text = "text/plain";
            public const string Zip = "application/zip";
            public const string Octet = "application/octet-stream";
        }

        public class TicketData {
            public string name {
                get; set;
            }
            public string notes {
                get; set;
            }
            public string projects {
                get; set;
            }
            public string workspace {
                get; set;
            }
            public Attachment[] attachments {
                get; set;
            }
            public string html_notes {
                get; set;
            }

            public Dictionary<string, List<string>> custom_fields = new Dictionary<string, List<string>>();

        }
        public class Attachment {
            public string filename {
                get; set;
            }
            public string contentType {
                get; set;
            }
            public string content {
                get; set;
            }
        }
    }
}
