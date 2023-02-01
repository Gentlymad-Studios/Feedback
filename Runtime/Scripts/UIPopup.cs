using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class UIPopup : UIPopUpBase { 

    public API_Type type;
    public DrawImage drawImage;

    private bool submitAccessToken = false;
    private PanelComponents panelComponents;
    private DataType currentDataType = DataType.feedback;

    private void Start() {
        drawImage.Setup();
        base.OnShowWindow();
        panelComponents = GetComponent<PanelComponents>();
        panelComponents.dropdown.onValueChanged.AddListener(SetDataType);
        panelComponents.button.onClick.AddListener(() => {
            submitAccessToken = true;
        });
        Configure();
        //base.GetData("https://app.asana.com/api/1.0/workspaces/1192678870065366/tasks/search?teamS.any=1192678870065368&opt_fields=projects,name,assignee,due_on,completed_at&limit=100 ");
        //base.GetData("https://app.asana.com/api/1.0/workspaces/1192678870065366/tasks");
        base.GetData();
    }
    public void Configure() {
        if (type.Equals(API_Type.asana)) {
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
            panelComponents.tokenPanel.SetActive(true);
            panelComponents.userInfoPanel.SetActive(true);
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
        api.requestHandler.TokenExchange();
       
        panelComponents.userName.text = api.requestHandler.user.name;
        panelComponents.userMail.text = api.requestHandler.user.email;
        panelComponents.tokenPanel.SetActive(false);
    }
    public void OnLogOutButtonClick() {
        LogOut();
        panelComponents.userName.text = "user name";
        panelComponents.userMail.text = "user mail";
        panelComponents.userInfoPanel.SetActive(false);
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
}
