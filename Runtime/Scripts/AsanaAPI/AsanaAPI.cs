using System;
using System.Collections.Generic;

/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
public class AsanaAPI : BaseAPI {

    public delegate void DataReceived(List<TaskModels.AsanaTaskModel> tasks, TaskModels.ReportTags reportTags);
    public static event DataReceived DataReceivedEvent;

    public AsanaAPISettings AsanaSpecificSettings;
    public List<TaskModels.AsanaTaskModel> TicketModelsBackup = new List<TaskModels.AsanaTaskModel>();
    public TaskModels.ReportTags ReportTagsBackup = new TaskModels.ReportTags();
    public List<string> CustomFields = new List<string>();
    public List<string> Mentions = new List<string>();

    public DateTime lastUpdateTime;

    public AsanaAPI(AsanaAPISettings s){
        //CreateAPISpecificSettings();
        AsanaSpecificSettings = s;
        base.RequestHandler = new AsanaRequestHandler(this);
    }

    public void SetMentionList(List<string> mentions) {
        this.Mentions = mentions;   
    }

    public override void CreateAPISpecificSettings() {
        //AsanaSpecificSettings = APISettings.LoadSettings<AsanaAPISettings>();
        Settings = AsanaSpecificSettings;
    }

    public void FireDataCreated(List<TaskModels.AsanaTaskModel> tickets, TaskModels.ReportTags reportTags) {
        DataReceivedEvent.Invoke(tickets, reportTags);
    }
}


