using Codice.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TicketBrowser : MonoBehaviour {

    public GameObject layoutGroup;
    public GameObject ticketPreviewPrefab;

    private List<AsanaTicketModel> tickets = new List<AsanaTicketModel>();
    private List<GameObject> ticketsList = new List<GameObject>();

    private string searchString = "";
    private string differ = "";
    private int count = 0;
    void Awake() {
        AsanaAPI.TicketsReceivedEvent -= OnTicketsReceived;
        AsanaAPI.TicketsReceivedEvent += OnTicketsReceived;
    }

    //Needs to be fired to operate on tickets!
    private void OnTicketsReceived(List<AsanaTicketModel> tickets) {
        this.tickets = tickets;
        SearchTicketFromString(searchString);
    }

    private IEnumerator Search() {
        ResetPreview();
        SearchTicketFromString(searchString);
        yield return new WaitForSeconds(0.5f);
    }
    private void Update() {
        differ = GetComponent<TMP_InputField>().text;
        if(searchString != differ) {
            StartCoroutine(Search());
            
            Debug.Log("current search string: " + searchString);
        }
        searchString = differ;
    }

    private void SearchTicketFromString(string searchString) {
        foreach (AsanaTicketModel ticket in tickets) {
            if (ticket.name.Contains(searchString)) {
                FillPreview(ticket.notes, ticket.name);
            }
            //if (ticket.notes.Contains(searchString) || ticket.name.Contains(searchString)) {
            //    FillPreview(ticket.notes, ticket.name);
            //}
        }
    }

    private void FillPreview(string notes, string name) {
        if(count == 10) {
            count = 0;
        }

        GameObject newPreviewField;

        if (ticketsList.Count < 10) {
            newPreviewField = Instantiate(ticketPreviewPrefab);
            newPreviewField.transform.SetParent(layoutGroup.transform);
            ticketsList.Add(newPreviewField);
        } else {
            newPreviewField = ticketsList[count];
            count++;
        }
 
        TicketPreview preview = newPreviewField.GetComponent<TicketPreview>();
        preview.notes.text = notes;
        preview.ticketName.text = name;
    }

    private void ResetPreview() {
        ticketsList.ForEach(obj => {
            TicketPreview preview = obj.GetComponent<TicketPreview>();
            preview.notes.text = "";
            preview.ticketName.text = "";
        });
    }
}
