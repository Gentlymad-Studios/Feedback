using System.Collections.Generic;

public class TicketModels {
    public class AsanaTicketModel {
        public string gid { get; set; }
        public string created_at { get; set; }
        public string name { get; set; }
        public string notes { get; set; }

    }

    public class AsanaTicketModels {
        public List<AsanaTicketModel> data;
    }
}