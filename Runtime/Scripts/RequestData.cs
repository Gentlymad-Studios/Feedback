
using UnityEngine;
/// <summary>
/// Generic data structure. 
/// </summary>
public class RequestData  {

    public string Title;
    public string Text;
    public Texture2D Screenshot;
    public string DataType;

    public RequestData(string title, string text,Texture2D screenshot, string dataType) {
        this.Title = title;
        this.Text = text;
        this.Screenshot = screenshot;
        this.DataType = dataType;
    }
}


