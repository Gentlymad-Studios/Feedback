using System.Collections.Generic;
using UnityEngine;

namespace Feedback {
    public class DefaultAdapter : IAdapter {
        public void OnCloseWindow() {}

        public void OnOpenWindow() {}

        public void OnBeforeScreenshot() {}

        public void OnAfterScreenshot() {}

        public void OnBeforeSend() {}

        public void OnErrorThrown(Error error) {}

        public void OnFirstErrorThrown(Error error) {}

        public void OpenUrl(string url, bool useFallback = false) {
            Application.OpenURL(url);
        }

        public bool GetDevMode() {
            return true;
        }
        public List<string> GetSavegame(out bool archive, out string archiveName) {
            archive = false;
            archiveName = "savegame";
            return null;
        }
        public List<string> GetLog(out bool archive, out string archiveName) {
            archive = false;
            archiveName = "log";
            return null;
        }
        public List<CustomData> GetCustomFields(AsanaProject projectType) {
            return null;
        }

    }
}