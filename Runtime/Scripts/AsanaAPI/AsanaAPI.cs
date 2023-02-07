using System.Collections.Generic;
/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
public class AsanaAPI : BaseAPI {
    // TODO: alternative event based way to receive data from anywhere
    public delegate void TicketsReceived(List<TicketModel> tickets);
    public static event TicketsReceived TicketsReceivedEvent;

    public AsanaAPISettings asanaSpecificSettings;
    public List<TicketModel> ticketModels = new List<TicketModel>();

    public AsanaAPI(){
        CreateAPISpecificSettings();
        CreateRequestHandler(new AsanaRequestHandler(this));
    }

    public override void CreateAPISpecificSettings() {
        asanaSpecificSettings = APISettings.LoadSettings<AsanaAPISettings>();
        settings = asanaSpecificSettings;
    }

    public void FireTicketsCreated(List<TicketModel> tickets) {
        TicketsReceivedEvent.Invoke(tickets);
    }
}


