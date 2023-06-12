using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public abstract class UIPopUpBase : MonoBehaviour {

    [Serializable]
    public class LoginFailMessage {
        public LoginFailReason Reason;
        public string Message;
    }

    public BaseAPI Api = null;
    public List<LoginFailMessage> LoginFailMessages;

    private Dictionary<LoginFailReason, string> loginFailMessagesLookup = new Dictionary<LoginFailReason, string>();
    private WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    protected void GetData() {
        Api.RequestHandler.GetAllData();
    }

    protected void PostData<T1, T2>(RequestData<T1, T2> data) {
        Api.RequestHandler.PostNewData(data);
    }

    protected void LogIn() {
        Api.RequestHandler.LogIn();
    }

    protected void LogOut() {
        Api.RequestHandler.LogOut();
    }
    protected void SetTag(TagPreview tag) {
        Api.RequestHandler.AddTagToTagList(tag);
    }
    protected void RemoveTag(TagPreview tag) {
        Api.RequestHandler.RemoveTagFromTagList(tag);
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
    /// Method called when the window was opened
    /// </summary>
    protected virtual void OnShowWindow() {
        StartCoroutine(CaptureScreenshot(OnAfterScreenshotCapture));
    }


    /// <summary>
    /// called when the window was hidden
    /// </summary>
    protected abstract void OnHideWindow();


    /// <summary>
    /// Called when a screenshot was captured
    /// </summary>
    /// <param name="screenshot">the captured screenshot</param>
    protected abstract void OnAfterScreenshotCapture(Texture2D screenshot);


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
