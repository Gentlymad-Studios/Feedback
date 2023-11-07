namespace Feedback {
    public interface IAdapter {
        void OpenUrl(string url);

        void OnOpenWindow();

        void OnCloseWindow();
    }
}