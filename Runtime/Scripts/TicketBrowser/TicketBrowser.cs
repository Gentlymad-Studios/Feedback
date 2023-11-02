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
            TicketPreview taskPreview = new TicketPreview(taskCard, uIPopup.TagUi);
            //add to tasklist
            taskPreviewList.Add(taskPreview);
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
        Debug.Log($"<color=cyan>{tasks.Count} Tasks received. Load to RAM...</color>");

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

        Debug.Log($"<color=cyan>{reportTags.enum_options.Count} ReportTags received.</color>");

        uIPopup.SetLoadingStatus(false);
    }

    //Search for the change text with lucene text analyzer
    private void Search(ChangeEvent<string> evt) {
        Search(evt.newValue);
    }

    //Search for the change text with lucene text analyzer
    public void Search(string searchTerm) {
        ResetPreview();

        //if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrWhiteSpace(searchTerm)) {
        //    return;
        //}

        searchResult = SearchWithLucene.Instance.SearchTerm(searchTerm).ToList();

        if (searchResult.Count == 0) {
            return;
        }

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

            preview.openDetailPopup = () => OnClickTicketPreviewAction(title, notes);
            preview.addToMentions = () => AddToMentionList(gid, task);
            preview.removeFromMentions = () => RemoveMentionFromList(gid);

            if (uIPopup.MentionedTask.ContainsKey(gid)) {
                preview.Select();
            }
        }
    }

    //Reset the preview objects (hide them and clear the text fields)
    public void ResetPreview() {
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
        if (!mentions.Contains(gid)) {
            mentions.Add(gid);
            uIPopup.MentionedTask.Add(gid, p);
        }
    }

    public void RemoveMentionFromList(string gid, bool updateUi = false) {
        if (mentions.Contains(gid)) {
            mentions.Remove(gid);
        }
        uIPopup.MentionedTask.Remove(gid);

        if (updateUi) {
            if (uIPopup.MentionedTask.Count == 0) {
                uIPopup.PanelComponents.mentionedTicketsContainer.style.display = DisplayStyle.None;
            }

            for (int i = 0; i < taskPreviewList.Count; i++) {
                if (taskPreviewList[i].gid == gid) {
                    taskPreviewList[i].Deselect();
                    break;
                }
            }
        }
    }
}
