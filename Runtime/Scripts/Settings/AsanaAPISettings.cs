using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(AsanaAPISettings), menuName = nameof(AsanaAPISettings), order = 1)]
public class AsanaAPISettings : APISettings {
    [Header("AsanaRequestManager Endpoints")]
    public string BaseUrl;
    public string GetReportTags;
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

    [Header("Asana CustomField")]
    public string upvoteField = "Upvotes";
    public string tagField = "Report Tags";

    [Header("Attachments")]
    public string AttachmentLocation = "\\Gentlymad Studios\\Endzone";
    public string SavegameLocation = "\\savegame";
    public string LogLocation = "\\logs";
    [Tooltip("List of Files that should be attached for any Project")]
    public List<string> GlobalCustomFiles;

    [Header("Tab GUI Settings")]
    [Multiline]
    public string searchDescripton;
    [Multiline]
    public string reportDescription;

    [Header("HowTo")]
    [Multiline]
    [Tooltip("Link will placed at the end of the Description")]
    public string howToDescription;
    public string howToName;
    public string howToUrl;

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
    public string titlePlaceholder = "Title...";
    [Multiline]
    public string descriptionPlaceholder = "Description...";
    public bool includeLatesOutputLog = true;
    public bool includeLatestSavegame = true;
    public bool includeGlobalCustomFiles = false;
    public bool includeCustomFiles = false;
    [Tooltip("If this is set, the project is only available logged in users")]
    public bool visibleOnLoginOnly = false;
    [Tooltip("If this is set, the project is hidden for logged in users")]
    public bool hideOnLogin = false;
    [Tooltip("List of Files that should be attached for to this Project")]
    public List<string> CustomFiles;

    public delegate List<CustomData> Callback();
    public Callback CustomDataCallback;

    public class CustomData {
        public string gid;
        public List<string> values;
    }
}

