using System;
using System.Collections.Generic;

/// <summary>
/// Api class which inherits from the basic api class.  
/// </summary>
namespace Feedback {
    public class AsanaAPI : BaseAPI {
        public delegate void DataReceived(List<TaskModels.AsanaTaskModel> playerTasks, List<TaskModels.AsanaTaskModel> devTasks, TaskModels.ReportTags reportTags);
        public static event DataReceived DataReceivedEvent;

        public delegate void LoginResult(bool success);
        public static event LoginResult LoginResultEvent;

        public delegate void AvatarLoaded();
        public static event AvatarLoaded AvatarLoadedEvent;

        public AsanaAPISettings AsanaSpecificSettings;
        public List<TaskModels.AsanaTaskModel> PlayerTicketModelsBackup = new List<TaskModels.AsanaTaskModel>();
        public List<TaskModels.AsanaTaskModel> DevTicketModelsBackup = new List<TaskModels.AsanaTaskModel>();
        public TaskModels.ReportTags ReportTagsBackup = new TaskModels.ReportTags();
        public List<string> CustomFields = new List<string>();
        public List<string> Mentions = new List<string>();

        public DateTime lastUpdateTime;

        public AsanaAPI(AsanaAPISettings s) {
            AsanaSpecificSettings = s;
            base.RequestHandler = new AsanaRequestHandler(this);
        }

        public override void CreateAPISpecificSettings() {
            Settings = AsanaSpecificSettings;
        }

        public void FireDataCreated(List<TaskModels.AsanaTaskModel> playerTickets, List<TaskModels.AsanaTaskModel> devTickets, TaskModels.ReportTags reportTags) {
            DataReceivedEvent.Invoke(playerTickets, devTickets, reportTags);
        }

        public void FireLoginResult(bool success) {
            LoginResultEvent.Invoke(success);
        }

        public void FireAvatarLoaded() {
            AvatarLoadedEvent.Invoke();
        }
    }
}