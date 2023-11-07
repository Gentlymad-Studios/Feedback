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
    }
}