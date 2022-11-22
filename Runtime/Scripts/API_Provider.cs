using UnityEngine;

public class API_Provider : MonoBehaviour {

    public GeneralSettings generalSettings;
    public API_Type type;

    private APISettingsHandler apiSettingsHandler;
    private Base_API apiBase;

    private void Start() {
        apiSettingsHandler = new APISettingsHandler();

        //To build your API settings call BuildAPISettings method:
        //apiSettingsHandler.BuildAPISettings(your token, your baseURL, API_Type.asana);

        APISettings settings = apiSettingsHandler.LoadAPISettingsAtPath(
            generalSettings.settingsPath + 
            apiSettingsHandler.SO_PREFIX +
            type.ToString() + 
            apiSettingsHandler.SO_SUFFIX);

        if(type is API_Type.asana) {
            apiBase = new AsanaAPI(settings);
        }
        
    }

    public void Submit() {
        Debug.Log("Submit");
    }

    
}
