using System.Collections.Generic;
using UnityEngine;

namespace Feedback {
    public class ErrorHandler {
        private AsanaAPISettings settings;

        private static bool isUnstable = false;
        public static bool IsUnstable => isUnstable;

        private static bool hasErrors = false;
        public static bool HasErrors => hasErrors;

        private List<Error> errorList = new List<Error>();
        public List<Error> ErrorList => errorList;

        public ErrorHandler(AsanaAPISettings settings) {
            this.settings = settings;

            errorList.Clear();

            Application.logMessageReceived -= HandleLog;
            Application.logMessageReceived += HandleLog;
        }

        public void SetUnstable() {
            isUnstable = true;
        }

        private void HandleLog(string logString, string stackTrace, LogType type) {
            if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception) {
                Error error = new Error(logString, stackTrace, type);

                hasErrors = true;

                if (errorList.Count == 0) {
                    settings.Adapter.OnFirstErrorThrown(error);
                }

                settings.Adapter.OnErrorThrown(error);

                errorList.Add(error);

            }
        }
    }

    public class Error {
        string logString;
        public string LogString => logString;

        string stackTrace;
        public string StackTrace => stackTrace;

        LogType type;
        public LogType Type => type;

        public Error(string logString, string stackTrace, LogType type) {
            this.logString = logString;
            this.stackTrace = stackTrace;
            this.type = type;
        }
    }
}