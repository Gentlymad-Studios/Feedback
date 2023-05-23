using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class UIPopup : UIPopUpBase {
    public APISettings.APIType Type;
    public DrawImage DrawImage;
    public PanelComponents PanelComponents;
    public UIDocument UIDocument;
    public TicketBrowser TicketBrowser;
    public VisualTreeAsset TaskCardUi;
    public VisualTreeAsset TagLabelUi;
    public VisualTreeAsset TaskDetailCardUi;
    public VisualTreeAsset PromptUi;

    //implement settings provider with editor helper
    public AsanaAPISettings asanaSpecificSettings;

    public Dictionary<string, TaskModels.AsanaTaskModel> MentionedTask = new Dictionary<string, TaskModels.AsanaTaskModel>();
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
                PanelComponents.root.style.display = DisplayStyle.Flex;
                PanelComponents.searchTab.style.display = DisplayStyle.Flex;
                PanelComponents.reportTab.style.display = DisplayStyle.None;
            } else if (activeWindow == WindowType.Report) {
                PanelComponents.root.style.display = DisplayStyle.Flex;
                PanelComponents.searchTab.style.display = DisplayStyle.None;
                PanelComponents.reportTab.style.display = DisplayStyle.Flex;
            } else {
                PanelComponents.root.style.display = DisplayStyle.None;
                PanelComponents.searchTab.style.display = DisplayStyle.None;
                PanelComponents.reportTab.style.display = DisplayStyle.None;
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
        if (PanelComponents == null) {
            PanelComponents = new PanelComponents();
            PanelComponents.Initialize(UIDocument);
        }

        ActiveWindow = WindowType.None;
        currentWindowType = WindowType.Search;
        //panelComponents.submitLoginPanel.SetActive(false);

        //AsanaAPISettings settings = APISettings.LoadSettings<AsanaAPISettings>();
        AsanaAPISettings settings = asanaSpecificSettings;

        //Init Type DropDown
        PanelComponents.taskTypeDrpDwn.choices.Clear();
        for (int i = 0; i < settings.DataTypes.Count; i++) {
            PanelComponents.taskTypeDrpDwn.choices.Add(settings.DataTypes[i]);
        }
        PanelComponents.taskTypeDrpDwn.value = settings.DataTypes[0];

        //Init Tags
        tagPreviewList.Clear();
        PanelComponents.tagContainer.Clear();

        for (int i = 0; i < settings.Tags.Count; i++) {
            VisualElement tagUi = TagLabelUi.Instantiate();
            PanelComponents.tagContainer.Add(tagUi);

            TagPreview tagPreview = new TagPreview(tagUi, settings.Tags[i]);
            tagPreviewList.Add(tagPreview);
        }

        RegisterEvents();
        ConfigureAPI();

        TicketBrowser = new TicketBrowser(this);
        DrawImage = new DrawImage();
    }

    protected override void OnShowWindow() {
        base.OnShowWindow();
        base.GetData();
    }
    protected override void OnHideWindow() {

        Destroy(screenshot);
        DrawImage?.Dispose();
        SearchWithLucene.Instance.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

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
        PanelComponents.taskTypeDrpDwn.RegisterValueChangedCallback(SetDataType);
        PanelComponents.reportBtn.RegisterCallback<ClickEvent>(ReportTab_clicked);
        PanelComponents.searchBtn.RegisterCallback<ClickEvent>(SearchTab_clicked);
        PanelComponents.loginBtn.RegisterCallback<ClickEvent>(LoginBtn_clicked);
        PanelComponents.searchSubmitBtn.RegisterCallback<ClickEvent>(SearchSubmit_clicked);
        PanelComponents.taskSubmitBtn.RegisterCallback<ClickEvent>(TaskSubmit_clicked);
        PanelComponents.taskMentionsDrpDwn.RegisterValueChangedCallback(TaskMentionDrpDwn_changed);
    }
    private void UnregisterEvents() {
        PanelComponents.taskTypeDrpDwn.UnregisterValueChangedCallback(SetDataType);
        PanelComponents.reportBtn.UnregisterCallback<ClickEvent>(ReportTab_clicked);
        PanelComponents.searchBtn.UnregisterCallback<ClickEvent>(SearchTab_clicked);
        PanelComponents.loginBtn.UnregisterCallback<ClickEvent>(LoginBtn_clicked);
        PanelComponents.searchSubmitBtn.UnregisterCallback<ClickEvent>(SearchSubmit_clicked);
        PanelComponents.taskSubmitBtn.UnregisterCallback<ClickEvent>(TaskSubmit_clicked);
        PanelComponents.taskMentionsDrpDwn.UnregisterValueChangedCallback(TaskMentionDrpDwn_changed);
    }

    /// <summary>
    /// Instantiate the api with given type
    /// </summary>
    public void ConfigureAPI() {
        if (Type.Equals(APISettings.APIType.Asana)) {
            Api = new AsanaAPI(asanaSpecificSettings);
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

    private void TaskMentionDrpDwn_changed(ChangeEvent<string> evt) {
        OnClickMentionDrpDwn(evt.newValue);
    }
    #endregion

    #region Data creation
    /// <summary>
    /// Called by clicking on "´Report Tab button". Transfer the data from search to report.
    /// Fill the mention list with mentioned tasks
    /// </summary>
    private void CreateTicketFromSearch() {
        string titleText = "";
        if (string.IsNullOrWhiteSpace(PanelComponents.searchTxtFld.text)) {
            titleText = "...";
        } else {
            titleText = PanelComponents.searchTxtFld.text;
        }

        foreach (string gid in MentionedTask.Keys) {
            if (!PanelComponents.taskMentionsDrpDwn.choices.Contains(gid)) {
                PanelComponents.taskMentionsDrpDwn.choices.Add(gid);
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

        PanelComponents.taskTitleTxt.value = titleText;
        ShowReportPanel();
    }

    public void OnClickMentionDrpDwn(string value) {
        TaskModels.AsanaTaskModel task = MentionedTask[value];
        TicketBrowser.OnClickTicketPreviewAction(task.name, task.notes);

        PanelComponents.taskMentionsDrpDwn.SetValueWithoutNotify(string.Empty);
    }

    private void SendData() {
        if (Api is AsanaAPI) {
            var asanaAPI = (AsanaAPI)Api;
            asanaAPI.Mentions.AddRange(MentionedTask.Keys);
        }

        List<Texture2D> textureList = new List<Texture2D>();
        textureList.Add(MergeTextures(screenshot, DrawImage.drawSurfaceTexture));

        List<string> fileList = new List<string>();
        fileList.Add("hi");

        //Create the dictonary with two different types of list
        Dictionary<List<Texture2D>, List<string>> text2 = new Dictionary<List<Texture2D>, List<string>>();
        text2.Add(textureList, fileList);

        
        RequestData<Texture2D, string> data = new RequestData<Texture2D, string>(PanelComponents.taskTitleTxt.text, PanelComponents.taskDescriptionTxt.text,
            MergeTextures(screenshot, DrawImage.drawSurfaceTexture), text2, currentDataType);

        PostData(data);
        foreach (TagPreview p in tagPreviewList) {
            p.Deselect();
        }

        PanelComponents.taskTitleTxt.value = "Descriptive Title";
        PanelComponents.taskDescriptionTxt.value = "Description of bug or feedback";
        ActiveWindow = WindowType.None;
    }
    #endregion

    #region Screenshot
    protected override void OnAfterScreenshotCapture(Texture2D screenshot) {
        PanelComponents.screenshotContainer.style.backgroundImage = screenshot;
        PanelComponents.imageContainer.RegisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
        this.screenshot = screenshot;
        screenshot.hideFlags = HideFlags.HideAndDontSave;
        screenshot.name = "Screenshot";
        screenshot.Apply();
        DrawImage.Setup(PanelComponents, screenshot.width, screenshot.height);
    }

    private void UpdateScreenshotUiScale(GeometryChangedEvent evt) {
        float uiWidth = PanelComponents.imageContainer.layout.width;
        float uiHeight = PanelComponents.imageContainer.layout.height;

        float calculatedHeight = (float)screenshot.height / screenshot.width * uiWidth;

        if (calculatedHeight < uiHeight) {
            //width = 100% | height = calculated in px
            Length height = new Length(calculatedHeight, LengthUnit.Pixel);

            PanelComponents.screenshotContainer.style.width = fullPercent;
            PanelComponents.screenshotContainer.style.height = height;
            PanelComponents.overpaintContainer.style.width = fullPercent;
            PanelComponents.overpaintContainer.style.height = height;
        } else {
            float calculatedWdith = (float)screenshot.width / screenshot.height * uiHeight;

            //width = calculated in px| height = 100%
            Length width = new Length(calculatedWdith, LengthUnit.Pixel);

            PanelComponents.screenshotContainer.style.width = width;
            PanelComponents.screenshotContainer.style.height = fullPercent;
            PanelComponents.overpaintContainer.style.width = width;
            PanelComponents.overpaintContainer.style.height = fullPercent;
        }
    }

    //Combine Screenshot and Drawing to one Texture
    private Texture2D MergeTextures(Texture2D screenshot, Texture2D overpaint) {
        FilterMode mode = FilterMode.Trilinear;
        int width = screenshot.width;
        int height = screenshot.height;

        //resize texture if ratio is larger than HD
        if (screenshot.width > 1920) {
            height = (int)(((float)height / width) * 1920);
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
        screenshot.hideFlags = HideFlags.HideAndDontSave;
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