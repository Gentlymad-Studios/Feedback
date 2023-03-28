using System;
using System.Collections;
using UnityEngine;

public class UIPopup : UIPopUpBase { 

    public APISettings.APIType type;
    public DrawImage drawImage;

    private bool submitAccessToken = false;
    public PanelComponents panelComponents;
    private DataType currentDataType = DataType.Feedback;
    private int loginCounter = 0;

    private void Start() {
        drawImage.Setup();
        base.OnShowWindow();
        ConfigureAPI();
        RegisterEvents();
        base.GetData();
        panelComponents.reportPanel.SetActive(false);
    }
    
    private void RegisterEvents() {
        panelComponents.tokenSubmitButton.onClick.AddListener(() => { submitAccessToken = true; });
        panelComponents.dropdown.onValueChanged.AddListener(SetDataType);
        panelComponents.reportTabButton.onClick.AddListener(ShowReportPanel);
        panelComponents.searchTabButton.onClick.AddListener(ShowSearchPanel);
        panelComponents.loginButton.onClick.AddListener(OnLogInButtonClick);
        panelComponents.logoutButton.onClick.AddListener(OnLogOutButtonClick);
        
    }
    public void ConfigureAPI() {
        if (type.Equals(APISettings.APIType.Asana)) {
            api =  new AsanaAPI();
        }
    }
    public void SetDataType(int i) {
        currentDataType = (DataType)i + 1;
    }

    #region Auth and login
    public void OnLogInButtonClick() {
        try {
            LogIn();
            loginCounter++;
            panelComponents.tokenPanel.SetActive(true);
            StartCoroutine(WaitForTokenSubmitButtonPress());
        } catch (Exception e) {
            OnLoginFail(e.Message);
        }
    }
    private IEnumerator WaitForTokenSubmitButtonPress() {
        while (!submitAccessToken) {
            yield return new WaitForSeconds(1f);
        }
        api.settings.token = panelComponents.tokenText.text.ToString();
        api.requestHandler.TokenExchange(false);
        
        panelComponents.userName.text = api.requestHandler.user?.name;
        panelComponents.loginSection.SetActive(false);
        panelComponents.tokenPanel.SetActive(false);
    }
    public void OnLogOutButtonClick() {
        LogOut();
        submitAccessToken = false;
        panelComponents.userName.text = "";
        panelComponents.tokenText.text = "Paste token from browser and click \"ok\"";
        panelComponents.loginSection.SetActive(true);
    }
    protected override void OnLoginFail(string failMessage) {
        Debug.LogWarning(failMessage);
    }

    #endregion
    public void Submit() {
        PostData(panelComponents.title.text, panelComponents.text.text,
            MergeTextures((Texture2D)panelComponents.screenshot.texture, (Texture2D)panelComponents.overpaint.texture),
            currentDataType);
    }
    protected override void OnHideWindow() {
        //dispose the lucene stuff here...
    }

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

    private void ShowReportPanel() {
        panelComponents.searchPanel.SetActive(false);
        panelComponents.reportPanel.SetActive(true);
        
    }
    private void ShowSearchPanel() {
        panelComponents.searchPanel.SetActive(true);
        panelComponents.reportPanel.SetActive(false);

    }
}
