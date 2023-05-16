using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TicketPreview {
    public VisualElement ui;

    public Action<string,int> sendUpvoteAction;
    public Action openDetailPopup;
    public Action addToMentions;
    public Action removeFromMentions;
    public bool mentioned = false;

    public Label taskTitleLbl;
    public Label taskDescriptionLbl;
    private Label upvoteLbl;
    private Button mentionBtn;

    private string gid;
    private TaskModels.AsanaTaskModel ticketModel;
    private Action<TaskModels.AsanaTaskModel> fillPreview;
    private Action resetPreview;

    private bool voted = false;

    public TicketPreview(VisualElement ui) {
        taskTitleLbl = ui.Q("taskTitleLbl") as Label;
        taskDescriptionLbl = ui.Q("taskDescriptionLbl") as Label;
        upvoteLbl = ui.Q("upvoteLbl") as Label;
        mentionBtn = ui.Q("mentionBtn") as Button;

        fillPreview = FillPreview;
        resetPreview = ResetPreview;

        this.ui = ui;
        ui.RegisterCallback<ClickEvent>(Card_clicked);

        voted = false;
        upvoteLbl.RegisterCallback<ClickEvent>(Upvote_clicked);

        mentionBtn.RegisterCallback<ClickEvent>(Mention_clicked);
    }

    private void OnDestroy() {
        sendUpvoteAction = null;
        openDetailPopup = null;
    }

    #region ClickEvents
    private void Card_clicked(ClickEvent evt) {
        openDetailPopup.Invoke();
    }

    private void Upvote_clicked(ClickEvent evt) {
        Vote();
        sendUpvoteAction.Invoke(gid, int.Parse(upvoteLbl.text));
    }

    private void Mention_clicked(ClickEvent evt) {
        Mention();
    }
    #endregion

    private void FillPreview(TaskModels.AsanaTaskModel ticketModel) {
        taskTitleLbl.text = ticketModel.name;
        gid = ticketModel.gid;
        taskDescriptionLbl.text = ticketModel.notes;
        upvoteLbl.text = ticketModel.custom_fields[0]?.display_value.ToString();
    }
    private void ResetPreview() {
        taskTitleLbl.text = string.Empty;
        taskDescriptionLbl.text = string.Empty;
        gid = string.Empty;
        upvoteLbl.text = "0";
        mentioned = false;
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

    private void Mention() {
        if (!mentioned) {
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
        mentionBtn.text = "mentioned";
        addToMentions.Invoke();
    }
    public void Deselect() {
        mentioned = false;
        mentionBtn.text = "not mentioned";
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
