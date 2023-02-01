using UnityEditor;
using UnityEngine;

/// <summary>
/// The base api class defines all fields of the following derived api instances and their preferred handling methods
/// </summary>
public class BaseAPI {

    public AuthorizationInfo authorizationInfo; //Information about currently logged in user
    public APISettings settings;
    public BaseRequestHandler requestHandler;
    protected BaseRequestHandler CreateRequestHandler(BaseRequestHandler handler) {
        this.requestHandler = handler;
        return requestHandler;
    }

    public virtual void CreateAPISpecificSettings() { }
}

