using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicketPreview : MonoBehaviour {
    [SerializeField] private TMP_Text ticketName;
    [SerializeField] private TMP_Text notes;
    [SerializeField] private TMP_Text upvotes;
    [SerializeField] private Button voteButton;

    private Action<string> VoteAction;
    private TicketModels.AsanaTicketModel ticketModel;
    private Action<TicketModels.AsanaTicketModel> fillPreview;
    private Action resetPreview;
    private bool voted;

    private void Start() {
        voted = false;
        voteButton.onClick.AddListener(Vote);
    }

    private void OnEnable() {
        fillPreview = FillPreview;
        resetPreview = ResetPreview;
    }

    private void FillPreview(TicketModels.AsanaTicketModel ticketModel) {
        ticketName.text = ticketModel.name;
        notes.text = ticketModel.notes;
        upvotes.text = ticketModel.custom_fields[0]?.display_value.ToString();
    }
    private void ResetPreview() {
        ticketName.text = string.Empty;
        notes.text = string.Empty;
        upvotes.text = "0";
    }

    private void Vote() {
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
        Debug.Log(value);
        upvotes.text = value.ToString();
    }

    public void SetTicketModel(TicketModels.AsanaTicketModel ticketModel) {
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
    public TicketModels.AsanaTicketModel GetTicketModel() {
        return ticketModel;
    }
}
