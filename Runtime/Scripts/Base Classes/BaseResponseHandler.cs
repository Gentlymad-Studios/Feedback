public class BaseResponseHandler : IResponseHandler {

    protected string jsonResponse = string.Empty;
    public virtual void SendResponse(string response) { }

}
