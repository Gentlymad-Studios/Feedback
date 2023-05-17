
using UnityEngine;
/// <summary>
/// Generic data structure. 
/// </summary>
public class RequestData  {

    public string title;
    public string text;
    public Texture2D screenshot;
    public string dataType;

    public RequestData(string title, string text,Texture2D screenshot, string dataType) {
        this.title = title;
        this.text = text;
        this.screenshot = screenshot;
        this.dataType = dataType;
    }
}

public enum DataType {
    Feedback = 1,
    Bug = 2,
}

