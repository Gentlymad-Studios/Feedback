using UnityEngine;

public class API_Provider : MonoBehaviour {

    public GeneralSettings generalSettings;
    public API_Type type;

    private APISettingsHandler apiSettingsHandler;
    private AsanaRequestHandler asanaRequestHandler;
    private Base_API apiBase;

    private void Start() {
        apiSettingsHandler = new APISettingsHandler();
        //apiSettingsHandler.BuildAPISettings(your token, your baseURL, API_Type.asana);
        apiBase = new Base_API(apiSettingsHandler
            .LoadAPISettingsAtPath(generalSettings.settingsPath + apiSettingsHandler.SO_PREFIX + 
            type.ToString() + apiSettingsHandler.SO_SUFFIX));

        asanaRequestHandler = new AsanaRequestHandler();
        asanaRequestHandler.CreateClientInstance(apiBase.settings.baseURL, apiBase.settings.token);
    }

    public void Submit() {
        Debug.Log(apiBase.settings.type);
    }

    
}
