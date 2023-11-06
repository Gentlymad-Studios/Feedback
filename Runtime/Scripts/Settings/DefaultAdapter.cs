using UnityEngine;

public class DefaultAdapter : IAdapter {
    public void OpenUrl(string url) {
        Application.OpenURL(url);
    }
}
