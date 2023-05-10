using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicketPreview : MonoBehaviour {

    [SerializeField] private TMP_Text ticketName;
    [SerializeField] private TMP_Text notes;
    [SerializeField] private TMP_Text upvotes;
    [SerializeField] private Button voteButton;

    public Action<string,int> sendUpvoteAction;

    private string gid;
    private TicketModels.AsanaTaskModel ticketModel;
    private Action<TicketModels.AsanaTaskModel> fillPreview;
    private Action resetPreview;
    private bool voted;

    private void Start() {
        voted = false;
        voteButton.onClick.AddListener(() => {
            Vote();
            sendUpvoteAction.Invoke(gid, int.Parse(upvotes.text));
        });
    }

    private void OnEnable() {
        fillPreview = FillPreview;
        resetPreview = ResetPreview;
    }

    private void FillPreview(TicketModels.AsanaTaskModel ticketModel) {
        ticketName.text = ticketModel.name;
        gid = ticketModel.gid;
        notes.text = ticketModel.notes;
        upvotes.text = ticketModel.custom_fields[0]?.display_value.ToString();
    }
    private void ResetPreview() {
        ticketName.text = string.Empty;
        notes.text = string.Empty;
        gid = string.Empty;
        upvotes.text = "0";
    }

    public string Vote() {
        int value;
        int.TryParse(upvotes.text.ToString(), out value);
        Debug.Log(value);

        if (!voted) {
            value += 1;
            voted = true;
        } else {
            value -= 1;
            voted = false;
        }
        upvotes.text = value.ToString();
        return value.ToString();
    }

    public void SetTicketModel(TicketModels.AsanaTaskModel ticketModel) {
        this.ticketModel = ticketModel;
        fillPreview.Invoke(ticketModel);
    }

    public void ResetTicketModel() {
        resetPreview.Invoke();
        ticketModel = null;
    }
    public bool PreviewEmpty() {
        if (ticketName.text.Equals(string.Empty)) {
            return true;
        }
        return false;
    }
    public TicketModels.AsanaTaskModel GetTicketModel() {
        return ticketModel;
    }
}
