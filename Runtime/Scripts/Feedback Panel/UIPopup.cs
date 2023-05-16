using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class UIPopup : UIPopUpBase {

    public APISettings.APIType type;
    public DrawImage drawImage;
    public PanelComponents panelComponents;
    public Dictionary<string, TaskModels.AsanaTaskModel> mentionedTask = new Dictionary<string, TaskModels.AsanaTaskModel>();
    public WindowType ActiveWindow {
        get {
            return activeWindow;
        }
        set {
            WindowType before = activeWindow;
            if (activeWindow == WindowType.None && (value == WindowType.Search || value == WindowType.Report)) {
                base.OnShowWindow();
            } else if (activeWindow != WindowType.None && value == WindowType.None) {
                OnHideWindow();
            }
            activeWindow = value;
            if (activeWindow == WindowType.Search) {
                panelComponents.searchPanel.SetActive(true);
                panelComponents.tabPanel.SetActive(true);
                panelComponents.reportPanel.SetActive(false);
            } else if (activeWindow == WindowType.Report) {
                panelComponents.reportPanel.SetActive(true);
                panelComponents.tabPanel.SetActive(true);
                panelComponents.searchPanel.SetActive(false);
            } else {
                panelComponents.searchPanel.SetActive(false);
                panelComponents.reportPanel.SetActive(false);
                panelComponents.tabPanel.SetActive(false);
            }

            if (before == WindowType.None) {
                //base.GetData();
            }
        }
    }
    private DataType currentDataType = DataType.Feedback;
    private WindowType currentWindowType;
    private WindowType activeWindow = WindowType.Search;

    private List<TagPreview> tagPreviewList = new List<TagPreview>();
    private DateTime lastOpenTime;

    private void Awake() {
        ActiveWindow = WindowType.None;
        currentWindowType = WindowType.Search;
        panelComponents.submitLoginPanel.SetActive(false);
        tagPreviewList = panelComponents.tagPanel.GetComponentsInChildren<TagPreview>().ToList();
        RegisterEvents();
        ConfigureAPI();
    }
    protected override void OnShowWindow() {
        base.OnShowWindow();
        base.GetData();
    }
    protected override void OnHideWindow() {
        SearchWithLucene.Instance.Dispose();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            Debug.Log(ActiveWindow);
            if (ActiveWindow != WindowType.None) {
                currentWindowType = ActiveWindow;
                ActiveWindow = WindowType.None;
            } else {
                ActiveWindow = currentWindowType;
                if (lastOpenTime.AddSeconds(4.0) > DateTime.Now) {
                    Debug.LogWarning("No F1 Spaming please ._.");
                    return;
                }
                base.GetData();
                lastOpenTime = DateTime.Now;
            }
        }

    }

    #region Auth and login
    public void OnLogInButtonClick() {
        try {
            LogIn();
            panelComponents.submitLoginPanel.SetActive(true);
        } catch (Exception e) {
            OnLoginFail(e.Message);
        }
    }
    private void OnLoginSucceed() {
        panelComponents.userName.text = api.requestHandler.GetUser()?.name;
        panelComponents.loginSection.SetActive(false);
        panelComponents.submitLoginPanel.SetActive(false);
    }
    public void OnLogOutButtonClick() {
        LogOut();
        panelComponents.userName.text = "";
        panelComponents.loginSection.SetActive(true);
    }
    protected override void OnLoginFail(string failMessage) {
        Debug.LogWarning(failMessage);
    }

    #endregion

    #region Setup
    private void RegisterEvents() {
        panelComponents.loginSubmitButton.onClick.AddListener(OnLoginSucceed);
        panelComponents.dataTyepDropdown.onValueChanged.AddListener(SetDataType);
        panelComponents.reportTabButton.onClick.AddListener(() => { ShowReportPanel(); CreateTicketFromSearch(); });
        panelComponents.searchTabButton.onClick.AddListener(ShowSearchPanel);
        panelComponents.loginButton.onClick.AddListener(OnLogInButtonClick);
        panelComponents.logoutButton.onClick.AddListener(OnLogOutButtonClick);
        panelComponents.createTicketButton.onClick.AddListener(CreateTicketFromSearch);
        panelComponents.sendButton.onClick.AddListener(SendData);
        panelComponents.mentionList.onValueChanged.AddListener(OnDropdownValueChange);
    }
    private void UnregisterEvents() {
        panelComponents.loginSubmitButton.onClick.RemoveAllListeners();
        panelComponents.loginSubmitButton.onClick.RemoveAllListeners();
        panelComponents.mentionList.onValueChanged.RemoveAllListeners();
        panelComponents.reportTabButton.onClick.RemoveAllListeners();
        panelComponents.searchTabButton.onClick.RemoveAllListeners();
        panelComponents.loginButton.onClick.RemoveAllListeners();
        panelComponents.logoutButton.onClick.RemoveAllListeners();
        panelComponents.createTicketButton.onClick.RemoveAllListeners();
        panelComponents.sendButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Instantiate the api with given type
    /// </summary>
    public void ConfigureAPI() {
        if (type.Equals(APISettings.APIType.Asana)) {
            api = new AsanaAPI();
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

    /// <summary>
    /// Called by clicking on "´Report Tab button". Transfer the data from search to report.
    /// Fill the mention list with mentioned tasks
    /// </summary>
    public void CreateTicketFromSearch() {
        string titleText = "";
        if (string.IsNullOrWhiteSpace(panelComponents.searchInput.text)) {
            titleText = "...";
        } else {
            titleText = panelComponents.searchInput.text;
        }

        foreach (string gid in mentionedTask.Keys) {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData() { text = gid };
            if (panelComponents.mentionList.options.Find(o => o.text == gid) == null) {
                panelComponents.mentionList.options.Add(optionData);
            }
        }

        //look for matching tags and set tag preview action
        foreach (TagPreview p in tagPreviewList) {
            p.addTagToTagList = () => SetTag(p.scriptableTag);
            p.removeFromTagList = () => RemoveTag(p.scriptableTag);
            p.btn = p.gameObject.GetComponent<Button>();
            if (titleText.ToLower().Contains(p.scriptableTag.tagName.ToLower())) {
                SetTag(p.scriptableTag);
                p.Select();
            }
        }

        panelComponents.title.text = titleText;
        ShowReportPanel();
    }

    /// <summary>
    /// Called by changing the mention list value. Instantiate a popup with detailed task informations. 
    /// </summary>
    /// <param name="dataId"></param>
    public void OnDropdownValueChange(int dataId) {
        TMP_Dropdown.OptionData optionData = panelComponents.mentionList.options[dataId];
        string gid = optionData.text;
        TaskModels.AsanaTaskModel task = mentionedTask[gid];

        GameObject popupObject = Instantiate(panelComponents.detailPopup);
        popupObject.transform.SetParent(panelComponents.reportPanel.transform);
        popupObject.transform.localPosition = Vector3.zero;

        DetailPopup popup = popupObject.GetComponent<DetailPopup>();
        popup.title.text = task.name;
        popup.description.text = task.notes;
    }

    public void SendData() {
        if (api is AsanaAPI) {
            var asanaAPI = (AsanaAPI)api;
            asanaAPI.mentions.AddRange(mentionedTask.Keys);
        }

        PostData(panelComponents.title.text, panelComponents.text.text,
            MergeTextures((Texture2D)panelComponents.screenshot.texture, (Texture2D)panelComponents.overpaint.texture),
            currentDataType);
        foreach (TagPreview p in tagPreviewList) {
            p.Deselect();
        }

        panelComponents.title.text = "Descriptive Title";
        panelComponents.text.text = "Description of bug or feedback";
        ActiveWindow = WindowType.None;
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