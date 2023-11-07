using System;
using UnityEngine;

namespace Feedback {
    [Serializable]
    public abstract class CustomAdapter : ScriptableObject, IAdapter {
        public abstract void OpenUrl(string url);
    }
}