using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(AsanaAPISettings), menuName = nameof(AsanaAPISettings), order = 1)]
public class AsanaAPISettings : APISettings {

    [Header("AsanaRequestManager Endpoints")]
    public string BaseUrl;
    public string GetCustomFields;
    public string GetAllTaskDataEndpoint;
    public string LoginEndpoint;
    public string LogoutEndpoint;
    public string PostNewTaskDataEndpoint;
    public string UpdateUpvotesEndpoint;
    public string GetUserWithUniqueId;

    [Header("Asana Workspace")]
    public string WorkspaceId;

    [Header("Asana Projects")]
    public List<AsanaProject> asanaProjects;

    [Header("Task Settings")]
    public List<string> Tags;

    [Header("Attachments")]
    public string AttachmentLocation = "\\Gentlymad Studios\\Endzone";
    public string SavegameLocation = "\\savegame";
    public string LogLocation = "\\logs";

    public List<string> CustomFileList;

    public AsanaProject GetProjectByName(string projectName) {
        for (int i = 0; i < asanaProjects.Count; i++) {
            if(asanaProjects[i].name == projectName) {
                return asanaProjects[i];
            }
        }

        return null;
    }
}

[Serializable]
public class AsanaProject {
    public string name;
    public string id;
    public bool includeLatesOutputLog = true;
    public bool includeLatestSavegame = true;
    public bool includeCustomFileList = false;
}
