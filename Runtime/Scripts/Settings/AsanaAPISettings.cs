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

    [Header("Asana Gids")]
    public string BugProjectId;
    public string FeedbackProjectId;
    public string WorkspaceId;

    [Header("Task Settings")]
    public List<string> Tags;
    public List<string> DataTypes;
}
