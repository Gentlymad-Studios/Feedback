/// <summary>
/// Api class which inherits from the basic api class. It defines its own handler. 
/// </summary>
public class AsanaAPI : BaseAPI {

    private APISettingsHandler settingsHandler;
    public AsanaAPISettings asanaAPISettings;
    public AsanaAPI(){
        settingsHandler = new APISettingsHandler();
        CreateAPISpecificSettings();

        CreateRequestHandler(new AsanaRequestHandler(this));
        CreateResponseHandler(new AsanaResponseHandler());
    }

    public override void CreateAPISpecificSettings() {
        asanaAPISettings = settingsHandler.FindAsanaAPISettings();
        base.settings = asanaAPISettings;
    }

}


