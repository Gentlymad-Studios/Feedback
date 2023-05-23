
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic data structure. 
/// </summary>
public class RequestData<T1,T2>  {
    
    public string Title;
    public string Text;
    public Texture2D Screenshot;
    public Dictionary<List<T1>, List<T2>> Attachments;
    public string DataType;

    public RequestData(string title, string text,Texture2D screenshot, Dictionary<List<T1>, List<T2>> attachments, string dataType) {
        Title = title;
        Text = text;
        Screenshot = screenshot;
        Attachments = attachments;
        DataType = dataType;
    }
}


