
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic data structure. 
/// </summary>
public class RequestData<T1,T2>  {
    
    public string Title;
    public string Text;
    public Dictionary<List<T1>, List<T2>> Attachments;
    public string DataType;

    public RequestData(string title, string text, Dictionary<List<T1>, List<T2>> attachments, string dataType) {
        Title = title;
        Text = text;
        Attachments = attachments;
        DataType = dataType;
    }
}


