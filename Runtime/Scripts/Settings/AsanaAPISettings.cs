using UnityEngine;

[CreateAssetMenu(fileName = nameof(AsanaAPISettings), menuName = nameof(AsanaAPISettings), order = 1)]
public class AsanaAPISettings : APISettings {

    [Header("Asana API Settings")]
    public string workspaceRoute;
    public string projectRoute;
    public string taskRoute;
    public string userRoute;
    public string attachmentRoute;

    [Header("")]
    public string workspaceId;
    public string feedbackId;
    public string bugId;

    [Header("Authorization Information")]
    public string asanaAuthorizationEndpoint;
    public string asanaTokenExchangeEndpoint;
    public string clientId;
    public string clientSecret; 
    public string responseType;
    public string redirectUri;
    public string scope;
}
