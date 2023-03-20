using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Search;
using UnityEngine;

public class TicketBrowser : MonoBehaviour {

    public GameObject layoutGroup;
    public GameObject ticketPreviewPrefab;
    public GameObject tagPanel;

    public LuceneTest lucene;
    public TMP_InputField input;
    public TagList tagList;

    private List<TicketModels.AsanaTicketModel> tickets = new List<TicketModels.AsanaTicketModel>();
    private List<GameObject> ticketsList = new List<GameObject>();

    private string searchString = "";
    private string differ = "";
    private int count = 0;

    private List<TicketModels.AsanaTicketModel> searchResult = new List<TicketModels.AsanaTicketModel>();
    public List<ScriptableTag> usedTags = new List<ScriptableTag>();

    void Awake() {
        AsanaAPI.TicketsReceivedEvent -= OnTicketsReceived;
        AsanaAPI.TicketsReceivedEvent += OnTicketsReceived;
    }

    private void Update() {
        differ = input.text;
        if (searchString != differ) {
            StartCoroutine(Search());
        }
        searchString = differ;
    }

    //Needs to be fired to operate on tickets!
    private void OnTicketsReceived(List<TicketModels.AsanaTicketModel> tickets) {
        this.tickets = tickets;
        Debug.Log("Tickets are there");
        lucene.AddTicketsToIndex(tickets);
    }

    //Remove a tag 
    public void RemoveTag(GameObject tag) {
        ScriptableTag st = tag.GetComponentInChildren<TagPreview>().scriptableTag;
        usedTags.Remove(st);
        Destroy(tag);
    }

    //Search for sarch term each time the string in the inout field was changed 
    private IEnumerator Search() {
        ResetPreview();
        var res = lucene.SearchTerm(searchString);
        searchResult = res.ToList();

        AddTag();
        foreach (TicketModels.AsanaTicketModel ticket in searchResult) {
            FillPreview(ticket.notes, ticket.name);
        }

        Debug.Log(searchResult.ToList().Count);
        yield return new WaitForSeconds(0.5f);
    }

    private void AddTag() {
        foreach (ScriptableTag tag in tagList.tags) {
            if (searchString.ToLower().Contains(tag.tagName.ToLower()) && !usedTags.Contains(tag)) {
                usedTags.Add(tag);
                GameObject tagObj = Instantiate(tag.tagPrefab);
                tagObj.transform.parent = tagPanel.transform;
                tagObj.GetComponentInChildren<TMP_Text>().text = tag.tagName;
                tagObj.GetComponentInChildren<TagPreview>().scriptableTag = tag;
            }
        }
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
