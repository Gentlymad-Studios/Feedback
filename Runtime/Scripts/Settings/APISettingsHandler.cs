using Codice.CM.SEIDInfo;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class APISettingsHandler {

    public APISettingsHandler() {
    }

    /// <summary>
    /// Returns true if a directory exists.
    /// </summary>
    /// <param name="path">The absolut path to the scriptable object</param>
    /// <returns></returns>
    public bool APISettingsAvailable(string path) {
        return Directory.Exists(path);
    }

    /// <summary>
    /// Return the APISettings at a given absoult path.
    /// </summary>
    /// <param name="path">The absolute path to the scriptable object</param>
    /// <returns></returns>
    public APISettings LoadAPISettingsAtPath(string path) {
        APISettings settings = AssetDatabase.LoadAssetAtPath<APISettings>(path);
        return settings;
    }

    /// <summary>
    /// Find all Graphs in asset database
    /// </summary>
    /// <returns></returns>
    public AsanaAPISettings FindAsanaAPISettings() {
        string[] guids = AssetDatabase.FindAssets($"t: {typeof(AsanaAPISettings).Name}").ToArray();
        AsanaAPISettings settings = null;
     
        if (guids.Length > 1) {
            Debug.LogError("More than one asana api settings.");
            return null;
        }
        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            settings = AssetDatabase.LoadAssetAtPath<AsanaAPISettings>(path);
        }

        return settings;
    }
}
