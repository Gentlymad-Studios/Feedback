using UnityEditor;

public class Base_API {

    public readonly APISettings settings;
    private IResponseHandler responseHandler;
    private IRequestHandler requestHandler;

    public Base_API(APISettings settings) {
        this.settings = settings;
    }

    public virtual void CreateResponseHandler() { }

    public virtual void CreateRequestHandler() { }


}
