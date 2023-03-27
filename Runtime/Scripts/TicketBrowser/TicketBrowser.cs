using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TicketBrowser : MonoBehaviour {

    public GameObject layoutGroup;
    public GameObject ticketPreviewPrefab;
    public GameObject tagPanel;

    public LuceneTest lucene;
    public TMP_InputField input;
    public TagList tagList;

    private string searchString = "";
    private string differ = "";
    private int count = 0;
    private int ticketPreviewCount = 10;


    private List<TicketModels.AsanaTicketModel> tickets = new List<TicketModels.AsanaTicketModel>();
    private List<GameObject> ticketsList = new List<GameObject>();

    private List<TicketModels.AsanaTicketModel> searchResult = new List<TicketModels.AsanaTicketModel>();
    public List<ScriptableTag> usedTags = new List<ScriptableTag>();

    private void Start() {
        //instatiate all preview objects on start and hide them
        for (int i = 0; i < ticketPreviewCount; i++) {
            GameObject ticketPreview = Instantiate(ticketPreviewPrefab);
            ticketPreview.transform.SetParent(layoutGroup.transform);
            ticketPreview.SetActive(false);
            ticketsList.Add(ticketPreview);
        }

    }

    void Awake() {
        AsanaAPI.TicketsReceivedEvent -= OnTicketsReceived;
        AsanaAPI.TicketsReceivedEvent += OnTicketsReceived;
    }

    private void Update() {

        differ = input.text;
        Debug.Log(usedTags.Count);
        if (input.text == " " || input.text.Equals(string.Empty)) {
            ResetPreview();
            return;
        }

        if (differ != searchString && searchString != " ") {
            searchString = differ;
            AddTag();
            StartCoroutine(Search());
        }
    }

    //private void OnDisable() {
    //    lucene.Dispose();
    //}

    //Needs to be fired to operate on tickets!
    private void OnTicketsReceived(List<TicketModels.AsanaTicketModel> tickets) {
        this.tickets = tickets;
        Debug.Log("Tickets are there");
        lucene.AddTicketsToIndex(tickets);
    }

    //Search for sarch term each time the string in the input field changed 
    private IEnumerator Search() {
        ResetPreview();
        searchResult = lucene.SearchTerm(searchString).ToList();
        FillPreview();
        yield return new WaitForSeconds(0.5f);
    }

    public void AddTag() {
        foreach (ScriptableTag tag in tagList.tags) {
            if (searchString.ToLower().Contains(tag.tagName.ToLower()) && !usedTags.Contains(tag)) {

                GameObject tagObj = Instantiate(tag.tagPrefab);
                tagObj.transform.SetParent(tagPanel.transform);
                tagObj.GetComponentInChildren<TMP_Text>().text = tag.tagName;

                TagPreview tagPreview = tagObj.GetComponentInChildren<TagPreview>();
                tagPreview.scriptableTag = tag;
                tagPreview.deleteAction = new Action<ScriptableTag>((tag) => { usedTags.Remove(tag); });

                usedTags.Add(tag);
                Debug.Log(usedTags.Count);
            }
        }
    }

    //fill the preview with lucene search results
    private void FillPreview() {
        for (int i = 0; i < searchResult.Count; i++) {
            TicketPreview preview = ticketsList[i].GetComponent<TicketPreview>();
            preview.ticketName.text = searchResult[i]?.name;
            preview.notes.text = searchResult[i]?.notes;
            ticketsList[i].SetActive(true);
        }
    }

    //Reset the preview objects (hide them and clear the text fields)
    private void ResetPreview() {
        ticketsList.ForEach(obj => {
            TicketPreview preview = obj.GetComponent<TicketPreview>();
            preview.notes.text = "";
            preview.ticketName.text = "";
            obj.SetActive(false);
        });
    }
}
