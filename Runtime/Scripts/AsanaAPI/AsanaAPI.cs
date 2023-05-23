using System;
using System.Collections.Generic;

/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
public class AsanaAPI : BaseAPI {

    public delegate void TasksReceived(List<TaskModels.AsanaTaskModel> tasks);
    public static event TasksReceived TasksReceivedEvent;

    public AsanaAPISettings AsanaSpecificSettings;
    public List<TaskModels.AsanaTaskModel> TicketModels = new List<TaskModels.AsanaTaskModel>();
    public List<TaskModels.AsanaTaskModel> TicketModelsBackup = new List<TaskModels.AsanaTaskModel>();
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

    public void FireTasksCreated(List<TaskModels.AsanaTaskModel> tickets) {
        TasksReceivedEvent.Invoke(tickets);
    }
}


