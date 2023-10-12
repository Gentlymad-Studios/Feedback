using System;
using System.Collections.Generic;

/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
public class AsanaAPI : BaseAPI {
    public delegate void DataReceived(List<TaskModels.AsanaTaskModel> tasks, TaskModels.ReportTags reportTags);
    public static event DataReceived DataReceivedEvent;

    public delegate void LoginResult(bool success);
    public static event LoginResult LoginResultEvent;

    public delegate void AvatarLoaded();
    public static event AvatarLoaded AvatarLoadedEvent;

    public AsanaAPISettings AsanaSpecificSettings;
    public List<TaskModels.AsanaTaskModel> TicketModelsBackup = new List<TaskModels.AsanaTaskModel>();
    public TaskModels.ReportTags ReportTagsBackup = new TaskModels.ReportTags();
    public List<string> CustomFields = new List<string>();
    public List<string> Mentions = new List<string>();

    public DateTime lastUpdateTime;

    public AsanaAPI(AsanaAPISettings s){
        AsanaSpecificSettings = s;
        base.RequestHandler = new AsanaRequestHandler(this);
    }

    public override void CreateAPISpecificSettings() {
        Settings = AsanaSpecificSettings;
    }

    public void FireDataCreated(List<TaskModels.AsanaTaskModel> tickets, TaskModels.ReportTags reportTags) {
        DataReceivedEvent.Invoke(tickets, reportTags);
    }

    public void FireLoginResult(bool success) {
        LoginResultEvent.Invoke(success);
    }

    public void FireAvatarLoaded() {
        AvatarLoadedEvent.Invoke();
    }
}


