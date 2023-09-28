using Game.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static TaskModels;
using Debug = UnityEngine.Debug;

public class UIPopup : UIPopUpBase {
    public APISettings.APIType Type;
    public DrawImage DrawImage;
    public PanelComponents PanelComponents;
    public UIDocument UIDocument;
    public TicketBrowser TicketBrowser;
    public Prompt Prompt;
    public VisualTreeAsset TaskCardUi;
    public VisualTreeAsset TagLabelUi;
    public VisualTreeAsset TaskDetailCardUi;
    public VisualTreeAsset PromptUi;
    public VisualTreeAsset LoadingUi;

    public List<TagPreview> tagPreviewList = new List<TagPreview>();

    public delegate void Callback();
    public Callback OnOpen;
    public Callback OnClose;

    //implement settings provider with editor helper
    public AsanaAPISettings asanaSpecificSettings;

    public Dictionary<string, AsanaTaskModel> MentionedTask = new Dictionary<string, AsanaTaskModel>();
    public WindowType ActiveWindow {
        get {
            return activeWindow;
        }
        set {
            if (activeWindow == WindowType.None && (value == WindowType.Search || value == WindowType.Report)) {
                OnShowWindow();
                if (OnOpen != null) {
                    OnOpen();
                }
            } else if (activeWindow != WindowType.None && value == WindowType.None) {
                OnHideWindow();
                if (OnClose != null) {
                    OnClose();
                }
            }
            activeWindow = value;
        }
    }

    private string currentDataType;
    private WindowType currentWindowType;
    private WindowType activeWindow = WindowType.Search;

    private float animationTime;
    private float duration = 1f;
    private bool currentlyLoading = false;
    private VisualElement loadingOverlay;
    private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private LoadAsanaAttachmentFiles fileLoader;

    Texture2D screenshot;
    Length fullPercent = new Length(100, LengthUnit.Percent);

    public void Toggle() {
        var before = ActiveWindow;

        if (ActiveWindow != WindowType.None) {
            ActiveWindow = WindowType.None;
            SetWindowTypes();
            currentWindowType = before;
        } else {
            ActiveWindow = currentWindowType;
            LockTimeHandler.CheckSpam(AbortOpen);
            if (LockTimeHandler.locked) {
                return;
            }
            base.GetData();
            LockTimeHandler.SetOpenTime();
        }
    }

    private void Awake() {
        if (PanelComponents == null) {
            PanelComponents = new PanelComponents();
            PanelComponents.Initialize(UIDocument);
        }

        ActiveWindow = WindowType.None;
        currentWindowType = WindowType.Search;

        SetWindowTypes();

        //AsanaAPISettings settings = APISettings.LoadSettings<AsanaAPISettings>();
        AsanaAPISettings settings = asanaSpecificSettings;

        //Init Type DropDown
        PanelComponents.taskTypeDrpDwn.choices.Clear();
        for (int i = 0; i < settings.asanaProjects.Count; i++) {
            PanelComponents.taskTypeDrpDwn.choices.Add(settings.asanaProjects[i].name);
        }
        PanelComponents.taskTypeDrpDwn.value = settings.asanaProjects[0].name;
        currentDataType = settings.asanaProjects[0].name;

        RegisterEvents();
        ConfigureAPI();

        TicketBrowser = new TicketBrowser(this);
        DrawImage = new DrawImage();

        if (Prompt == null) {
            Prompt = new Prompt(PromptUi);
            PanelComponents.root.Add(Prompt);
        }

        loadingOverlay = LoadingUi.Instantiate();
        loadingOverlay.name = "LoadWH";
        loadingOverlay.style.position = Position.Absolute;
        loadingOverlay.style.display = DisplayStyle.Flex;
        loadingOverlay.style.height = new Length(100, LengthUnit.Percent);
        loadingOverlay.style.width = new Length(100, LengthUnit.Percent);
        PanelComponents.root.Add(loadingOverlay);
        PanelComponents.loadingLbl = PanelComponents.root.Q("loadingLabel") as Label;
        PanelComponents.loadingSpinner = PanelComponents.root.Q("spinner");
        PanelComponents.loadingLbl.text = "load tickets...";

        fileLoader = new LoadAsanaAttachmentFiles(settings);
    }

    private void Update() {
        if (currentlyLoading) {
            //PanelComponents.root.SetEnabled(false);
            if (loadingOverlay.style.display.Equals(DisplayStyle.None)) {
                loadingOverlay.style.display = DisplayStyle.Flex;
            }
            SpinLoadingIcon();
        }
        if (!currentlyLoading && loadingOverlay != null && !ActiveWindow.Equals(WindowType.None)) {
            //Init Tags after loading
            if (tagPreviewList.Count == 0) {
                PanelComponents.tagContainer.Clear();

                AsanaAPI asanaAPI = Api as AsanaAPI;

                foreach (Tags tag in asanaAPI.ReportTagsBackup.enum_options) {
                    VisualElement tagUi = TagLabelUi.Instantiate();
                    PanelComponents.tagContainer.Add(tagUi);

                    TagPreview tagPreview = new TagPreview(tagUi, tag.name);
                    tagPreviewList.Add(tagPreview);
                }
            }

            //PanelComponents.root.SetEnabled(true);
            if (loadingOverlay.style.display.Equals(DisplayStyle.Flex)) {
                loadingOverlay.style.display = DisplayStyle.None;
            }
        }
    }

    #region Manage Windows
    private void SetWindowTypes() {
        if (activeWindow == WindowType.Search) {
            PanelComponents.root.style.display = DisplayStyle.Flex;
            PanelComponents.searchTab.style.display = DisplayStyle.Flex;
            PanelComponents.reportTab.style.display = DisplayStyle.None;
            PanelComponents.searchBtn.style.backgroundColor = Color.white;
            PanelComponents.reportBtn.style.backgroundColor = Color.grey;
        } else if (activeWindow == WindowType.Report) {
            PanelComponents.root.style.display = DisplayStyle.Flex;
            PanelComponents.searchTab.style.display = DisplayStyle.None;
            PanelComponents.reportTab.style.display = DisplayStyle.Flex;
            PanelComponents.searchBtn.style.backgroundColor = Color.grey;
            PanelComponents.reportBtn.style.backgroundColor = Color.white;
        } else {
            PanelComponents.root.style.display = DisplayStyle.None;
            PanelComponents.searchTab.style.display = DisplayStyle.None;
            PanelComponents.reportTab.style.display = DisplayStyle.None;

            List<PopupPanel> popups = PanelComponents.root.Query<PopupPanel>().ToList();
            for (int i = 0; i < popups.Count; i++) {
                popups[i].Hide();
            }
        }
    }
    protected override void OnShowWindow() {
        SetLoading(true);
        base.OnShowWindow();
        RegisterEvents();
        TicketBrowser?.InitEvents();
    }
    protected override void OnHideWindow() {
        SetLoading(false);
        UnregisterEvents();

        Destroy(screenshot);
        DrawImage?.Dispose();
        TicketBrowser?.Dispose();
        SearchWithLucene.Instance.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
    private void ShowReportPanel() {
        ActiveWindow = WindowType.Report;
        SetWindowTypes();
    }
    private void ShowSearchPanel() {
        ActiveWindow = WindowType.Search;
        SetWindowTypes();
    }

    private void AbortOpen() {
        ActiveWindow = WindowType.None;
        SetWindowTypes();
    }
    #endregion

    #region Auth and login
    public void OnLogInButtonClick() {
        if (Api.RequestHandler.User == null) {
            try {
                LogIn();
                Prompt.Show("Login", "Login erfolgreich", OnLoginSucceed);
            } catch (Exception e) {
                OnLoginFail(e.Message);
            }
        } else {
            LogOut();
            PanelComponents.loginBtn.text = "Login";
        }
    }

    private void OnLoginSucceed() {
        if (Api.RequestHandler.GetUser() != null) {
            PanelComponents.loginBtn.text = "Logout";
        }
    }

    protected override void OnLoginFail(string failMessage) {
        Debug.LogWarning(failMessage);
    }

    #endregion

    #region Setup Events and API
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
        PanelComponents.imageContainer.UnregisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
    }
    public void ConfigureAPI() {
        if (Type.Equals(APISettings.APIType.Asana)) {
            Api = new AsanaAPI(asanaSpecificSettings);
        }
    }
    public void SetDataType(ChangeEvent<string> changeEvent) {
        currentDataType = changeEvent.newValue;
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
    public void OnClickMentionDrpDwn(string value) {
        TaskModels.AsanaTaskModel task = MentionedTask[value];
        TicketBrowser.OnClickTicketPreviewAction(task.name, task.notes);

        PanelComponents.taskMentionsDrpDwn.SetValueWithoutNotify(string.Empty);
    }
    #endregion

    #region Handle Send Data 
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

        if (string.IsNullOrEmpty(PanelComponents.taskTitleTxt.value)) {
            PanelComponents.taskTitleTxt.value = titleText;
        }
        ShowReportPanel();
    }

    private void SendData() {
        if (Api is AsanaAPI) {
            var asanaAPI = (AsanaAPI)Api;
            asanaAPI.Mentions.AddRange(MentionedTask.Keys);
        }

        List<Texture2D> textureList = new List<Texture2D>();
        textureList.Add(MergeTextures(screenshot, DrawImage.drawSurfaceTexture));
        DrawImage.drawingCanBeDestroyed = true;

        AsanaProject asanaProject = asanaSpecificSettings.GetProjectByName(currentDataType);

        Dictionary<string, string> fileList = fileLoader.LoadAttachments(asanaProject);
        //fileList.Add("Test text to represent textual data");

        Dictionary<Dictionary<string, string>, List<Texture2D>> attachmentSet = new Dictionary<Dictionary<string, string>, List<Texture2D>>();
        
        attachmentSet.Add(fileList, textureList);

        RequestData<string, Texture2D> data = new RequestData<string, Texture2D>(PanelComponents.taskTitleTxt.text, PanelComponents.taskDescriptionTxt.text, attachmentSet, asanaProject);

        PostData(data);
        foreach (TagPreview p in tagPreviewList) {
            p.Deselect();
        }

        PanelComponents.taskTitleTxt.value = "Descriptive Title";
        PanelComponents.taskDescriptionTxt.value = "Description of bug or feedback";

        Prompt.Show("Feedback", "Feedback gesendet", () => {
            ActiveWindow = WindowType.None;
            SetWindowTypes();
        });
    }
    #endregion

    #region Screenshot
    protected override void OnAfterScreenshotCapture(Texture2D screenshot) {
        PanelComponents.screenshotContainer.style.backgroundImage = screenshot;
        PanelComponents.imageContainer.UnregisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
        PanelComponents.imageContainer.RegisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
        this.screenshot = screenshot;
        screenshot.hideFlags = HideFlags.HideAndDontSave;
        screenshot.name = "Screenshot";
        screenshot.Apply();
        DrawImage.Setup(PanelComponents, screenshot.width, screenshot.height);
        SetWindowTypes();
    }

    private void UpdateScreenshotUiScale(GeometryChangedEvent evt) {
        if (screenshot == null) {
            return;
        }

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

        screenshot.ReadPixels(texR, 0, 0, true);
        screenshot.hideFlags = HideFlags.HideAndDontSave;
        screenshot.Apply(true);

        return screenshot;
    }
    #endregion

    #region Loading
    public void SetLoading(bool loading) {
        currentlyLoading = loading;
    }

    private void SpinLoadingIcon() {
        while (animationTime > duration) {
            animationTime -= duration;
        }
        var t = animationTime / duration;
        var angle = rotationCurve.Evaluate(t) * 360f;
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        PanelComponents.loadingSpinner.transform.rotation = rotation;
        animationTime += Time.deltaTime;
    }
    #endregion
}

public enum WindowType {
    None = 0,
    Search = 1,
    Report = 2,
}