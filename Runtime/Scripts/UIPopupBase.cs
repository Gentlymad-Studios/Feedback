using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public abstract class UIPopUpBase : MonoBehaviour {

    [Serializable]
    public class LoginFailMessage {
        public LoginFailReason reason;
        public string message;
    }

    public BaseAPI api = null;
    public List<LoginFailMessage> loginFailMessages;

    private Dictionary<LoginFailReason, string> loginFailMessagesLookup = new Dictionary<LoginFailReason, string>();
    private WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
    

    private void Awake() {
        //convert the login fail messages to a lookup table for fast and easy access
        //foreach (LoginFailMessage message in loginFailMessages) {
        //    loginFailMessagesLookup.Add(message.reason, message.message);
        //}
    }

    protected virtual void PostData(string title, string text, DataType type) {
        RequestData data = new RequestData(title, text, type);
        api.requestHandler.POST(data);
    }

    public void LogIn() {
       api.requestHandler.LogIn();
    }

    public void LogOut() {
       api.requestHandler.LogOut();
    }

    /// <summary>
    /// Called when there was an error while processing the login request
    /// </summary>
    /// <param name="error"></param>
    private void OnLoginFail(HttpWebResponse error) {
        LoginFailReason reason = LoginFailReason.UnknownError;
        if (error.StatusCode.Equals(HttpStatusCode.NotFound)) {
            reason = LoginFailReason.WrongUsernameOrMail;
        } else if (error.StatusCode.Equals(System.Net.HttpStatusCode.Forbidden)) {
            reason = LoginFailReason.WrongPassword;
        }
        // retrieve the correct fail message from the lookup and call the defined & specific fail method
        OnLoginFail(loginFailMessagesLookup[reason]);
    }

    /// <summary>
    /// Called when there was an error while processing the login request
    /// </summary>
    /// <param name="failMessage">the message</param>
    protected abstract void OnLoginFail(string failMessage);

    /// <summary>
    /// base method called, when a login was successful
    /// </summary>
    protected virtual void OnLoginSuccessBase() { }

    /// <summary>
    /// Called when a login was successful
    /// </summary>
    protected virtual void OnLoginSuccess() {
        OnLoginSuccessBase();
    }

    /// <summary>
    /// called when the window was hidden
    /// </summary>
    protected abstract void OnHideWindow();

    /// <summary>
    /// Method called when the window was opened
    /// </summary>
    protected virtual void OnShowWindow() {
        StartCoroutine(CaptureScreenshot(OnAfterScreenshotCapture));
    }

    /// <summary>
    /// Called when a screenshot was captured
    /// </summary>
    /// <param name="screenshot">the captured screenshot</param>
    protected virtual void OnAfterScreenshotCapture(Texture2D screenshot) {

    }


    /// <summary>
    /// Do a fullscreen capture
    /// </summary>
    /// <param name="onAfterCapture">callback with the captured texture</param>
    /// <returns></returns>
    private IEnumerator CaptureScreenshot(Action<Texture2D> onAfterCapture) {
        yield return frameEnd;
        //screenshot
        Texture2D screenshot = null;
        try {
            screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
        onAfterCapture(screenshot);
    }


}

public enum LoginFailReason {
    WrongUsernameOrMail = 0,
    WrongPassword = 1,
    UnknownError = 2

}
