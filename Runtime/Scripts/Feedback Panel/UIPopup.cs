using Codice.Client.Common.GameUI;
using System;
using System.Collections;
using System.ComponentModel.Design;
using UnityEngine;

public class UIPopup : UIPopUpBase { 

    public APISettings.APIType type;
    public DrawImage drawImage;
    public PanelComponents panelComponents;

    private DataType currentDataType = DataType.Feedback;

    private WindowType currentWindowType;

    public WindowType activeWindow = WindowType.Search;
    public WindowType ActiveWindow {
        get {
            return activeWindow;
        }
        set {
            // check the window state
            if (activeWindow == WindowType.None && (value == WindowType.Search || value == WindowType.Report)) {
                OnShowWindow();
            } else if (activeWindow != WindowType.None && value == WindowType.None) {
                OnHideWindow();
            }
            activeWindow = value;
         
            // set window visibility accordingly
            if (activeWindow == WindowType.Search) {
                panelComponents.searchPanel.SetActive(true);
                panelComponents.reportPanel.SetActive(false);
                panelComponents.tabPanel.SetActive(true);

            } else if (activeWindow == WindowType.Report) {
                panelComponents.searchPanel.SetActive(false);
                panelComponents.reportPanel.SetActive(true);
                panelComponents.tabPanel.SetActive(true);
            } else {
                panelComponents.searchPanel.SetActive(false);
                panelComponents.reportPanel.SetActive(false);
                panelComponents.tabPanel.SetActive(false);
            }
        }
    }
   

    private void Awake() {
        ConfigureAPI();
        RegisterEvents();
        base.GetData();
        ActiveWindow = WindowType.None;
        currentWindowType = WindowType.Search;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            if (ActiveWindow != WindowType.None) {
                currentWindowType = ActiveWindow;
                ActiveWindow = WindowType.None;
            } else {
                ActiveWindow = currentWindowType;
            }
        }
    }
    private void RegisterEvents() {
        panelComponents.tokenSubmitButton.onClick.AddListener(TokenSubmitButton);
        panelComponents.dropdown.onValueChanged.AddListener(SetDataType);
        panelComponents.reportTabButton.onClick.AddListener(ShowReportPanel);
        panelComponents.searchTabButton.onClick.AddListener(ShowSearchPanel);
        panelComponents.loginButton.onClick.AddListener(OnLogInButtonClick);
        panelComponents.logoutButton.onClick.AddListener(OnLogOutButtonClick);
        panelComponents.createTicketButton.onClick.AddListener(CreateTicketFromSearch);
        panelComponents.sendButton.onClick.AddListener(SendData);
    }
    protected override void OnHideWindow() {
    }

    #region Auth and login
    public void OnLogInButtonClick() {
        try {
            LogIn();
            panelComponents.tokenPanel.SetActive(true);
        } catch (Exception e) {
            OnLoginFail(e.Message);
        }
    }
    private void TokenSubmitButton() {
        api.settings.token = panelComponents.tokenText.text.ToString();
        api.requestHandler.TokenExchange(false);
        
        panelComponents.userName.text = api.requestHandler.user?.name;
        panelComponents.loginSection.SetActive(false);
        panelComponents.tokenPanel.SetActive(false);
    }
    public void OnLogOutButtonClick() {
        LogOut();
        panelComponents.userName.text = "";
        panelComponents.tokenText.text = "Paste token from browser and click \"ok\"";
        panelComponents.loginSection.SetActive(true);
    }
    protected override void OnLoginFail(string failMessage) {
        Debug.LogWarning(failMessage);
    }

    #endregion

    #region Setup

    public void ConfigureAPI() {
        if (type.Equals(APISettings.APIType.Asana)) {
            api =  new AsanaAPI();
        }
    }
    public void SetDataType(int i) {
        currentDataType = (DataType)i + 1;
    }
    private void ShowReportPanel() {
        ActiveWindow = WindowType.Report;
    }
    private void ShowSearchPanel() {
        ActiveWindow = WindowType.Search;
    }

    #endregion

    #region Data creation
    public void CreateTicketFromSearch() {
        string titleText = "";
        if (string.IsNullOrWhiteSpace(panelComponents.searchInput.text)) {
            titleText = "...";
        } else {
            titleText = panelComponents.searchInput.text;
        }
        panelComponents.title.text = titleText;
        ShowReportPanel();
    }
    public void SendData() {
        PostData(panelComponents.title.text, panelComponents.text.text,
            MergeTextures((Texture2D)panelComponents.screenshot.texture, (Texture2D)panelComponents.overpaint.texture),
            currentDataType);
        panelComponents.title.text = "Enter descriptive Title";
        panelComponents.text.text = "Description of bug or feedback";

    }
    #endregion

    #region Screenshot
    protected override void OnAfterScreenshotCapture(Texture2D screenshot) {
        panelComponents.screenshot.texture = screenshot;
        screenshot.Apply();
        drawImage.Setup();
    }

    //Combine Screenshot and Drawing to one Texture
    private Texture2D MergeTextures(Texture2D screenshot, Texture2D overpaint) {
        FilterMode mode = FilterMode.Trilinear;
        int width = screenshot.width;
        int height = screenshot.height;

        void ResizeOnGPU(Texture2D texA, Texture2D texB, int widthGPU, int heightGPU, FilterMode fmode) {
            //We need the source texture in VRAM because we render with it
            texA.filterMode = fmode;
            texA.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new RenderTexture(widthGPU, heightGPU, 32);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), texA);
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), texB);
        }

        Rect texR = new Rect(0, 0, width, height);
        ResizeOnGPU(screenshot, overpaint, width, height, mode);

        // Update new texture
        screenshot.Reinitialize(width, height);
        screenshot.ReadPixels(texR, 0, 0, true);
        screenshot.Apply(true);

        return screenshot;
    }
    #endregion
}

public enum WindowType {
    None = 0,
    Search = 1,
    Report = 2,
}