using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Feedback {
    public class DefaultAdapter : IAdapter {
        public void OnCloseWindow() {}

        public void OnOpenWindow() {}

        public void OpenUrl(string url) {
            Application.OpenURL(url);
        }

        public bool GetDevMode() {
            return true;
        }
        public List<string> GetSavegame(out bool archive) {
            archive = false;
            return null;
        }
        public List<string> GetLog(out bool archive) {
            archive = false;
            return null;
        }
        public List<CustomData> GetCustomFields(AsanaProject projectType) {
            return null;
        }
    }
}