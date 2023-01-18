
/// <summary>
/// Generic data structure. 
/// </summary>
public class RequestData  {

    public string title;
    public string text;
    public DataType dataType;

    public RequestData(string title, string text, DataType dataType) {
        this.title = title;
        this.text = text;
        this.dataType = dataType;
    }
}

public enum DataType {
    feedback = 1,
    bug = 2,
}

