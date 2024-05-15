using System;
using System.Collections.Generic;

/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
namespace Feedback {
    public class AsanaAPI {
        public delegate void DataReceived(TaskModels.ReportTags reportTags);
        public static event DataReceived DataReceivedEvent;

        public delegate void LoginResult(bool success);
        public static event LoginResult LoginResultEvent;

        public delegate void GetUserResult();
        public static event GetUserResult GetUserResultEvent;

        public delegate void AvatarLoaded();
        public static event AvatarLoaded AvatarLoadedEvent;

        public delegate void FeedbackSend(bool success);
        public static event FeedbackSend FeedbackSendEvent;

        public BaseRequestHandler RequestHandler;
        public AsanaAPISettings AsanaSpecificSettings;
        public TaskModels.ReportTags ReportTagsBackup = new TaskModels.ReportTags();
        public List<string> CustomFields = new List<string>();

        public DateTime lastUpdateTime;

        public AsanaAPI(AsanaAPISettings s) {
            AsanaSpecificSettings = s;
            RequestHandler = new AsanaRequestHandler(this);
        }

        public void FireDataCreated(TaskModels.ReportTags reportTags) {
            DataReceivedEvent.Invoke(reportTags);
        }

        public void FireLoginResult(bool success) {
            LoginResultEvent.Invoke(success);
        }

        public void FireGetUserResult() {
            GetUserResultEvent.Invoke();
        }

        public void FireAvatarLoaded() {
            AvatarLoadedEvent.Invoke();
        }

        public void FireFeedbackSend(bool success) {
            FeedbackSendEvent.Invoke(success);
        }
    }
}