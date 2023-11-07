using System.Collections.Generic;

namespace Feedback {
    public interface IAdapter {
        void OpenUrl(string url);

        void OnOpenWindow();

        void OnCloseWindow();

        bool GetDevMode();

        List<CustomData> GetCustomFields(AsanaProject projectType);
    }
}