using System.Collections.Generic;

namespace Feedback {
    public interface IAdapter {
        void OpenUrl(string url);

        void OnOpenWindow();

        void OnCloseWindow();

        void OnBeforeScreenshot();

        void OnAfterScreenshot();

        bool GetDevMode();

        List<string> GetSavegame(out bool archive);

        List<string> GetLog(out bool archive);

        List<CustomData> GetCustomFields(AsanaProject projectType);
    }
}