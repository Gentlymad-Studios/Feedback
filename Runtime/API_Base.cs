using UnityEditor;

public class APIBase {

    public readonly APISettings settings;
    private IResponseHandler responseHandler;
    private IRequestHandler requestHandler;

    public APIBase(APISettings settings) {
        this.settings = settings;
    }

    public virtual void CreateResponseHandler() { }

    public virtual void CreateRequestHandler() { }


}
