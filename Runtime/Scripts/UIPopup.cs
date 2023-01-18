using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup : UIPopUpBase { 

    public API_Type type;
    public DrawImage drawImage;

    private bool submit = false;
    private PanelComponents panelComponents;
    private DataType currentDataType = DataType.feedback;

    private void Start() {
        base.OnShowWindow();
        panelComponents = GetComponent<PanelComponents>();
        panelComponents.dropdown.onValueChanged.AddListener(SetDataType);
        panelComponents.button.onClick.AddListener(() => {
            submit = true;
        });
        Configure();
    }
    public void Configure() {
        if (type.Equals(API_Type.asana)) {
            api =  new AsanaAPI();
        }
    }
    public void SetDataType(int i) {
        currentDataType = (DataType)i + 1;
    }
    public void OnLogInButtonClick() {
        LogIn();
        panelComponents.tokenPanel.SetActive(true);
        panelComponents.userInfoPanel.SetActive(true);
        StartCoroutine(WaitForTokenSubmitButtonPress());
    }
    private IEnumerator WaitForTokenSubmitButtonPress() {
        while (!submit) {
            yield return new WaitForSeconds(1f);
        }
        api.settings.token = panelComponents.tokenText.text.ToString();
        api.requestHandler.TokenExchange();
        panelComponents.userName.text = api.requestHandler.GetUserData().name;
        panelComponents.userMail.text = api.requestHandler.GetUserData().email;
        panelComponents.tokenPanel.SetActive(false);
    }
    public void OnLogOutButtonClick() {
        LogOut();
        panelComponents.userName.text = "user name";
        panelComponents.userMail.text = "user mail";
        panelComponents.userInfoPanel.SetActive(false);
    }
    public void Submit() {
        PostData(panelComponents.title.text, panelComponents.text.text, currentDataType);
    }

    protected override void OnLoginFail(string failMessage) {
        throw new NotImplementedException();
    }

    protected override void OnHideWindow() {
        throw new NotImplementedException();
    }

    protected override void OnAfterScreenshotCapture(Texture2D screenshot) {
        panelComponents.screenshot.texture = screenshot;
        screenshot.Apply();
        drawImage.Setup();
    }

}
