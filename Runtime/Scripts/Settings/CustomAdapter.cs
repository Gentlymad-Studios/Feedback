using System;
using UnityEngine;

namespace Feedback {
    [Serializable]
    public abstract class CustomAdapter : ScriptableObject, IAdapter {
        public abstract void OnCloseWindow();
        public abstract void OnOpenWindow();
        public abstract void OpenUrl(string url);
        public abstract bool GetDevMode();
    }
}