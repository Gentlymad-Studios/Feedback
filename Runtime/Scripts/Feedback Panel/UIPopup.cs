using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class UIPopup : UIPopUpBase {
    public APISettings.APIType type;
    public DrawImage drawImage;
    public PanelComponents panelComponents;
    public UIDocument uiDocument;
    public TicketBrowser ticketBrowser;
    public VisualTreeAsset taskCardUi;
    public VisualTreeAsset tagLabelUi;
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
                panelComponents.root.style.display = DisplayStyle.Flex;
                panelComponents.searchTab.style.display = DisplayStyle.Flex;
                panelComponents.reportTab.style.display = DisplayStyle.None;
            } else if (activeWindow == WindowType.Report) {
                panelComponents.root.style.display = DisplayStyle.Flex;
                panelComponents.searchTab.style.display = DisplayStyle.None;
                panelComponents.reportTab.style.display = DisplayStyle.Flex;
            } else {
                panelComponents.root.style.display = DisplayStyle.None;
                panelComponents.searchTab.style.display = DisplayStyle.None;
                panelComponents.reportTab.style.display = DisplayStyle.None;
            }
        }
    }

    private string currentDataType = "Feedback";
    private WindowType currentWindowType;
    private WindowType activeWindow = WindowType.Search;

    private List<TagPreview> tagPreviewList = new List<TagPreview>();
    private DateTime lastOpenTime;

    Texture2D screenshot;
    Length fullPercent = new Length(100, LengthUnit.Percent);

    private void Awake() {
        if (panelComponents == null) {
            panelComponents = new PanelComponents();
            panelComponents.Initialize(uiDocument);
        }

        ActiveWindow = WindowType.None;
        currentWindowType = WindowType.Search;
        //panelComponents.submitLoginPanel.SetActive(false);

        AsanaAPISettings settings = APISettings.LoadSettings<AsanaAPISettings>();

        //Init Type DropDown
        panelComponents.taskTypeDrpDwn.choices.Clear();
        for (int i = 0; i < settings.dataTypes.Count; i++) {
            panelComponents.taskTypeDrpDwn.choices.Add(settings.dataTypes[i]);
        }
        panelComponents.taskTypeDrpDwn.value = settings.dataTypes[0];

        //Init Tags
        tagPreviewList.Clear();
        panelComponents.tagContainer.Clear();

        for (int i = 0; i < settings.tags.Count; i++) {
            VisualElement tagUi = tagLabelUi.Instantiate();
            panelComponents.tagContainer.Add(tagUi);

            TagPreview tagPreview = new TagPreview(tagUi, settings.tags[i]);
            tagPreviewList.Add(tagPreview);
        }

        RegisterEvents();
        ConfigureAPI();

        drawImage = new DrawImage();
        ticketBrowser = new TicketBrowser(this);
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
            //panelComponents.submitLoginPanel.SetActive(true);
        } catch (Exception e) {
            OnLoginFail(e.Message);
        }
    }
    private void OnLoginSucceed() {
        //panelComponents.userName.text = api.requestHandler.GetUser()?.name;
        //panelComponents.loginSection.SetActive(false);
        //panelComponents.submitLoginPanel.SetActive(false);
    }
    public void OnLogOutButtonClick() {
        LogOut();
        //panelComponents.userName.text = "";
        //panelComponents.loginSection.SetActive(true);
    }
    protected override void OnLoginFail(string failMessage) {
        Debug.LogWarning(failMessage);
    }

    #endregion

    #region Setup
    private void RegisterEvents() {
        UnregisterEvents();
        panelComponents.taskTypeDrpDwn.RegisterValueChangedCallback(SetDataType);
        panelComponents.reportBtn.RegisterCallback<ClickEvent>(ReportTab_clicked);
        panelComponents.searchBtn.RegisterCallback<ClickEvent>(SearchTab_clicked);
        panelComponents.loginBtn.RegisterCallback<ClickEvent>(LoginBtn_clicked);
        panelComponents.searchSubmitBtn.RegisterCallback<ClickEvent>(SearchSubmit_clicked);
        panelComponents.taskSubmitBtn.RegisterCallback<ClickEvent>(TaskSubmit_clicked);
        panelComponents.taskMentionsDrpDwn.RegisterCallback<ClickEvent>(TaskMentionDrpDwn_clicked);
    }
    private void UnregisterEvents() {
        panelComponents.taskTypeDrpDwn.UnregisterValueChangedCallback(SetDataType);
        panelComponents.reportBtn.UnregisterCallback<ClickEvent>(ReportTab_clicked);
        panelComponents.searchBtn.UnregisterCallback<ClickEvent>(SearchTab_clicked);
        panelComponents.loginBtn.UnregisterCallback<ClickEvent>(LoginBtn_clicked);
        panelComponents.searchSubmitBtn.UnregisterCallback<ClickEvent>(SearchSubmit_clicked);
        panelComponents.taskSubmitBtn.UnregisterCallback<ClickEvent>(TaskSubmit_clicked);
        panelComponents.taskMentionsDrpDwn.UnregisterCallback<ClickEvent>(TaskMentionDrpDwn_clicked);
    }

    /// <summary>
    /// Instantiate the api with given type
    /// </summary>
    public void ConfigureAPI() {
        if (type.Equals(APISettings.APIType.Asana)) {
            api = new AsanaAPI();
        }
    }
    public void SetDataType(ChangeEvent<string> changeEvent) {
        currentDataType = changeEvent.newValue;
    }
    private void ShowReportPanel() {
        ActiveWindow = WindowType.Report;
    }
    private void ShowSearchPanel() {
        ActiveWindow = WindowType.Search;
    }
    #endregion

    #region Click Events
    private void SearchTab_clicked(ClickEvent evt) {
        ShowSearchPanel();
    }

    private void ReportTab_clicked(ClickEvent evt) {
        ShowReportPanel();
        CreateTicketFromSearch();
    }

    private void LoginBtn_clicked(ClickEvent evt) {
        OnLogInButtonClick();
    }

    private void SearchSubmit_clicked(ClickEvent evt) {
        CreateTicketFromSearch();
    }

    private void TaskSubmit_clicked(ClickEvent evt) {
        SendData();
    }

    private void TaskMentionDrpDwn_clicked(ClickEvent evt) {
        OnClickMentionDrpDwn();
    }
    #endregion

    #region Data creation
    /// <summary>
    /// Called by clicking on "´Report Tab button". Transfer the data from search to report.
    /// Fill the mention list with mentioned tasks
    /// </summary>
    public void CreateTicketFromSearch() {
        string titleText = "";
        if (string.IsNullOrWhiteSpace(panelComponents.searchTxtFld.text)) {
            titleText = "...";
        } else {
            titleText = panelComponents.searchTxtFld.text;
        }

        foreach (string gid in mentionedTask.Keys) {
            if (!panelComponents.taskMentionsDrpDwn.choices.Contains(gid)) {
                panelComponents.taskMentionsDrpDwn.choices.Add(gid);
            }
        }

        //look for matching tags and set tag preview action
        foreach (TagPreview p in tagPreviewList) {
            p.addTagToTagList = () => SetTag(p);
            p.removeFromTagList = () => RemoveTag(p);
            if (titleText.ToLower().Contains(p.title.ToLower())) {
                SetTag(p);
                p.Select();
            }
        }

        panelComponents.taskTitleTxt.value = titleText;
        ShowReportPanel();
    }

    public void OnClickMentionDrpDwn() {
        Debug.Log("Mention Dropdown clicked");
    }

    public void SendData() {
        if (api is AsanaAPI) {
            var asanaAPI = (AsanaAPI)api;
            asanaAPI.mentions.AddRange(mentionedTask.Keys);
        }

        PostData(panelComponents.taskTitleTxt.text, panelComponents.taskDescriptionTxt.text,
            MergeTextures(screenshot, drawImage.drawSurfaceTexture),
            currentDataType);
        foreach (TagPreview p in tagPreviewList) {
            p.Deselect();
        }

        panelComponents.taskTitleTxt.value = "Descriptive Title";
        panelComponents.taskDescriptionTxt.value = "Description of bug or feedback";
        ActiveWindow = WindowType.None;
    }
    #endregion

    #region Screenshot
    protected override void OnAfterScreenshotCapture(Texture2D screenshot) {
        panelComponents.screenshotContainer.style.backgroundImage = screenshot;
        panelComponents.imageContainer.RegisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
        this.screenshot = screenshot;
        screenshot.Apply();
        drawImage.Setup(panelComponents);
    }

    private void UpdateScreenshotUiScale(GeometryChangedEvent evt) {
        float uiWidth = panelComponents.imageContainer.layout.width;
        float uiHeight = panelComponents.imageContainer.layout.height;

        float calculatedHeight = (float)screenshot.height / screenshot.width * uiWidth;

        if (calculatedHeight < uiHeight) {
            //width = 100% | height = calculated in px
            Length height = new Length(calculatedHeight, LengthUnit.Pixel);

            panelComponents.screenshotContainer.style.width = fullPercent;
            panelComponents.screenshotContainer.style.height = height;
            panelComponents.overpaintContainer.style.width = fullPercent;
            panelComponents.overpaintContainer.style.height = height;
        } else {
            float calculatedWdith = (float)screenshot.width /screenshot.height * uiHeight;

            //width = calculated in px| height = 100%
            Length width = new Length(calculatedWdith, LengthUnit.Pixel);

            panelComponents.screenshotContainer.style.width = width;
            panelComponents.screenshotContainer.style.height = fullPercent;
            panelComponents.overpaintContainer.style.width = width;
            panelComponents.overpaintContainer.style.height = fullPercent;
        }
    }

    //Combine Screenshot and Drawing to one Texture
    private Texture2D MergeTextures(Texture2D screenshot, Texture2D overpaint) {
        FilterMode mode = FilterMode.Trilinear;
        int width = screenshot.width;
        int height = screenshot.height;

        //resize texture if ratio is larger than HD
        if (screenshot.width > 1920) {
            height = (int)(((float) height / width) * 1920);
            width = 1920;
        }
       
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