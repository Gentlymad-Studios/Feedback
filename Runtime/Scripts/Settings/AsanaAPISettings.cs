using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(AsanaAPISettings), menuName = nameof(AsanaAPISettings), order = 1)]
public class AsanaAPISettings : APISettings {

    [Header("AsanaRequestManager Endpoints")]
    public string baseUrl;
    public string getCustomFields;
    public string getAllTaskDataEndpoint;
    public string loginEndpoint;
    public string logoutEndpoint;
    public string postNewTaskDataEndpoint;
    public string updateUpvotesEndpoint;
    public string getUserWithUniqueId;

    [Header("Asana Gids")]
    public string bugProjectId;
    public string feedbackProjectId;
    public string workspaceId;

    [Header("Task Settings")]
    public List<string> tags;
    public List<string> dataTypes;
}
