using System;
using System.Collections.Generic;

/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
public class AsanaAPI : BaseAPI {

    public delegate void TicketsReceived(List<TaskModels.AsanaTaskModel> tickets);
    public static event TicketsReceived TicketsReceivedEvent;

    public AsanaAPISettings asanaSpecificSettings;
    public List<TaskModels.AsanaTaskModel> ticketModels = new List<TaskModels.AsanaTaskModel>();
    public List<TaskModels.AsanaTaskModel> ticketModelsBackup = new List<TaskModels.AsanaTaskModel>();
    public List<string> customFields = new List<string>();
    public List<string> mentions = new List<string>();

    public DateTime lastUpdateTime;

    public AsanaAPI(){
        CreateAPISpecificSettings();
        CreateRequestHandler(new AsanaRequestHandler(this));
    }

    public void SetMentionList(List<string> mentions) {
        this.mentions = mentions;   
    }

    public override void CreateAPISpecificSettings() {
        asanaSpecificSettings = APISettings.LoadSettings<AsanaAPISettings>();
        settings = asanaSpecificSettings;
    }

    public void FireTicketsCreated(List<TaskModels.AsanaTaskModel> tickets) {
        TicketsReceivedEvent.Invoke(tickets);
    }
}


