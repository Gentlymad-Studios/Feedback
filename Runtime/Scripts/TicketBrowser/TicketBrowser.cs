using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using File = System.IO.File;

public class TicketBrowser : MonoBehaviour {

    public string searchString;
    public int counter = 0;
    public GameObject layoutGroup;
    public GameObject ticketPreviewPrefab;

    private string jsonFile;
    private List<TicketModel> ticketModels = new List<TicketModel>();

    void Start() {
        FormatFile();
       
    }

    private void SearchTicketFromString(string searchString) {
        ticketModels = JsonConvert.DeserializeObject<List<TicketModel>>(jsonFile);
        foreach (TicketModel ticket in ticketModels) {
            if (ticket.notes.Contains(searchString) || ticket.name.Contains(searchString)) {
                FillPreiew(ticket.notes, ticket.name);
            }
        }
    }

    private void Search() {
        
    }

    private void FillPreiew(string notes, string name) {
        GameObject newPreviewField = Instantiate(ticketPreviewPrefab);
        newPreviewField.transform.parent = layoutGroup.transform;
        TicketPreview preview = newPreviewField.GetComponent<TicketPreview>();
        preview.notes.text = notes;
        preview.ticketName.text = name;
    }
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
    }
}
