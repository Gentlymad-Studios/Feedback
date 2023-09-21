using UnityEngine;

public class Feedback {
    private static UIPopup window;
    public static UIPopup Window {
        get {
            if (window == null) {
                window = GameObject.FindObjectOfType<UIPopup>();
            }

            if (window == null) {
                Debug.LogError("Unable to find Feedback Tool Object!");
                return null;
            }

            return window;
        }
    }
}
