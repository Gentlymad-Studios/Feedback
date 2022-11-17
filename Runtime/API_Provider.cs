using UnityEngine;

public class API_Provider : MonoBehaviour {

    private APISettingsHandler apiSettingsHandler;
    private APIBase apiBase;
    private void Start() {
        apiSettingsHandler = new APISettingsHandler();
        apiBase = new APIBase(apiSettingsHandler.LoadAPISettingsAtPath("Assets/Feedback/Runtime/Settings/APISettings.asset"));
    }

    public void Submit() {
        Debug.Log(apiBase.settings.type);
    }
}
