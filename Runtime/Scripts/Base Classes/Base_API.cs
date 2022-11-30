using UnityEditor;

/// <summary>
/// The base api class defines all fields of the following derived api instances and their preferred handling methods
/// </summary>
public class Base_API {

    public APISettings settings;
    public BaseResponseHandler responseHandler;
    public BaseRequestHandler requestHandler;

    public Base_API(APISettings settings) {
        this.settings = settings;
    }

    public virtual void CreateHandler() { }
}

