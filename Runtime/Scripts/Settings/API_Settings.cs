using Codice.CM.SEIDInfo;
using UnityEditor;
using UnityEngine;


//[CreateAssetMenu(fileName = "API_Settings_SO", menuName = nameof(APISettings), order = 1)]
public class APISettings : ScriptableObject{
    [Header("General API Settings")]
    public string token;
    public string baseURL;
    public API_Type type;
}

public enum API_Type {
    asana = 1,
}
