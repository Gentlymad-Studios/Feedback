using System;
using System.Collections.Generic;
using UnityEngine;


namespace Feedback {
    [CreateAssetMenu(fileName = nameof(AsanaAPISettings), menuName = nameof(AsanaAPISettings), order = 1)]
    public class AsanaAPISettings : ScriptableObject {
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

        [Header("Attachments")]
        [Tooltip("Maximum size for files that cant be reduced, in bytes")]
        public long maxFileSize = 10000000;
        [Tooltip("Maximum size for files that can be reduced, in bytes")]
        public long maxFileSizeReducible = 10000000;
        [Tooltip("List of Files that should be attached for any Project, realtive to the Application Data Location")]
        public List<string> Files;
        [Tooltip("List of Files that should be attached for any Project, realtive to the Application Data Location, these files will bundled in one archive")]
        public List<ArchivedFiles> ArchivedFiles;

        [Header("Request")]
        [Tooltip("The Timeout for the HTTP Request when recieving data in ms.")]
        public int recieveTimeout = 5000;
        [Tooltip("The Timeout for the HTTP Request when sending data in ms.")]
        public int sendTimeout = 10000;
        [Tooltip("The Timeout for the HTTP Request when login / logout / ... in ms.")]
        public int loginoutTimeout = 5000;
        [Tooltip("The Cooldown between a fetch of tickets in s.")]
        public int dataFetchCooldown = 60;

        [Header("GUI Settings")]
        public string headerTitle;
        [Multiline]
        public string reportDescription;
        public string helpLink;
        public string overviewText;
        public string overviewLink;

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
        public bool includeErrorLog = true;
        [Tooltip("The Amount of Errors we add to special Error Log. 0 = unlimited")]
        public int errorLogCount = 5;
        public bool includePlayerLog = true;
        public bool includeCustomLog = true;
        public bool includeSavegame = true;
        [Tooltip("Enable this if you want to include global files")]
        public bool includeGlobalFiles = false;
        [Tooltip("Enable this if you want to include project specific files")]
        public bool includeProjectFiles = false;
        [Tooltip("If this is set, the project is available for logged in users")]
        public bool visibleForDev = false;
        [Tooltip("If this is set, the project is available players")]
        public bool visibleForPlayer = false;
        [Tooltip("List of Files that should be attached to this Project, realtive to the Application Data Location")]
        public List<string> Files;
        [Tooltip("List of Files that should be attached to this Project, realtive to the Application Data Location, these files will bundled in one archive")]
        public List<ArchivedFiles> ArchivedFiles;
    }

    [Serializable]
    public class ArchivedFiles {
        public string name;
        public List<string> Files;
    }

    public class CustomData {
        public string gid;
        public string friendly_name;
        public List<string> values;
        public List<string> friendly_values;
    }
}