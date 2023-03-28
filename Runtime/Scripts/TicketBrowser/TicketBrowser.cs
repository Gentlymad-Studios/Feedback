using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TicketBrowser : MonoBehaviour {


    [Header("Ticket Browser Components")]
    public LuceneTest lucene;
    public TagList tagList;
    public PanelComponents panelComponents;
    public List<ScriptableTag> usedTagsScriptableList = new List<ScriptableTag>();

    private int ticketPreviewCount = 10;
    private List<GameObject> tagsGameObjectsList = new List<GameObject>();
    private List<GameObject> ticketsList = new List<GameObject>();
    private List<TicketModels.AsanaTicketModel> searchResult = new List<TicketModels.AsanaTicketModel>();

    private void Start() {
        //pool all preview objects on start and hide them
        for (int i = 0; i < ticketPreviewCount; i++) {
            GameObject ticketPreview = Instantiate(panelComponents.ticketPreviewPrefab);
            ticketPreview.transform.SetParent(panelComponents.scrollviewPreviewLayoutGroup.transform);
            ticketPreview.SetActive(false);
            ticketsList.Add(ticketPreview);
        }

        RegisterEvents();
    }

    void Awake() {
        AsanaAPI.TicketsReceivedEvent -= OnTicketsReceived;
        AsanaAPI.TicketsReceivedEvent += OnTicketsReceived;
    }

    private void RegisterEvents() {
        panelComponents.searchInput.onValueChanged.AddListener(Search);
    }

    //Needs to be fired to operate on tickets!
    private void OnTicketsReceived(List<TicketModels.AsanaTicketModel> tickets) {
        Debug.Log("<color=cyan>Tickets are there.</color>");
        lucene.AddTicketsToIndex(tickets);
    }

    private void Search(string change) {
        if (string.IsNullOrEmpty(change) || string.IsNullOrWhiteSpace(change)) {
            if (!ticketsList[0].GetComponent<TicketPreview>().ticketName.text.Equals(string.Empty)) {
                ResetPreview();
            }
            return;
        }

        searchResult = lucene.SearchTerm(change).ToList();
        FillPreview();
        ManageTags(change);
    }


    public void ManageTags(string change) {
        var text = change.ToLower();

        foreach (ScriptableTag tag in tagList.tags) {
            if (usedTagsScriptableList.Contains(tag)) {
                continue;
            }

            if (text.Contains(tag.tagName.ToLower())){

                GameObject tagObj = Instantiate(tag.tagPrefab);
                tagObj.transform.SetParent(panelComponents.tagPanel.transform);
                tagObj.GetComponentInChildren<TMP_Text>().text = tag.tagName;
                tagObj.name = tag.tagName.ToLower();
                tagsGameObjectsList.Add(tagObj);

                TagPreview tagPreview = tagObj.GetComponentInChildren<TagPreview>();
                tagPreview.scriptableTag = tag;
                tagPreview.deleteAction = new Action<ScriptableTag>((tag) => {
                    usedTagsScriptableList.Remove(tag);
                    tagsGameObjectsList.Remove(tagObj);
                });

                usedTagsScriptableList.Add(tag);
            }
        }

        for (int i = 0; i < usedTagsScriptableList.Count; i++) {
            if (!usedTagsScriptableList.Contains(usedTagsScriptableList[i])) {
                continue;
            }
            if (!text.Contains(usedTagsScriptableList[i].tagName.ToLower())) {
                var obj = tagsGameObjectsList.Find(tag => tag.name.Contains(usedTagsScriptableList[i].tagName.ToLower()));
                usedTagsScriptableList.Remove(usedTagsScriptableList[i]);
                tagsGameObjectsList.Remove(obj);
                Destroy(obj);

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
        Debug.Log("reset preview list");
    }
}
