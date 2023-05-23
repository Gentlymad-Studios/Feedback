using System.Linq;
using UnityEditor;
using UnityEngine;


public class APISettings : ScriptableObject {

    public APIType type;

    //public static T LoadSettings<T>() where T: APISettings {
    //    string[] guids = AssetDatabase.FindAssets($"t: {typeof(T).Name}").ToArray();
    //    T settings;

    //    if (guids.Length > 0) {
    //        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
    //        if (guids.Length > 1) {
    //            Debug.LogError($"More than one settings file found! Using: {path}");
    //        }
    //        settings = AssetDatabase.LoadAssetAtPath<T>(path);
    //    } else {
    //        Debug.LogError($"No settings file found! ");
    //        return null;
    //    }
    //    return settings;
    //}

    public enum APIType {
        Asana = 1,
    }
}
