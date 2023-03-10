using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TicketBrowser : MonoBehaviour {

    public GameObject layoutGroup;
    public GameObject ticketPreviewPrefab;
    public LuceneTest lucene;

    private List<TicketModels.AsanaTicketModel> tickets = new List<TicketModels.AsanaTicketModel>();
    private List<GameObject> ticketsList = new List<GameObject>();

    private string searchString = "";
    private string differ = "";
    private int count = 0;

    private List<TicketModels.AsanaTicketModel> searchResult = new List<TicketModels.AsanaTicketModel>();
    void Awake() {
        AsanaAPI.TicketsReceivedEvent -= OnTicketsReceived;
        AsanaAPI.TicketsReceivedEvent += OnTicketsReceived;
    }

    //Needs to be fired to operate on tickets!
    private void OnTicketsReceived(List<TicketModels.AsanaTicketModel> tickets) {
        this.tickets = tickets;
        Debug.Log("Tickets are there");
        lucene.AddTicketsToIndex(tickets);
    }

    private IEnumerator Search() {
        ResetPreview();
        var res = lucene.SearchTerm(searchString);
        searchResult = res.ToList();

        foreach (TicketModels.AsanaTicketModel ticket in searchResult) {
            FillPreview(ticket.notes, ticket.name);
        }
        Debug.Log(searchResult.ToList().Count);
        yield return new WaitForSeconds(0.5f);
    }
    private void Update() {
        differ = GetComponent<TMP_InputField>().text;
        if (searchString != differ) {
            StartCoroutine(Search());
            Debug.Log("current search string: " + searchString);
        }
        searchString = differ;
    }

    private void FillPreview(string notes, string name) {
        if (count == 10) {
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
