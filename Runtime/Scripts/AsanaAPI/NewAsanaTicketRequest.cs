using System.Collections.Generic;

public class NewAsanaTicketRequest {
    public class NewTicketData {
        public string name { get; set; }
        public string notes { get; set; }
        public string projects { get; set; }
        public string workspace { get; set; }
        public Attachment[] attachments { get; set; }
        public string html_notes { get; set; } 

        public List<string> custom_fields = new List<string>();

    }
    public class Attachment {
        public string filename { get; set; }
        public string contentType { get; set; }
        public string content { get; set; }
    }

}

