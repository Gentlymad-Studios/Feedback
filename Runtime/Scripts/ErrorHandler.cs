using System.Collections.Generic;
using UnityEngine;

namespace Feedback {
    public class ErrorHandler {
        public delegate void FirstErrorThrown();
        public FirstErrorThrown firstErrorThrown;

        public delegate void ErrorThrown();
        public FirstErrorThrown errorThrown;

        private List<Error> errorList = new List<Error>();
        public List<Error> ErrorList => errorList;

        public ErrorHandler() {
            errorList.Clear();

            Application.logMessageReceived -= HandleLog;
            Application.logMessageReceived += HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type) {
            if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception) {
                Error error = new Error(logString, stackTrace);

                if (firstErrorThrown != null && errorList.Count == 0) {
                    firstErrorThrown();
                }

                if (errorThrown != null) {
                    errorThrown();
                }

                errorList.Add(error); //limit
            }
        }
    }

    public class Error {
        string logString;
        public string LogString => logString;

        string stackTrace;
        public string StackTrace => stackTrace;

        public Error(string logString, string stackTrace) {
            this.logString = logString;
            this.stackTrace = stackTrace;
        }
    }
}