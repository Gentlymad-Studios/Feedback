using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feedback {
    [Serializable]
    public abstract class CustomAdapter : ScriptableObject, IAdapter {
        public abstract void OnCloseWindow();
        public abstract void OnOpenWindow();
        public abstract void OpenUrl(string url);
        public abstract bool GetDevMode();
        public abstract List<string> GetSavegame();
        public abstract List<CustomData> GetCustomFields(AsanaProject projectType);
    }
}