public interface IRequestHandler {
    public void CreateClientInstance();
    public void GET(string route);
    public void POST(API_Data data);

}
