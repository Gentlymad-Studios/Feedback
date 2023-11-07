using UnityEditor;
using UnityEngine;

/// <summary>
/// The base api class defines all fields of the following derived api instances and their preferred handling methods
/// </summary>
namespace Feedback {
    public abstract class BaseAPI {

        public APISettings Settings;
        public BaseRequestHandler RequestHandler;

        public virtual void CreateAPISpecificSettings() {
        }
    }
}