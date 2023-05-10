using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicketBrowser : MonoBehaviour {


    [Header("Ticket Browser Components")]
    public TagList tagList;
    public PanelComponents panelComponents;
    public List<ScriptableTag> usedTagsScriptableList = new List<ScriptableTag>();
 
    private int taskPreviewCount = 10;
    private List<GameObject> taskPreviewList = new List<GameObject>();
   
    private List<TicketModels.AsanaTaskModel> searchResult = new List<TicketModels.AsanaTaskModel>();

    private UIPopup uIPopup;
    private void Start() {
        uIPopup = panelComponents.GetComponentInParent<UIPopup>();
        //pool all preview objects on start and hide them
        for (int i = 0; i < taskPreviewCount; i++) {
            GameObject ticketPreview = Instantiate(panelComponents.ticketPreviewPrefab);
            ticketPreview.transform.SetParent(panelComponents.scrollviewPreviewLayoutGroup.transform);
            TicketPreview p = ticketPreview.GetComponent<TicketPreview>();
            p.sendUpvoteAction = uIPopup.api.requestHandler.PostUpvoteCount;
            ticketPreview.transform.localScale = Vector3.one;
            ticketPreview.SetActive(false);
            taskPreviewList.Add(ticketPreview);
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
    private void OnTicketsReceived(List<TicketModels.AsanaTaskModel> tickets) {
        Debug.Log("<color=cyan>Tickets are there: </color>" + tickets.Count);

        //change nulls to empty strings
        foreach (TicketModels.AsanaTaskModel ticket in tickets) {
            for (int i = 0; i < ticket.GetType().GetProperties().Length; i++) {
                PropertyInfo pinfo = ticket.GetType().GetProperties()[i];
                if (pinfo.PropertyType == typeof(string)) {
                    if (pinfo.GetValue(ticket) == null) {
                        pinfo.SetValue(ticket, "");
                    }
                }
            }
        }
        SearchWithLucene.Instance.CreateIndex(tickets);
    }

    private void Search(string change) {
        if (string.IsNullOrEmpty(change) || string.IsNullOrWhiteSpace(change)) {
            if (!taskPreviewList[0].GetComponent<TicketPreview>().PreviewEmpty()) {
                ResetPreview();
            }
            return;
        }

        searchResult = SearchWithLucene.Instance.SearchTerm(change).ToList();
        FillPreview();
    }


    //fill the preview with lucene search results
    private void FillPreview() {
        for (int i = 0; i < searchResult.Count; i++) {
            TicketPreview preview = taskPreviewList[i].GetComponent<TicketPreview>();
            preview.SetTicketModel(searchResult[i]);
            taskPreviewList[i].SetActive(true);
        }
    }

    //Reset the preview objects (hide them and clear the text fields)
    private void ResetPreview() {
        taskPreviewList.ForEach(obj => {
            TicketPreview preview = obj.GetComponent<TicketPreview>();
            preview.ResetTicketModel();
            obj.SetActive(false);
        });
    }
}
