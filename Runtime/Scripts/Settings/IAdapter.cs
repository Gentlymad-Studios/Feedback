using System.Collections.Generic;

namespace Feedback {
    public interface IAdapter {
        void OpenUrl(string url, bool useFallback = false);

        void OnOpenWindow();

        void OnCloseWindow();

        void OnBeforeScreenshot();

        void OnAfterScreenshot();

        void OnBeforeSend();

        void OnErrorThrown(Error error);

        void OnFirstErrorThrown(Error error);

        bool GetDevMode();

        List<string> GetSavegame(out bool archive, out string archiveName);

        List<string> GetLog(out bool archive, out string archiveName);

        List<CustomData> GetCustomFields(AsanaProject projectType);
    }
}