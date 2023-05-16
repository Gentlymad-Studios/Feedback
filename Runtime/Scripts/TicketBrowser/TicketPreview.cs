using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicketPreview : MonoBehaviour {

    public Action<string,int> sendUpvoteAction;
    public Action openDetailPopup;
    public Action addToMentions;
    public Action removeFromMentions;
    public bool mentioned = false;

    [SerializeField] public TMP_Text ticketName;
    [SerializeField] public TMP_Text notes;
    [SerializeField] private TMP_Text upvotes;
    [SerializeField] private Button voteButton;
    [SerializeField] private Button mentionButton;

    private string gid;
    private Button taskButton;
    private TaskModels.AsanaTaskModel ticketModel;
    private Action<TaskModels.AsanaTaskModel> fillPreview;
    private Action resetPreview;

    private bool voted = false;


    private void Start() {

        taskButton = GetComponent<Button>();
        taskButton.onClick.AddListener(() => openDetailPopup.Invoke());

        voted = false;
        voteButton.onClick.AddListener(() => {
            Vote();
            sendUpvoteAction.Invoke(gid, int.Parse(upvotes.text));
        });

        mentionButton.onClick.AddListener(Mention);
    }

    private void OnEnable() {
        fillPreview = FillPreview;
        resetPreview = ResetPreview;
    }

    private void OnDestroy() {
        sendUpvoteAction = null;
        openDetailPopup = null;
    }

    private void FillPreview(TaskModels.AsanaTaskModel ticketModel) {
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
        mentioned = false;
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
        if (ticketName.text.Equals(string.Empty)) {
            return true;
        }
        return false;
    }

    public void Select() {
        mentioned = true;
        mentionButton.GetComponentInChildren<TMP_Text>().text = "mentioned";
        addToMentions.Invoke();
    }
    public void Deselect() {
        mentioned = false;
        mentionButton.GetComponentInChildren<TMP_Text>().text = "not mentioned";
        removeFromMentions.Invoke();
    }

}
