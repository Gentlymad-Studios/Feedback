using System;
using UnityEngine;

namespace Feedback {
    public class APISettings : ScriptableObject {

        public APIType type;

        public enum APIType {
            Asana = 1,
        }
    }
}