using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TicketPreview {
    public Action<string,int> sendUpvoteAction;
    public Action openDetailPopup;
    public Action addToMentions;
    public Action removeFromMentions;
    public bool mentioned = false;

    public VisualElement ui;
    public Label taskTitleLbl;
    public Label taskTypeLbl;
    public Label taskDescriptionLbl;
    private Label upvoteLbl;
    private Toggle mentionTgl;
    private VisualElement tagContainer;
    private VisualElement tagHolder;

    public VisualTreeAsset tagUi;

    public string gid;
    private TaskModels.AsanaTaskModel ticketModel;
    private Action<TaskModels.AsanaTaskModel> fillPreview;
    private Action resetPreview;

    private bool voted = false;

    public TicketPreview(VisualElement ui, VisualTreeAsset tagUi) {
        this.ui = ui;
        this.tagUi = tagUi;

        taskTitleLbl = ui.Q("taskTitleLbl") as Label;
        taskTypeLbl = ui.Q("taskTypeLbl") as Label;
        taskDescriptionLbl = ui.Q("taskDescriptionLbl") as Label;
        upvoteLbl = ui.Q("upvoteLbl") as Label;
        mentionTgl = ui.Q("mentionedTgl") as Toggle;
        tagContainer = ui.Q("TagContainer");
        tagHolder = ui.Q("TagHolder");

        fillPreview = FillPreview;
        resetPreview = ResetPreview;

        voted = false;
        RegisterEvents();
    }

    public void RegisterEvents() {
        UnregisterEvents();
        ui.RegisterCallback<ClickEvent>(Card_clicked);
        upvoteLbl.RegisterCallback<ClickEvent>(Upvote_clicked);
        mentionTgl.RegisterValueChangedCallback(Mention_changed);
        mentionTgl.RegisterCallback<ClickEvent>(Mention_clicked);
    }

    public void UnregisterEvents() {
        ui.UnregisterCallback<ClickEvent>(Card_clicked);
        upvoteLbl.UnregisterCallback<ClickEvent>(Upvote_clicked);
        mentionTgl.UnregisterValueChangedCallback(Mention_changed);
        mentionTgl.UnregisterCallback<ClickEvent>(Mention_clicked);

    }

    #region ClickEvents
    private void Card_clicked(ClickEvent evt) {
        openDetailPopup.Invoke();
    }

    private void Upvote_clicked(ClickEvent evt) {
        Vote();
        sendUpvoteAction.Invoke(gid, int.Parse(upvoteLbl.text));
        evt.StopPropagation();
    }

    private void Mention_clicked(ClickEvent evt) {
        evt.StopPropagation();
    }

    private void Mention_changed(ChangeEvent<bool> evt) {
        Mention(evt.newValue);
    }
    #endregion

    private void FillPreview(TaskModels.AsanaTaskModel ticketModel) {
        taskTitleLbl.text = ticketModel.name;
        gid = ticketModel.gid;
        taskDescriptionLbl.text = ticketModel.notes;

        taskTypeLbl.text = "Type: Unknown";
        for (int i = 0; i < UIPopup.settings.asanaProjects.Count; i++) {
            if (UIPopup.settings.asanaProjects[i].id == ticketModel.project_id) {
                taskTypeLbl.text = $"Type: {UIPopup.settings.asanaProjects[i].name}";
                break;
            }
        }

        upvoteLbl.text = ticketModel.custom_fields[0]?.display_value.ToString();

        string[] tags = null;
        string value = ticketModel.custom_fields[1]?.display_value?.ToString();
        if (!string.IsNullOrEmpty(value)) {
            tags = value.Split(", ");
        }

        tagHolder.Clear();

        if (tags != null && tags.Length > 0) {
            tagContainer.style.display = DisplayStyle.Flex;

            for (int i = 0; i < tags.Length; i++) {
                VisualElement ui = tagUi.Instantiate();
                tagHolder.Add(ui);
                new TagPreview(ui, tags[i], displayOnly: true);
            }
        } else {
            tagContainer.style.display = DisplayStyle.None;
        }
    }
    private void ResetPreview() {
        taskTitleLbl.text = string.Empty;
        taskDescriptionLbl.text = string.Empty;
        gid = string.Empty;
        upvoteLbl.text = "0";
        mentioned = false;
        mentionTgl.SetValueWithoutNotify(false);
    }
    public string Vote() {
        int value;
        int.TryParse(upvoteLbl.text.ToString(), out value);
        Debug.Log(value);

        if (!voted) {
            value += 1;
            voted = true;
        } else {
            value -= 1;
            voted = false;
        }
        upvoteLbl.text = value.ToString();
        return value.ToString();
    }

    private void Mention(bool mentioned) {
        if (mentioned) {
            Select();
        } else {
            Deselect();
        }
    }

    public TicketPreview SetTicketModel(TaskModels.AsanaTaskModel ticketModel) {
        this.ticketModel = ticketModel;
        fillPreview.Invoke(ticketModel);
        return this;
    }

    public void ResetTicketModel() {
        resetPreview.Invoke();
        ticketModel = null;
    }
    public bool PreviewEmpty() {
        if (taskTitleLbl.text.Equals(string.Empty)) {
            return true;
        }
        return false;
    }

    public void Select() {
        mentioned = true;
        mentionTgl.SetValueWithoutNotify(true);
        addToMentions.Invoke();
    }
    public void Deselect() {
        mentioned = false;
        mentionTgl.SetValueWithoutNotify(false);
        removeFromMentions.Invoke();
    }

    public void SetActive(bool active) {
        if (active) {
            ui.style.display = DisplayStyle.Flex;
        } else {
            ui.style.display = DisplayStyle.None;
        }
    }
}
