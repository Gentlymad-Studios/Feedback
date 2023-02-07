using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Debug = UnityEngine.Debug;
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
        SearchTicketFromString(searchString);
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
        jsonFile = File.ReadAllText("Assets/Feedback/Runtime/Resources/AsanaTasks.json");
        jsonFile.Replace("]}{\"data\":[", ",").Replace("{\"data\":[{", "[{")
            .Replace("{[", "{").Replace("]}", "}").Replace("}}", "}").Replace("{{", "{");
        if(!jsonFile.EndsWith("]")) {
            jsonFile = jsonFile + "]";
        }
        File.WriteAllText("Assets/Feedback/Runtime/Resources/AsanaTasks.json", jsonFile);
    }
}
