using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = nameof(APISettings), menuName = nameof(APISettings), order = 1)]
public class APISettings : ScriptableObject{
    public string token;
    public string baseURL;
    public API_Type type;
}

public class APISettingsHandler {

    public static readonly string SO_LOCATION = "Assets/Feedback/Runtime/APISettings";
    public static readonly string SO_PREFIX = "API_Settings_SO";
    public void BuildAPISettings(string appId, string token, string baseURL, string apiKey, API_Type type) {

        APISettings API_SETTINGS_SO = ScriptableObject.CreateInstance<APISettings>();
        
        API_SETTINGS_SO.token = token;
        API_SETTINGS_SO.baseURL = baseURL;
        API_SETTINGS_SO.type = type;

        string settingsPath = SO_LOCATION + SO_PREFIX + type.ToString() + ".asset";

        AssetDatabase.CreateAsset(API_SETTINGS_SO, settingsPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = API_SETTINGS_SO;
    }

    public APISettings LoadAPISettingsAtPath(string path) {
        return AssetDatabase.LoadAssetAtPath<APISettings>(path);
    }
}

public enum API_Type {
    asana = 1,
}
