using UnityEngine;

public class APISettings : ScriptableObject{
    [Header("General API Settings")]
    public string token;
    public string defaultToken;
    public string baseURL;
    public API_Type type;
}

public enum API_Type {
    asana = 1,
}
