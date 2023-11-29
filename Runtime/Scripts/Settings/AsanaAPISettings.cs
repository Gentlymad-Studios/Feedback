using System;
using System.Collections.Generic;
using UnityEngine;


namespace Feedback {
    [CreateAssetMenu(fileName = nameof(AsanaAPISettings), menuName = nameof(AsanaAPISettings), order = 1)]
    public class AsanaAPISettings : APISettings {
        [Header("Adapter")]
        [SerializeReference]
        public CustomAdapter customAdapter = null;

        [NonSerialized]
        public IAdapter adapter;
        public IAdapter Adapter {
            get {
                if (adapter == null) {
                    if (customAdapter == null) {
                        adapter = new DefaultAdapter();
                    } else {
                        adapter = customAdapter;
                    }
                }
                return adapter;
            }
        }

        [Header("AsanaRequestManager Endpoints")]
        public string BaseUrl;
        public string GetReportTags = "/GetReportTags";
        public string GetPlayerTaskDataEndpoint = "GetAllPlayer";
        public string GetDevTaskDataEndpoint = "GetAllDev";
        public string LoginEndpoint = "Login/";
        public string LogoutEndpoint = "Logout/";
        public string PostNewTaskDataEndpoint = "PostNewTaskData/";
        public string GetUserWithUniqueId = "GetUserWithUniqueId/";

        [Header("Asana Workspace")]
        public string WorkspaceId;

        [Header("Asana Projects")]
        public bool enablePlayerProjects = true;
        public bool enableDevProjects = true;
        public List<AsanaProject> asanaProjects;

        [Header("Asana CustomField")]
        public string upvoteField = "Upvotes";
        public string tagField = "Report Tags";

        [Header("Attachments")]
        [Tooltip("Maximum Size per File in bytes")]
        public long maxFileSize = 10000000;
        public string AttachmentLocation = "\\Gentlymad Studios\\Endzone";
        [Tooltip("List of Files that should be attached for any Project, realtive to the Attachment Location")]
        public List<string> Files;
        [Tooltip("List of Files that should be attached for any Project, realtive to the Attachment Location, these files will bundled in one archive")]
        public List<ArchivedFiles> ArchivedFiles;

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

        [Header("Painter")]
        [ColorUsage(false)]
        public Color defaultColor = Color.red;
        public bool showBrushIndicator = true;

        public AsanaProject GetProjectByName(string projectName) {
            for (int i = 0; i < asanaProjects.Count; i++) {
                if (asanaProjects[i].name == projectName) {
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
        public bool includeLog = true;
        public bool includeSavegame = true;
        [Tooltip("Enable this if you want to include global files")]
        public bool includeGlobalFiles = false;
        [Tooltip("Enable this if you want to include project specific files")]
        public bool includeProjectFiles = false;
        [Tooltip("If this is set, the project is available for logged in users")]
        public bool visibleForDev = false;
        [Tooltip("If this is set, the project is available players")]
        public bool visibleForPlayer = false;
        [Tooltip("List of Files that should be attached to this Project, realtive to the Attachment Location")]
        public List<string> Files;
        [Tooltip("List of Files that should be attached to this Project, realtive to the Attachment Location, these files will bundled in one archive")]
        public List<ArchivedFiles> ArchivedFiles;
    }

    [Serializable]
    public class ArchivedFiles {
        public string name;
        public List<string> Files;
    }

    public class CustomData {
        public string gid;
        public List<string> values;
    }
}