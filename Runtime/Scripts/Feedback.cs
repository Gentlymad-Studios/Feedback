using UnityEngine;

public class Feedback {
    private static UIPopup feedbackWindow;

    public static void ToggleFeedbackWindow() {
        if (feedbackWindow == null) {
            feedbackWindow = GameObject.FindObjectOfType<UIPopup>();
        }

        if (feedbackWindow == null) {
            Debug.LogError("Unable to find Feedback Tool Object!");
        } else {
            feedbackWindow.Toggle();
        }
    }
}
