using System.Collections.Generic;
using UnityEngine;

public class TicketBrowser : MonoBehaviour {

    public string searchString;
    public int counter = 0;
    public GameObject layoutGroup;
    public GameObject ticketPreviewPrefab;

    //private string jsonFile;
    private List<TicketModel> tickets = new List<TicketModel>();

    void Awake() {
        //FormatFile();
        //TODO: unsubscribe from any event (safety measure)
        AsanaAPI.TicketsReceivedEvent -= OnTicketsReceived;
        //TODO: subscribe to TicketsReceivedEvent event to get notified when the api did receive tickets
        AsanaAPI.TicketsReceivedEvent += OnTicketsReceived;
    }

    private void OnTicketsReceived(List<TicketModel> tickets) {
        this.tickets = tickets;
        //TODO: this is only a demo so this should go whereever this makes sense, just keep in mind that we can only be sure that we have received tickets after this event has fired.
        SearchTicketFromString(searchString);
    }

    private void SearchTicketFromString(string searchString) {
        //ticketModels = JsonConvert.DeserializeObject<List<TicketModel>>(jsonFile);
        foreach (TicketModel ticket in tickets) {
            if (ticket.notes.Contains(searchString) || ticket.name.Contains(searchString)) {
                Debug.Log(ticket.notes);
                FillPreview(ticket.notes, ticket.name);
            }
        }
    }

    private void Search() {
        
    }

    private void FillPreview(string notes, string name) {
        GameObject newPreviewField = Instantiate(ticketPreviewPrefab);
        newPreviewField.transform.parent = layoutGroup.transform;
        TicketPreview preview = newPreviewField.GetComponent<TicketPreview>();
        preview.notes.text = notes;
        preview.ticketName.text = name;
    }
    /* TODO: I think we should operate directly on the data instead of doing the extra steps of get response > save .json to disk > load again and deserialize
    public void FormatFile() {
        if(Resources.Load<TextAsset>("AsanaTasks")== null) {
            return;
        }
        jsonFile = Resources.Load<TextAsset>("AsanaTasks").ToString();
        jsonFile = jsonFile.Replace("]}{\"data\":[", ",");
        jsonFile = jsonFile.Replace("{\"data\":[{", "[{");
        jsonFile = jsonFile.Replace("{[", "{");
        jsonFile = jsonFile.Replace("]}", "}");
        jsonFile = jsonFile.Replace("}}", "}");
        jsonFile = jsonFile.Replace("{{", "{");
        jsonFile = jsonFile.Replace("][", ",");

        if (!jsonFile.EndsWith("]")) {
            jsonFile = jsonFile + "]";
        }

        File.WriteAllText("Assets/Feedback/Runtime/Resources/AsanaTasks.json", jsonFile);
        SearchTicketFromString(searchString);
    }*/
}
