using System;
using UnityEngine;

public class APISettings : ScriptableObject {

    public APIType type;

    public enum APIType {
        Asana = 1,
    }

    [SerializeReference]
    public CustomAdapter customAdapter=null;

    [NonSerialized]
    public IAdapter adapter;
    public IAdapter Adapter {
        get {
            if (adapter == null) {
                if (customAdapter == null) {
                    adapter = new DefaultAdapter();
                } else {
                    adapter = customAdapter;
                }
            }
            return adapter;
        }
    }

}
