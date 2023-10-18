using Game.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static AsanaAPI;
using static TaskModels;
using Debug = UnityEngine.Debug;

public class UIPopup : UIPopUpBase {
    public APISettings.APIType Type;
    public DrawImage DrawImage;
    public PanelComponents PanelComponents;
    public UIDocument UIDocument;
    public TicketBrowser TicketBrowser;
    public Prompt Prompt;
    public Loading Loading;
    public VisualTreeAsset TaskCardUi;
    public VisualTreeAsset TagUi;
    public VisualTreeAsset TaskDetailCardUi;
    public VisualTreeAsset PromptUi;
    public VisualTreeAsset LoadingUi;
    public Texture2D avatarPlaceholderIcon;

    private List<TagPreview> tagPreviewList = new List<TagPreview>();

    public delegate void Callback();
    public Callback OnOpen;
    public Callback OnClose;

    public static AsanaAPISettings settings;
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

    private bool currentlyLoading = false;
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
            base.GetData();
        }
    }

    private void Awake() {
        settings = asanaSpecificSettings;

        if (PanelComponents == null) {
            PanelComponents = new PanelComponents();
            PanelComponents.Initialize(UIDocument);
        }

        CheckDevLogin();

        ActiveWindow = WindowType.None;
        currentWindowType = WindowType.Search;

        SetWindowTypes();

        ConfigureAPI();
        RegisterEvents();
        SetupTaskTypeDrowndown();

        TicketBrowser = new TicketBrowser(this);
        DrawImage = new DrawImage();

        if (Prompt == null) {
            Prompt = new Prompt(PromptUi);
            PanelComponents.root.Add(Prompt);
        }

        if (Loading == null) {
            Loading = new Loading(LoadingUi);
            PanelComponents.root.Add(Loading);
        }

        fileLoader = new LoadAsanaAttachmentFiles(settings);
    }

    private void Update() {
        if (currentlyLoading) {
            Loading.SpinLoadingIcon();
        }

        if (!currentlyLoading && !ActiveWindow.Equals(WindowType.None)) {
            //Init Tags after loading
            if (PanelComponents.taskTagDrpDwn.choices.Count == 0) {
                PanelComponents.tagContainer.Clear();
                PanelComponents.taskTagDrpDwn.choices.Clear();

                AsanaAPI asanaAPI = Api as AsanaAPI;

                //Init Tag DropDown
                foreach (Tags tag in asanaAPI.ReportTagsBackup.enum_options) {
                    VisualElement tagUi = TagUi.Instantiate();
                    PanelComponents.tagContainer.Add(tagUi);

                    TagPreview tagPreview = new TagPreview(tagUi, tag.name, tag.gid);
                    tagPreviewList.Add(tagPreview);

                    tagPreview.addTagToTagList = () => SetTag(tagPreview);
                    tagPreview.removeFromTagList = () => RemoveTag(tagPreview);

                    tagPreview.ToggleTag(false);

                    PanelComponents.taskTagDrpDwn.choices.Add(tag.name);
                }
            }

            Loading?.Hide();
        }

        //Hide Overlays
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            Prompt.callback?.Invoke();
            Prompt.Hide();
            TicketBrowser?.HideDetailCard();
        }
    }

    #region Manage Windows
    private void SetWindowTypes() {
        if (activeWindow == WindowType.Search) {
            PanelComponents.root.style.display = DisplayStyle.Flex;
            PanelComponents.searchTab.style.display = DisplayStyle.Flex;
            PanelComponents.reportTab.style.display = DisplayStyle.None;
            PanelComponents.searchBtn.AddToClassList("menuItemActive");
            PanelComponents.reportBtn.RemoveFromClassList("menuItemActive");
            PanelComponents.tabDescriptionLbl.text = settings.searchDescripton;
        } else if (activeWindow == WindowType.Report) {
            PanelComponents.root.style.display = DisplayStyle.Flex;
            PanelComponents.searchTab.style.display = DisplayStyle.None;
            PanelComponents.reportTab.style.display = DisplayStyle.Flex;
            PanelComponents.searchBtn.RemoveFromClassList("menuItemActive");
            PanelComponents.reportBtn.AddToClassList("menuItemActive");
            PanelComponents.tabDescriptionLbl.text = settings.reportDescription;
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
        SetLoadingStatus(true);
        base.OnShowWindow();
        Loading.Show("load tickets...");
        RegisterEvents();
        TicketBrowser?.InitEvents();
    }

    protected override void OnHideWindow() {
        SetLoadingStatus(false);
        UnregisterEvents();

        Destroy(screenshot);
        DrawImage?.Dispose();
        TicketBrowser?.Dispose();
        SearchWithLucene.Instance.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private void Reset() {
        DrawImage.drawingCanBeDestroyed = true;
        PanelComponents.searchTxtFld.value = string.Empty;
        PanelComponents.taskDescriptionTxt.value = string.Empty;
        PanelComponents.taskTitleTxt.value = string.Empty;
        PanelComponents.taskTypeDrpDwn.value = settings.asanaProjects[0].name;

        MentionedTask.Clear();

        for (int i = 0; i < tagPreviewList.Count; i++) {
            tagPreviewList[i].ToggleTag(false);
        }

        TicketBrowser?.ResetPreview();
    }

    private void ShowReportPanel() {
        ActiveWindow = WindowType.Report;
        SetWindowTypes();
    }

    private void ShowSearchPanel() {
        ActiveWindow = WindowType.Search;
        SetWindowTypes();
    }
    #endregion

    #region Auth and login
    private void CheckDevLogin() {
        if (Debug.isDebugBuild) {
            PanelComponents.loginSection.SetEnabled(true);
        }
    }

    private void OnLogInButtonClick() {
        if (Api.RequestHandler.User == null) {
            try {
                SetLoadingStatus(true);
                Loading.Show("wait for login...", true, AbortLogIn);

                LoginResultEvent -= LoginResult;
                LoginResultEvent += LoginResult;

                AvatarLoadedEvent -= AvatarLoaded;
                AvatarLoadedEvent += AvatarLoaded;

                LogIn();
            } catch (Exception e) {
                OnLoginFail(e.Message);
            }
        } else {
            LogOut();
            SetupTaskTypeDrowndown();
            PanelComponents.loginBtn.text = "<u>Login</u>";
            PanelComponents.userImg.style.backgroundImage = avatarPlaceholderIcon;
        }
    }

    private void LoginResult(bool success) {
        if (success) {
            PanelComponents.loginBtn.text = "<u>Logout</u>";
            //PanelComponents.loginBtn.text = "Logout " + Api.RequestHandler.User.name;

            Api.RequestHandler.LoadAvatar();
        } else {
            OnLoginFail("Unable to Login!");
        }
        SetupTaskTypeDrowndown();
        SetLoadingStatus(false);
    }

    private void AvatarLoaded() {
        PanelComponents.userImg.style.backgroundImage = Api.RequestHandler.User.avatar;
    }

    protected override void OnLoginFail(string failMessage) {
        Debug.LogWarning(failMessage);
    }
    #endregion

    #region Setup Events and API
    private void RegisterEvents() {
        UnregisterEvents();
        PanelComponents.taskTypeDrpDwn.RegisterValueChangedCallback(SetDataType);
        PanelComponents.howToLbl.RegisterCallback<ClickEvent>(HowToLbl_clicked);
        PanelComponents.reportBtn.RegisterCallback<ClickEvent>(ReportTab_clicked);
        PanelComponents.searchBtn.RegisterCallback<ClickEvent>(SearchTab_clicked);
        PanelComponents.loginBtn.RegisterCallback<ClickEvent>(LoginBtn_clicked);
        PanelComponents.searchSubmitBtn.RegisterCallback<ClickEvent>(SearchCreateTicket_clicked);
        PanelComponents.taskSubmitBtn.RegisterCallback<ClickEvent>(TaskSubmit_clicked);
        PanelComponents.taskTagDrpDwn.RegisterValueChangedCallback(TaskTagDrpDwn_changed);
        PanelComponents.taskTitleTxt.RegisterValueChangedCallback(Text_changed);
        PanelComponents.taskDescriptionTxt.RegisterValueChangedCallback(Text_changed);
        PanelComponents.searchTxtFld.RegisterValueChangedCallback(Text_changed);
        PanelComponents.searchCancelBtn.clicked += CancelBtn_clicked;
        PanelComponents.taskCancelBtn.clicked += CancelBtn_clicked;
    }
    private void UnregisterEvents() {
        PanelComponents.taskTypeDrpDwn.UnregisterValueChangedCallback(SetDataType);
        PanelComponents.howToLbl.UnregisterCallback<ClickEvent>(HowToLbl_clicked);
        PanelComponents.reportBtn.UnregisterCallback<ClickEvent>(ReportTab_clicked);
        PanelComponents.searchBtn.UnregisterCallback<ClickEvent>(SearchTab_clicked);
        PanelComponents.loginBtn.UnregisterCallback<ClickEvent>(LoginBtn_clicked);
        PanelComponents.searchSubmitBtn.UnregisterCallback<ClickEvent>(SearchCreateTicket_clicked);
        PanelComponents.taskSubmitBtn.UnregisterCallback<ClickEvent>(TaskSubmit_clicked);
        PanelComponents.imageContainer.UnregisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
        PanelComponents.taskTitleTxt.UnregisterValueChangedCallback(Text_changed);
        PanelComponents.taskDescriptionTxt.UnregisterValueChangedCallback(Text_changed);
        PanelComponents.searchTxtFld.UnregisterValueChangedCallback(Text_changed);
        PanelComponents.searchCancelBtn.clicked -= CancelBtn_clicked;
        PanelComponents.taskCancelBtn.clicked -= CancelBtn_clicked;
    }
    public void ConfigureAPI() {
        if (Type.Equals(APISettings.APIType.Asana)) {
            Api = new AsanaAPI(asanaSpecificSettings);
        }
    }
    public void SetDataType(ChangeEvent<string> changeEvent) {
        currentDataType = changeEvent.newValue;

        //cast label to INotifyValueChanged<string> to got the option to SetValueWithoutNotify
        INotifyValueChanged<string> titlePreviewLbl = PanelComponents.taskTitleTxt.Q("previewTxt") as INotifyValueChanged<string>;
        INotifyValueChanged<string> descPreviewLbl = PanelComponents.taskDescriptionTxt.Q("previewTxt") as INotifyValueChanged<string>;

        for (int i = 0; i < settings.asanaProjects.Count; i++) {
            if (settings.asanaProjects[i].name == currentDataType) {
                titlePreviewLbl.SetValueWithoutNotify(settings.asanaProjects[i].titlePlaceholder);
                descPreviewLbl.SetValueWithoutNotify(settings.asanaProjects[i].descriptionPlaceholder);
                break;
            }   
        }
    }
    #endregion

    #region UI helper
    private void SetupTaskTypeDrowndown() {
        bool loggedIn = Api.RequestHandler.User != null;

        PanelComponents.taskTypeDrpDwn.choices.Clear();
        for (int i = 0; i < settings.asanaProjects.Count; i++) {
            AsanaProject project = settings.asanaProjects[i];

            if (loggedIn) {
                if (project.hideOnLogin) {
                    continue;
                }
            } else {
                if (project.visibleOnLoginOnly) {
                    continue;
                }
            }
            PanelComponents.taskTypeDrpDwn.choices.Add(settings.asanaProjects[i].name);
        }

        if (string.IsNullOrEmpty(currentDataType) || !PanelComponents.taskTypeDrpDwn.choices.Contains(currentDataType)) {
            PanelComponents.taskTypeDrpDwn.value = PanelComponents.taskTypeDrpDwn.choices[0];
            currentDataType = PanelComponents.taskTypeDrpDwn.choices[0];
        }
    }
    #endregion

    #region UI Events
    private void HowToLbl_clicked(ClickEvent evt) {
        Application.OpenURL(settings.howToUrl);
    }

    private void CancelBtn_clicked() {
        Reset();
        ActiveWindow = WindowType.None;
        SetWindowTypes();
    }

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

    private void Text_changed(ChangeEvent<string> evt) {
        VisualElement visualElement = evt.currentTarget as VisualElement;
        if (string.IsNullOrEmpty(evt.newValue) || string.IsNullOrWhiteSpace(evt.newValue)) {
            visualElement.RemoveFromClassList("hidePreviewText");
        } else {
            visualElement.AddToClassList("hidePreviewText");
        }
    }

    private void SearchCreateTicket_clicked(ClickEvent evt) {
        CreateTicketFromSearch();
    }

    private void TaskSubmit_clicked(ClickEvent evt) {
        SendData();
        Reset();
    }

    private void TaskTagDrpDwn_changed(ChangeEvent<string> evt) {
        OnClickTagDrpDwn(evt.newValue);
    }

    public void OnClickTagDrpDwn(string value) {
        PanelComponents.taskTagDrpDwn.SetValueWithoutNotify("add tag");

        for (int i = 0; i < tagPreviewList.Count; i++) {
            TagPreview tag = tagPreviewList[i];
            if (tag.title == value) {
                tag.ToggleTag();
                break;
            }
        }
    }
    #endregion

    #region Handle Send Data 
    /// <summary>
    /// Called by clicking on "´Report Tab button". Transfer the data from search to report.
    /// Fill the mention list with mentioned tasks
    /// </summary>
    private void CreateTicketFromSearch() {
        string titleText = PanelComponents.searchTxtFld.text;

        PanelComponents.mentionedTickets.Clear();
        if (MentionedTask.Count > 0) {
            PanelComponents.mentionedTicketsContainer.style.display = DisplayStyle.Flex;
        } else {
            PanelComponents.mentionedTicketsContainer.style.display = DisplayStyle.None;
        }

        foreach (var task in MentionedTask) {
            VisualElement tagUi = TagUi.Instantiate();
            PanelComponents.mentionedTickets.Add(tagUi);
            new MentionedTag(tagUi, task.Value.name, task.Value.gid, () => TicketBrowser.OnClickTicketPreviewAction(task.Value.name, task.Value.notes), () => TicketBrowser.RemoveMentionFromList(task.Key, true));
        }

        //look for matching tags
        foreach (TagPreview p in tagPreviewList) {
            if (titleText.ToLower().Contains(p.title.ToLower())) {
                p.ToggleTag(true);
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
    public void SetLoadingStatus(bool loading) {
        currentlyLoading = loading;
    }
    #endregion
}

public enum WindowType {
    None = 0,
    Search = 1,
    Report = 2,
}