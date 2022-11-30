using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        APISettings settings = AssetDatabase.LoadAssetAtPath<APISettings>(path);
        return settings;
    }
}
