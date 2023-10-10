using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class TicketBrowser {
 
    private int taskPreviewCount = 10;
    private UIPopup uIPopup;
    private DetailPopup detailPopup;
    private List<TicketPreview> taskPreviewList = new List<TicketPreview>();
    private List<TaskModels.AsanaTaskModel> searchResult = new List<TaskModels.AsanaTaskModel>();
    private List<string> mentions = new List<string>();

    public TicketBrowser(UIPopup uIPopup) {
        AsanaAPI.DataReceivedEvent -= OnDataReceived;
        AsanaAPI.DataReceivedEvent += OnDataReceived;

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

    public void InitEvents() {
        if (taskPreviewList != null) {
            for (int i = 0; i < taskPreviewList.Count; i++) {
                taskPreviewList[i].RegisterEvents();
            }
        }

        detailPopup?.RegisterEvents();
        RegisterEvents();
    }

    public void Dispose() {
        if (taskPreviewList != null) {
            for (int i = 0; i < taskPreviewList.Count; i++) {
                taskPreviewList[i].UnregisterEvents();
            }
        }

        detailPopup?.UnregisterEvents();
        UnregisterEvents();
    }

    public void HideDetailCard() {
        detailPopup?.Hide();
    }

    private void RegisterEvents() {
        UnregisterEvents();
        uIPopup.PanelComponents.searchTxtFld.RegisterValueChangedCallback(Search);
    }

    private void UnregisterEvents() {
        uIPopup.PanelComponents.searchTxtFld.UnregisterValueChangedCallback(Search);
    }

    //Needs to be fired to operate on tickets!
    private void OnDataReceived(List<TaskModels.AsanaTaskModel> tasks, TaskModels.ReportTags reportTags) {
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

        Debug.Log("<color=cyan>All ReportTags received: </color>" + reportTags.enum_options.Count);

        uIPopup.SetLoadingStatus(false);
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
            preview.openDetailPopup = () => OnClickTicketPreviewAction(title, notes);
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

    //Instantiate detail popup with ticket preview content
    public void OnClickTicketPreviewAction(string title, string description) {
        if (detailPopup == null) {
            detailPopup = new DetailPopup(uIPopup.TaskDetailCardUi);
        }

        detailPopup.FillDetailPopup(title, description);
        detailPopup.Show();
        uIPopup.PanelComponents.root.Add(detailPopup);
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
