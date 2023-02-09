
using UnityEngine;
/// <summary>
/// Generic data structure. 
/// </summary>
public class RequestData  {

    public string title;
    public string text;
    public Texture2D screenshot;
    public DataType dataType;

    public RequestData(string title, string text,Texture2D screenshot, DataType dataType) {
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

