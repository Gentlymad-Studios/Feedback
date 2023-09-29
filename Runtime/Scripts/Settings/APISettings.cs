using UnityEngine;

public class APISettings : ScriptableObject {

    public APIType type;

    public enum APIType {
        Asana = 1,
    }
}
