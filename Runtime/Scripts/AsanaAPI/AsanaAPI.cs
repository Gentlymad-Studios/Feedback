/// <summary>
/// Api class which inherits from the basic api class. It defines its own handler. 
/// </summary>
public class AsanaAPI : Base_API {
    public AsanaAPI(APISettings settings) : base(settings) {
        base.settings = settings;
        CreateHandler();
    }

    /// <summary>
    /// Create the api handler.
    /// </summary>
    public override void CreateHandler() {
        base.requestHandler = new AsanaRequestHandler(base.settings);
        base.responseHandler = new AsanaResponseHandler();
    }
}
