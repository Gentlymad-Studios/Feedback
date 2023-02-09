using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAsanaTicketRequest {
    public class NewTicketData {
        public string name;
        public string notes;
        public string projects;
        public string workspace;
    }

    public NewTicketData data = new NewTicketData();
}

