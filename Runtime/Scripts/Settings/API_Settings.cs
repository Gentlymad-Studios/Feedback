using Codice.CM.SEIDInfo;
using UnityEditor;
using UnityEngine;


//[CreateAssetMenu(fileName = "API_Settings_SO", menuName = nameof(APISettings), order = 1)]
public class APISettings : ScriptableObject{
    [Header("General API Settings")]
    public string token;
    public string baseURL;
    public API_Type type;
}

public class AsanaAPISettings : APISettings {

    [Header("Asana API Settings")]
    public string workspaceRoute;
    public string projectRoute;
    public string taskRoute;
    public string userRoute;

    [Header("")]
    public string workspaceId;
    public string feedbackId;
    public string bugId;
    
    [Header("Resources")]
    public string pathToTaskTemplate;
}

public class APISettingsHandler {

    public readonly string SO_PREFIX = "API_Settings_SO_";
    public readonly string SO_SUFFIX = ".asset";
    public void BuildAPISettings(string token, string baseURL, API_Type type) {

        GeneralSettings general = AssetDatabase.LoadAssetAtPath<GeneralSettings>("Assets/Feedback/Runtime/Settings/GeneralSettings_SO.asset");
        APISettings API_SETTINGS_SO;
        
        if (type.Equals(API_Type.asana)) { 
            API_SETTINGS_SO = ScriptableObject.CreateInstance<AsanaAPISettings>();
        } else {
            API_SETTINGS_SO = ScriptableObject.CreateInstance<APISettings>();
        }
    
        
        API_SETTINGS_SO.token = token;
        API_SETTINGS_SO.baseURL = baseURL;
        API_SETTINGS_SO.type = type;

        string settingsPath = general.settingsPath + SO_PREFIX + API_Type.asana.ToString() + SO_SUFFIX;

        AssetDatabase.CreateAsset(API_SETTINGS_SO, settingsPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = API_SETTINGS_SO;
    }

    public APISettings LoadAPISettingsAtPath(string path) {
        return Object.Instantiate(AssetDatabase.LoadAssetAtPath<APISettings>(path));
    }
}

public enum API_Type {
    asana = 1,
}
