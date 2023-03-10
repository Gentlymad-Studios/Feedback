using System.Collections.Generic;

/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
public class AsanaAPI : BaseAPI {

    public delegate void TicketsReceived(List<TicketModels.AsanaTicketModel> tickets);
    public static event TicketsReceived TicketsReceivedEvent;

    public AsanaAPISettings asanaSpecificSettings;
    public List<TicketModels.AsanaTicketModel> ticketModels = new List<TicketModels.AsanaTicketModel>();

    public AsanaAPI(){
        CreateAPISpecificSettings();
        CreateRequestHandler(new AsanaRequestHandler(this));
    }

    public override void CreateAPISpecificSettings() {
        asanaSpecificSettings = APISettings.LoadSettings<AsanaAPISettings>();
        settings = asanaSpecificSettings;
    }

    public void FireTicketsCreated(List<TicketModels.AsanaTicketModel> tickets) {
        TicketsReceivedEvent.Invoke(tickets);
    }
}


