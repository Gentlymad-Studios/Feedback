/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
public class AsanaAPI : BaseAPI {

    private APISettingsHandler settingsHandler;
    public AsanaAPISettings asanaAPISettings;
    public AsanaAPI(){
        settingsHandler = new APISettingsHandler();
        CreateAPISpecificSettings();
        CreateRequestHandler(new AsanaRequestHandler(this));
    }
    public override void CreateAPISpecificSettings() {
        asanaAPISettings = settingsHandler.FindAsanaAPISettings();
        base.settings = asanaAPISettings;
    }

}


