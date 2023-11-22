using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feedback {
    [Serializable]
    public abstract class CustomAdapter : ScriptableObject, IAdapter {
        public abstract void OnCloseWindow();
        public abstract void OnOpenWindow();
        public abstract void OnBeforeScreenshot();
        public abstract void OnAfterScreenshot();
        public abstract void OpenUrl(string url);
        public abstract bool GetDevMode();
        public abstract List<string> GetSavegame(out bool archive);
        public abstract List<string> GetLog(out bool archive);
        public abstract List<CustomData> GetCustomFields(AsanaProject projectType);
    }
}