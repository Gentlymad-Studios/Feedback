using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TicketBrowser : MonoBehaviour {


    [Header("Ticket Browser Components")]
    public TagList tagList;
    public PanelComponents panelComponents;
    public List<ScriptableTag> usedTagsScriptableList = new List<ScriptableTag>();
 
    private int taskPreviewCount = 10;
    private List<GameObject> taskPreviewList = new List<GameObject>();
    private List<TaskModels.AsanaTaskModel> searchResult = new List<TaskModels.AsanaTaskModel>();
    private List<string> mentions = new List<string>();

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
    private void OnTicketsReceived(List<TaskModels.AsanaTaskModel> tickets) {
        Debug.Log("<color=cyan>Tickets are there: </color>" + tickets.Count);

        //change nulls to empty strings
        foreach (TaskModels.AsanaTaskModel ticket in tickets) {
            for (int i = 0; i < ticket.GetType().GetProperties().Length; i++) {
                PropertyInfo pinfo = ticket.GetType().GetProperties()[i];
                if (pinfo.PropertyType == typeof(string)) {
                    if (pinfo.GetValue(ticket) == null) {
                        pinfo.SetValue(ticket, "...");
                    }
                }
            }
        }
        SearchWithLucene.Instance.CreateIndex(tickets);
    }

    //Search for the change text with lucene text analyzer
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
            TaskModels.AsanaTaskModel task = searchResult[i];
            TicketPreview preview = taskPreviewList[i].GetComponent<TicketPreview>();
            preview = preview.SetTicketModel(task);
            preview.mentioned = false;
            
            taskPreviewList[i].SetActive(true);
            string title = task.name;
            string notes = task.notes;
            string gid = task.gid;

            if (uIPopup.mentionedTask.ContainsKey(gid)) {
                preview.mentioned = true;
            }
            preview.openDetailPopup = () => OnClickTicketPreviewAction(preview, title, notes);
            preview.addToMentions = () => AddToMentionList(gid, task);
            preview.removeFromMentions = () => RemoveMentionFromList(gid);
        }
    }

    //Reset the preview objects (hide them and clear the text fields)
    private void ResetPreview() {
        taskPreviewList.ForEach(obj => {
            TicketPreview preview = obj.GetComponent<TicketPreview>();
            preview.ResetTicketModel();
            obj.SetActive(false);
        });
        searchResult.Clear();
    }

    //Instatniate detail popup with ticket preview content
    private void OnClickTicketPreviewAction(TicketPreview preview, string title, string description) {
        if(panelComponents.searchPanel.GetComponentInChildren<DetailPopup>() != null) { return; }
        GameObject popup = Instantiate(panelComponents.detailPopup);
        popup.gameObject.transform.SetParent(panelComponents.searchPanel.transform);
        popup.transform.localPosition = Vector3.zero;
        DetailPopup detailPopup = popup.GetComponent<DetailPopup>();
        detailPopup.FillDetailPopup(title, description);
        detailPopup.onClosePopup += () => OnClickCloseButton(detailPopup, preview);
    }

    private void OnClickCloseButton(DetailPopup popup, TicketPreview preview) {
        preview.openDetailPopup -= () => OnClickTicketPreviewAction(preview, popup.title.text, popup.description.text);
        Destroy(popup.gameObject);
    }

    private void AddToMentionList(string gid, TaskModels.AsanaTaskModel p) {
        mentions.Add(gid);
        uIPopup.mentionedTask.Add(gid, p);
    }

    private void RemoveMentionFromList(string gid) {
        if (mentions.Contains(gid)) {
            mentions.Remove(gid);
        }
        uIPopup.mentionedTask.Remove(gid);
        panelComponents.mentionList.options.Remove(panelComponents.mentionList.options.Find(o => o.text == gid));
    }

}
