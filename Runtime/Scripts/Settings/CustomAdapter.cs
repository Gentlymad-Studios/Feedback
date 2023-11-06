using System;
using UnityEngine;

[Serializable]
public abstract class CustomAdapter : ScriptableObject, IAdapter {
    public abstract void OpenUrl(string url);
}
