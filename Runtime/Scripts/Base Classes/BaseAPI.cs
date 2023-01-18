using UnityEditor;
using UnityEngine;

/// <summary>
/// The base api class defines all fields of the following derived api instances and their preferred handling methods
/// </summary>
public class BaseAPI {
    public AuthorizationInfo authorizationInfo; //Information about currently logged in user
    public APISettings settings;
    public BaseResponseHandler responseHandler;
    public BaseRequestHandler requestHandler;
    protected BaseResponseHandler CreateResponseHandler(BaseResponseHandler handler) {
        this.responseHandler = handler;
        return responseHandler;
    }
    protected BaseRequestHandler CreateRequestHandler(BaseRequestHandler handler) {
        this.requestHandler = handler;
        return requestHandler;
    }

    public virtual void CreateAPISpecificSettings() { }
}

