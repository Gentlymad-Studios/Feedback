using System.Collections.Generic;

public class TicketModels {
    public class AsanaTaskModel {
        public string gid { get; set; }
        public string created_at { get; set; }
        public List<CustomField> custom_fields { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
    }

    public class AsanaTicketModels {
        public List<AsanaTaskModel> data { get; set; }
    }
    public class CustomField {
        public string gid { get; set; }
        public string name { get; set; }
        public object display_value { get; set; }

    }
    public class ReportTags {
        public string gid { get; set; }
        public List<Tags> enum_options { get; set; }
    }

    public class Tags {
        public string gid { get; set; }
        public string name { get; set; }
    }


}