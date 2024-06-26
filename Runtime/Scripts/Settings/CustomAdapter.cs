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
        public abstract void OnBeforeSend();
        public abstract void OnErrorThrown(Error error);
        public abstract void OnFirstErrorThrown(Error error);
        public abstract void OpenUrl(string url, bool useFallback = false);
        public abstract bool GetDevMode();
        public abstract List<string> GetSavegame(out bool archive, out string archiveName);
        public abstract List<string> GetLog(out bool archive, out string archiveName);
        public abstract List<CustomData> GetCustomFields(AsanaProject projectType);
    }
}