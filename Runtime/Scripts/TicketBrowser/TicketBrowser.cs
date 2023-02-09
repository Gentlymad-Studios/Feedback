using System.Collections.Generic;
using UnityEngine;

public class TicketBrowser : MonoBehaviour {

    public string searchString;
    public int counter = 0;
    public GameObject layoutGroup;
    public GameObject ticketPreviewPrefab;

    private List<AsanaTicketModel> tickets = new List<AsanaTicketModel>();

    void Awake() {
        AsanaAPI.TicketsReceivedEvent -= OnTicketsReceived;
        AsanaAPI.TicketsReceivedEvent += OnTicketsReceived;
    }

    //Needs to be fired to operate on tickets!
    private void OnTicketsReceived(List<AsanaTicketModel> tickets) {
        this.tickets = tickets;
        SearchTicketFromString(searchString);
    }

    private void SearchTicketFromString(string searchString) {
        foreach (AsanaTicketModel ticket in tickets) {
            if (ticket.notes.Contains(searchString) || ticket.name.Contains(searchString)) {
                FillPreview(ticket.notes, ticket.name);
            }
        }
    }

    private void FillPreview(string notes, string name) {
        GameObject newPreviewField = Instantiate(ticketPreviewPrefab);
        newPreviewField.transform.SetParent(layoutGroup.transform);
        TicketPreview preview = newPreviewField.GetComponent<TicketPreview>();
        preview.notes.text = notes;
        preview.ticketName.text = name;
    }
}
