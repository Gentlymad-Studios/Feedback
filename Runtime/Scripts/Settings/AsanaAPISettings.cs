using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsanaAPISettings : APISettings {

    [Header("Asana API Settings")]
    public string workspaceRoute;
    public string projectRoute;
    public string taskRoute;
    public string userRoute;

    [Header("")]
    public string workspaceId;
    public string feedbackId;
    public string bugId;

    [Header("Resources")]
    public string pathToTaskTemplate;

    [Header("Authorization Service Endpoits")]
    public string ServiceEndPoint;
    public string ServiceCookieEndpoint;

}
