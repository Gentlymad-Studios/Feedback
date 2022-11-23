
/// <summary>
/// Generic data structure. 
/// </summary>
public class API_Data  {

    public string title;
    public string text;
    public DataType dataType;

    public API_Data(string title, string text, DataType dataType) {
        this.title = title;
        this.text = text;
        this.dataType = dataType;
    }
}

public enum DataType {
    feedback = 1,
    bug = 2,
}

