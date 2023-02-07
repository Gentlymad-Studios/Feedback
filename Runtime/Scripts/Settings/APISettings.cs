using System.Linq;
using UnityEditor;
using UnityEngine;


public class APISettings : ScriptableObject {
    [Header("General API Settings")]
    public string token;
    public string defaultToken;
    public string baseURL;
    public API_Type type;

    //TODO: please revise this new implementation, which is built upon the way you did it by doing a FindAssets call.
    public static T LoadSettings<T>() where T: APISettings {
        string[] guids = AssetDatabase.FindAssets($"t: {typeof(T).Name}").ToArray();
        T settings;

        if (guids.Length > 0) {
            //TODO: Since we'll always pick the first guid we can avoid using a for loop
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            
            //TODO: I would advise to just use the first guid we found and don't crash by returning null.
            //If we provide the empty shell for a settings file and we just simply throw a warning here, things should be fine.
            if (guids.Length > 1) {
                Debug.LogError($"More than one settings file found! Using: {path}");
            }

            settings = AssetDatabase.LoadAssetAtPath<T>(path);
        } else {
            Debug.LogError($"No settings file found! ");
            return null;
        }

        return settings;
    }
}

//TODO: Please write enum definitions with a capital letter
//TODO: Please descide on a consistent naming pattern (with/without underscores, my personal suggestion is to use camelCase)
//TODO: not sure if the enum declaration should be made here in this settings file. If this makes sense i would advise to encapsulate it inside the APISettings class
public enum API_Type {
    asana = 1,
}
