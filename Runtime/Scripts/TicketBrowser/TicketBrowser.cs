using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class TicketBrowser {
 
    private int taskPreviewCount = 10;
    private UIPopup uIPopup;
    private List<TicketPreview> taskPreviewList = new List<TicketPreview>();
    private List<TaskModels.AsanaTaskModel> searchResult = new List<TaskModels.AsanaTaskModel>();
    private List<string> mentions = new List<string>();

    public TicketBrowser(UIPopup uIPopup) {
        AsanaAPI.TasksReceivedEvent -= OnTasksReceived;
        AsanaAPI.TasksReceivedEvent += OnTasksReceived;

        this.uIPopup = uIPopup;

        uIPopup.PanelComponents.taskContainer.Clear();

        //pool all preview objects on start and hide them
        for (int i = 0; i < taskPreviewCount; i++) {
            //create instance of an taskCard
            VisualElement taskCard = uIPopup.TaskCardUi.Instantiate();
            //hide it
            taskCard.style.display = DisplayStyle.None;
            //add to ui
            uIPopup.PanelComponents.taskContainer.Add(taskCard);

            //create ticketPreview and link ui to it
            TicketPreview taskPreview = new TicketPreview(taskCard);
            //add to tasklist
            taskPreviewList.Add(taskPreview);
            //add events
            taskPreview.sendUpvoteAction = uIPopup.Api.RequestHandler.PostUpvoteCount;
        }

        RegisterEvents();
    }

    private void RegisterEvents() {
        uIPopup.PanelComponents.searchTxtFld.UnregisterValueChangedCallback(Search);
        uIPopup.PanelComponents.searchTxtFld.RegisterValueChangedCallback(Search);
    }

    //Needs to be fired to operate on tickets!
    private void OnTasksReceived(List<TaskModels.AsanaTaskModel> tasks) {
        Debug.Log("<color=cyan>All Tasks received: </color>" + tasks.Count);

        //change nulls to empty strings
        foreach (TaskModels.AsanaTaskModel task in tasks) {
            for (int i = 0; i < task.GetType().GetProperties().Length; i++) {
                PropertyInfo pinfo = task.GetType().GetProperties()[i];
                if (pinfo.PropertyType == typeof(string)) {
                    if (pinfo.GetValue(task) == null) {
                        pinfo.SetValue(task, "...");
                    }
                }
            }
        }

        SearchWithLucene.Instance.CreateIndex(tasks);
    }

    //Search for the change text with lucene text analyzer
    private void Search(ChangeEvent<string> evt) {
        if (string.IsNullOrEmpty(evt.newValue) || string.IsNullOrWhiteSpace(evt.newValue)) {
            if (!taskPreviewList[0].PreviewEmpty()) {
                ResetPreview();
            }
            return;
        }

        searchResult = SearchWithLucene.Instance.SearchTerm(evt.newValue).ToList();
        FillPreview();
    }

    //fill the preview with lucene search results
    private void FillPreview() {
        for (int i = 0; i < searchResult.Count; i++) {
            TaskModels.AsanaTaskModel task = searchResult[i];
            TicketPreview preview = taskPreviewList[i];
            preview = preview.SetTicketModel(task);
            preview.mentioned = false;
            
            taskPreviewList[i].SetActive(true);
            string title = task.name;
            string notes = task.notes;
            string gid = task.gid;

            if (uIPopup.MentionedTask.ContainsKey(gid)) {
                preview.mentioned = true;
            }
            preview.openDetailPopup = () => OnClickTicketPreviewAction(preview, title, notes);
            preview.addToMentions = () => AddToMentionList(gid, task);
            preview.removeFromMentions = () => RemoveMentionFromList(gid);
        }
    }

    //Reset the preview objects (hide them and clear the text fields)
    private void ResetPreview() {
        taskPreviewList.ForEach(preview => {
            preview.ResetTicketModel();
            preview.SetActive(false);
        });
        searchResult.Clear();
    }

    //Instatniate detail popup with ticket preview content
    private void OnClickTicketPreviewAction(TicketPreview preview, string title, string description) {
        //if(panelComponents.searchPanel.GetComponentInChildren<DetailPopup>() != null) { return; }
        //GameObject popup = Instantiate(panelComponents.detailPopup);
        //popup.gameObject.transform.SetParent(panelComponents.searchPanel.transform);
        //popup.transform.localPosition = Vector3.zero;
        //DetailPopup detailPopup = popup.GetComponent<DetailPopup>();
        //detailPopup.FillDetailPopup(title, description);
        //detailPopup.onClosePopup += () => OnClickCloseButton(detailPopup, preview);
    }

    private void OnClickCloseButton(DetailPopup popup, TicketPreview preview) {
        preview.openDetailPopup -= () => OnClickTicketPreviewAction(preview, popup.title.text, popup.description.text);
        //Close the popup
    }

    private void AddToMentionList(string gid, TaskModels.AsanaTaskModel p) {
        mentions.Add(gid);
        uIPopup.MentionedTask.Add(gid, p);
    }

    private void RemoveMentionFromList(string gid) {
        if (mentions.Contains(gid)) {
            mentions.Remove(gid);
        }
        uIPopup.MentionedTask.Remove(gid);
        uIPopup.PanelComponents.taskMentionsDrpDwn.choices.Remove(gid);
    }

}
