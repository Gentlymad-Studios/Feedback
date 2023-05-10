using System;
using System.Collections.Generic;

/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
public class AsanaAPI : BaseAPI {

    public delegate void TicketsReceived(List<TicketModels.AsanaTaskModel> tickets);
    public static event TicketsReceived TicketsReceivedEvent;

    public AsanaAPISettings asanaSpecificSettings;
    public List<TicketModels.AsanaTaskModel> ticketModels = new List<TicketModels.AsanaTaskModel>();
    public List<TicketModels.AsanaTaskModel> ticketModelsBackup = new List<TicketModels.AsanaTaskModel>();
    public List<string> customFields = new List<string>();

    public DateTime lastUpdateTime;

    public AsanaAPI(){
        CreateAPISpecificSettings();
        CreateRequestHandler(new AsanaRequestHandler(this));
    }

    public override void CreateAPISpecificSettings() {
        asanaSpecificSettings = APISettings.LoadSettings<AsanaAPISettings>();
        settings = asanaSpecificSettings;
    }

    public void FireTicketsCreated(List<TicketModels.AsanaTaskModel> tickets) {
        TicketsReceivedEvent.Invoke(tickets);
    }
}


