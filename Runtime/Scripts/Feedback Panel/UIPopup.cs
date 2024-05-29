using Game.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Feedback.AsanaAPI;
using static Feedback.TaskModels;
using Debug = UnityEngine.Debug;

namespace Feedback {
    public class UIPopup : UIPopUpBase {
        public DrawImage DrawImage;
        public PanelComponents PanelComponents;
        public UIDocument UIDocument;
        public Prompt Prompt;
        public Loading Loading;
        public VisualTreeAsset TagUi;
        public VisualTreeAsset PromptUi;
        public VisualTreeAsset LoadingUi;
        public Texture2D avatarPlaceholderIcon;

        [HideInInspector]
        public ErrorHandler errorHandler;

        private List<TagPreview> tagPreviewList = new List<TagPreview>();

        public static AsanaAPISettings settings;
        public AsanaAPISettings asanaSpecificSettings;

        public WindowType ActiveWindow {
            get {
                return activeWindow;
            }
            set {
                if (activeWindow == WindowType.None && value == WindowType.Report) {
                    OnShowWindow();
                    settings.Adapter.OnOpenWindow();
                } else if (activeWindow != WindowType.None && value == WindowType.None) {
                    OnHideWindow();
                    settings.Adapter.OnCloseWindow();
                }
                activeWindow = value;
            }
        }

        private string currentDataType;
        private WindowType currentWindowType;
        private WindowType activeWindow = WindowType.Report;

        private bool devMode = false;
        private string ticketTypeKey;

        private bool initializedAfterLogin = false;
        private bool initializedAfterLoad = false;
        private bool currentlyLoading = false;
        private LoadAsanaAttachmentFiles fileLoader;

        Texture2D screenshot;
        Length fullPercent = new Length(100, LengthUnit.Percent);

        public void Toggle() {
            WindowType cache = ActiveWindow;

            if (ActiveWindow != WindowType.None) {
                ActiveWindow = WindowType.None;
                SetWindowTypes();
                currentWindowType = cache;
            } else {
                ActiveWindow = currentWindowType;
            }
        }

        public void Open(Texture2D externalScreenshot = null) {
            this.externalScreenshot = externalScreenshot;
            ActiveWindow = currentWindowType;
        }

        public void Close() {
            WindowType cache = ActiveWindow;

            ActiveWindow = WindowType.None;
            SetWindowTypes();

            if (cache != WindowType.None) {
                currentWindowType = cache;
            }
        }

        public bool IsOpen() {
            return ActiveWindow != WindowType.None;
        }

        private void Awake() {
            DataReceivedEvent += OnDataReceived;

            ticketTypeKey = $"{nameof(Feedback)}_{nameof(ticketTypeKey)}";
            settings = asanaSpecificSettings;

            if (errorHandler == null) {
                errorHandler = new ErrorHandler(settings);
            }

            fileLoader = new LoadAsanaAttachmentFiles(settings);

            if (PanelComponents == null) {
                PanelComponents = new PanelComponents();
                PanelComponents.Initialize(UIDocument);
            }

            PanelComponents.root.RegisterCallback<MouseDownEvent>(Click);

            //Setup Scrolling for the DescriptionField
            PanelComponents.taskDescriptionTxt.SetVerticalScrollerVisibility(ScrollerVisibility.AlwaysVisible);
            ScrollView descriptionScrollView = PanelComponents.taskDescriptionTxt.Q<ScrollView>();
            descriptionScrollView.mouseWheelScrollSize = 100;

            //... and AttachmentField
            PanelComponents.attachmentTxt.SetVerticalScrollerVisibility(ScrollerVisibility.AlwaysVisible);
            ScrollView attachmentScrollView = PanelComponents.attachmentTxt.Q<ScrollView>();
            attachmentScrollView.mouseWheelScrollSize = 20;

            CheckDevLogin();

            ActiveWindow = WindowType.None;
            currentWindowType = WindowType.Report;

            SetWindowTypes();

            FillUI();

            ConfigureAPI();
            RegisterEvents();
            SetupTaskTypeDrowndown();

            DrawImage = new DrawImage();

            if (Prompt == null) {
                Prompt = new Prompt(PromptUi);
                PanelComponents.root.Add(Prompt);
            }

            if (Loading == null) {
                Loading = new Loading(LoadingUi);
                PanelComponents.root.Add(Loading);
            }
        }

        private void Update() {
            if (currentlyLoading) {
                Loading.SpinLoadingIcon();
            }

            if (initializedAfterLogin) {
                InitializeStartupLoginResult();
            }

            if (!currentlyLoading && !ActiveWindow.Equals(WindowType.None)) {
                //Init Tags after loading
                if (PanelComponents.taskTagDrpDwn.choices.Count == 0) {
                    foreach (VisualElement child in PanelComponents.tagHolder.Children()) {
                        if (!child.ClassListContains("previewText")) {
                            PanelComponents.tagHolder.Remove(child);
                        }
                    }
                    PanelComponents.taskTagDrpDwn.choices.Clear();

                    AsanaAPI asanaAPI = Api as AsanaAPI;

                    //Init Tag DropDown
                    if (asanaAPI.ReportTagsBackup.enum_options != null && asanaAPI.ReportTagsBackup.enum_options.Count > 0) {
                        foreach (Tags tag in asanaAPI.ReportTagsBackup.enum_options) {
                            VisualElement tagUi = TagUi.Instantiate();
                            PanelComponents.tagHolder.Add(tagUi);

                            TagPreview tagPreview = new TagPreview(tagUi, tag.name, tag.gid);
                            tagPreviewList.Add(tagPreview);

                            tagPreview.addTagToTagList = () => {
                                SetTag(tagPreview);
                                UpdateTagHolderPreview();
                            };
                            tagPreview.removeFromTagList = () => {
                                RemoveTag(tagPreview);
                                UpdateTagHolderPreview();
                            };

                            tagPreview.ToggleTag(false);

                            PanelComponents.taskTagDrpDwn.choices.Add(tag.name);
                        }

                        PanelComponents.tagContainer.style.display = DisplayStyle.Flex;
                        //PanelComponents.taskTagDrpDwn.SetValueWithoutNotify("add tag (optional)");
                        //PanelComponents.taskTagDrpDwn.SetEnabled(true);
                    } else {
                        PanelComponents.tagContainer.style.display = DisplayStyle.None;
                        //PanelComponents.taskTagDrpDwn.SetValueWithoutNotify("no tags loaded");
                        //PanelComponents.taskTagDrpDwn.SetEnabled(false);
                    }
                }

                if (!initializedAfterLoad) {
                    initializedAfterLoad = true;

                    SetupTaskTypeDrowndown();
                    SetupAttachmentUI();
                }

                Loading?.Hide();
            }

            //Hide Overlays
            if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                Prompt.callback?.Invoke();
                Prompt.Hide();
            }
        }

        private void OnGUI() {
            if (activeWindow == WindowType.Report && !currentlyLoading && screenshot != null) {
                DrawImage?.OnGUI();
            }
        }

        #region Manage Windows
        private void SetWindowTypes() {
            if (activeWindow == WindowType.Report) {
                PanelComponents.root.style.display = DisplayStyle.Flex;
            } else {
                PanelComponents.root.style.display = DisplayStyle.None;

                List<PopupPanel> popups = PanelComponents.root.Query<PopupPanel>().ToList();
                for (int i = 0; i < popups.Count; i++) {
                    popups[i].Hide();
                }
            }
        }

        protected override void OnShowWindow() {
            settings.Adapter.OnBeforeScreenshot();
            base.OnShowWindow();
            SetLoadingStatus(true);
            Loading.Show("loading...");
            RegisterEvents();

            if (devMode) {
                StartupLoginCheck();
            } else {
                Loading.SetText("load tags...");
                base.GetData(false);
            }

            SetupAttachmentUI();
        }

        protected override void OnHideWindow() {
            Prompt?.Hide();

            SetLoadingStatus(false);
            UnregisterEvents();

            Destroy(screenshot);
            DrawImage?.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void Reset() {
            initializedAfterLoad = false;
            DrawImage.drawingCanBeDestroyed = true;
            PanelComponents.taskDescriptionTxt.value = string.Empty;
            PanelComponents.taskDescriptionTxt.RemoveFromClassList("hidePreviewText");
            PanelComponents.taskTitleTxt.value = string.Empty;
            PanelComponents.taskTitleTxt.RemoveFromClassList("hidePreviewText");

            for (int i = 0; i < tagPreviewList.Count; i++) {
                tagPreviewList[i].ToggleTag(false);
                PanelComponents.tagHolder.RemoveFromClassList("hidePreviewText");
            }
        }
        #endregion

        #region Auth and login
        private void StartupLoginCheck() {
            SetLoadingStatus(true);
            Loading.SetText("check login...");

            GetUserResultEvent -= StartupLoginCheckFinished;
            GetUserResultEvent += StartupLoginCheckFinished;

            string id = Api.RequestHandler.UniqueId;
            Task task = new Task(() => Api.RequestHandler.TryGetUserAsync(id));
            task.Start();
        }

        private void StartupLoginCheckFinished() {
            initializedAfterLogin = true;
        }

        private void InitializeStartupLoginResult() {
            bool loggedIn = Api.RequestHandler.User != null;

            SetLoginUI(loggedIn);

            Loading.SetText("load tags...");
            base.GetData(false);

            initializedAfterLogin = false;
            GetUserResultEvent -= StartupLoginCheckFinished;
        }

        private void CheckDevLogin() {
            devMode = settings.Adapter.GetDevMode();

            PanelComponents.loginSection.SetEnabled(devMode);
            PanelComponents.loginSection.style.display = devMode ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnLogInButtonClick() {
            if (Api.RequestHandler.User == null) {
                try {
                    SetLoadingStatus(true);
                    Loading.Show("wait for login...", true, AbortLogIn);

                    LoginResultEvent -= LoginResult;
                    LoginResultEvent += LoginResult;

                    LogIn();
                } catch (Exception e) {
                    OnLoginFail(e.Message);
                }
            } else {
                LogOut();
                SetLoginUI(false);

                UpdateTasksAfterLogInOut();
            }
        }

        private void LoginResult(bool success) {
            if (success) {
                SetLoginUI(success);
                UpdateTasksAfterLogInOut();
            } else {
                OnLoginFail("Unable to Login!");
                SetLoadingStatus(false);
            }
        }

        private void UpdateTasksAfterLogInOut() {
            SetLoadingStatus(true);
            Loading.ToggleAbort(false);
            Loading.SetText("update data...");

            initializedAfterLoad = false;
            base.GetData(true);
        }

        private void AvatarLoaded() {
            PanelComponents.userImg.style.backgroundImage = Api.RequestHandler.User.avatar;
        }

        private void SetLoginUI(bool loggedIn) {
            if (loggedIn) {
                PanelComponents.loginBtn.text = "<u>Logout</u>";

                AvatarLoadedEvent -= AvatarLoaded;
                AvatarLoadedEvent += AvatarLoaded;
                Api.RequestHandler.LoadAvatar();
            } else {
                PanelComponents.loginBtn.text = "<u>Login</u>";
                PanelComponents.userImg.style.backgroundImage = avatarPlaceholderIcon;
            }
        }

        protected override void OnLoginFail(string failMessage) {
            Debug.LogWarning($"[FeedbackTool] {failMessage}");
        }
        #endregion

        #region Setup Events and API
        private void RegisterEvents() {
            UnregisterEvents();
            PanelComponents.taskTypeDrpDwn.RegisterValueChangedCallback(SetDataType);
            PanelComponents.loginBtn.RegisterCallback<ClickEvent>(LoginBtn_clicked);
            PanelComponents.taskSubmitBtn.RegisterCallback<ClickEvent>(TaskSubmit_clicked);
            PanelComponents.taskTagDrpDwn.RegisterValueChangedCallback(TaskTagDrpDwn_changed);
            PanelComponents.taskTitleTxt.RegisterValueChangedCallback(Text_changed);
            PanelComponents.taskDescriptionTxt.RegisterValueChangedCallback(Text_changed);
            PanelComponents.taskCancelBtn.clicked += CancelBtn_clicked;
            PanelComponents.xButton.clicked += CancelBtn_clicked;
            PanelComponents.helpButton.RegisterCallback<ClickEvent>(HelpBtn_clicked);
            PanelComponents.overviewButton.RegisterCallback<ClickEvent>(OverviewBtn_clicked);
        }

        private void UnregisterEvents() {
            PanelComponents.taskTypeDrpDwn.UnregisterValueChangedCallback(SetDataType);
            PanelComponents.loginBtn.UnregisterCallback<ClickEvent>(LoginBtn_clicked);
            PanelComponents.taskSubmitBtn.UnregisterCallback<ClickEvent>(TaskSubmit_clicked);
            PanelComponents.imageContainer.UnregisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
            PanelComponents.taskTitleTxt.UnregisterValueChangedCallback(Text_changed);
            PanelComponents.taskDescriptionTxt.UnregisterValueChangedCallback(Text_changed);
            PanelComponents.taskCancelBtn.clicked -= CancelBtn_clicked;
            PanelComponents.xButton.clicked -= CancelBtn_clicked;
            PanelComponents.helpButton.UnregisterCallback<ClickEvent>(HelpBtn_clicked);
            PanelComponents.overviewButton.UnregisterCallback<ClickEvent>(OverviewBtn_clicked);
        }

        public void ConfigureAPI() {
            Api = new AsanaAPI(settings);
        }
        #endregion

        #region UI helper
        public void SetupTaskTypeDrowndown() {
            bool loggedIn = Api.RequestHandler.User != null;

            PanelComponents.taskTypeDrpDwn.choices.Clear();
            for (int i = 0; i < settings.asanaProjects.Count; i++) {
                AsanaProject project = settings.asanaProjects[i];

                if (loggedIn) {
                    if (!project.visibleForDev || !settings.enableDevProjects) {
                        continue;
                    }
                } else {
                    if (devMode || !project.visibleForPlayer || !settings.enablePlayerProjects) {
                        continue;
                    }
                }
                PanelComponents.taskTypeDrpDwn.choices.Add(settings.asanaProjects[i].name);
            }

            if (PanelComponents.taskTypeDrpDwn.choices.Count == 0) {
                currentDataType = string.Empty;
                PanelComponents.taskTypeDrpDwn.value = string.Empty;

                PanelComponents.taskSubmitBtn.SetEnabled(false);
                PanelComponents.taskTypeDrpDwn.SetEnabled(false);
                return;
            }

            PanelComponents.taskSubmitBtn.SetEnabled(true);
            PanelComponents.taskTypeDrpDwn.SetEnabled(true);

            currentDataType = PlayerPrefs.GetString(ticketTypeKey);

            if (string.IsNullOrEmpty(currentDataType) || !PanelComponents.taskTypeDrpDwn.choices.Contains(currentDataType)) {
                PanelComponents.taskTypeDrpDwn.value = string.Empty;
            } else {
                PanelComponents.taskTypeDrpDwn.value = currentDataType;
            }

            if (string.IsNullOrEmpty(currentDataType) || string.IsNullOrWhiteSpace(currentDataType)) {
                PanelComponents.taskTypeDrpDwn.RemoveFromClassList("hidePreviewText");
            } else {
                PanelComponents.taskTypeDrpDwn.AddToClassList("hidePreviewText");
            }
        }

        private void FillUI() {
            PanelComponents.titleLbl.text = settings.headerTitle;

            PanelComponents.tabDescriptionLbl.style.display = string.IsNullOrEmpty(settings.reportDescription) ? DisplayStyle.None : DisplayStyle.Flex;
            PanelComponents.tabDescriptionLbl.text = settings.reportDescription;

            PanelComponents.helpButton.style.display = string.IsNullOrEmpty(settings.helpLink) ? DisplayStyle.None : DisplayStyle.Flex;

            PanelComponents.overviewButton.style.display = (string.IsNullOrEmpty(settings.overviewLink) || string.IsNullOrEmpty(settings.overviewText)) ? DisplayStyle.None : DisplayStyle.Flex;
            PanelComponents.overviewButton.text = settings.overviewText;
        }

        private void SetupAttachmentUI() {
            List<string> attachments = new List<string>();

            if (!string.IsNullOrEmpty(currentDataType)) {
                AsanaProject asanaProject = settings.GetProjectByName(currentDataType);
                attachments.AddRange(fileLoader.LoadAttachmentsDummy(asanaProject, errorHandler));

                List<CustomData> customFields = settings.adapter.GetCustomFields(asanaProject);

                if (customFields != null) {
                    for (int i = 0; i < customFields.Count; i++) {
                        if (string.IsNullOrEmpty(customFields[i].friendly_name) || customFields[i].friendly_values == null || customFields[i].friendly_values.Count == 0 || string.IsNullOrEmpty(customFields[i].friendly_values[0])) {
                            continue;
                        }

                        attachments.Add($"[Attribute] {customFields[i].friendly_name} - {string.Join(",", customFields[i].friendly_values)}");
                    }
                }
            }

            //PanelComponents.attachmentContainer.style.display = attachments.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            PanelComponents.attachmentTxt.value = string.Join("\n", attachments);
        }

        private void UpdateTagHolderPreview() {
            bool nothingSelected = true;
            for (int i = 0; i < tagPreviewList.Count; i++) {
                if (tagPreviewList[i].selected) {
                    nothingSelected = false;
                    break;
                }
            }

            if (nothingSelected) {
                PanelComponents.tagHolder.RemoveFromClassList("hidePreviewText");
            } else {
                PanelComponents.tagHolder.AddToClassList("hidePreviewText");
            }
        }
        #endregion

        #region UI Events
        private void CancelBtn_clicked() {
            Reset();
            ActiveWindow = WindowType.None;
            SetWindowTypes();
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

        private void TaskSubmit_clicked(ClickEvent evt) {
            SendData();
        }

        private void TaskTagDrpDwn_changed(ChangeEvent<string> evt) {
            OnClickTagDrpDwn(evt.newValue);
        }

        public void OnClickTagDrpDwn(string value) {
            PanelComponents.taskTagDrpDwn.SetValueWithoutNotify("add tag (optional)");

            for (int i = 0; i < tagPreviewList.Count; i++) {
                TagPreview tag = tagPreviewList[i];
                if (tag.title == value) {
                    tag.ToggleTag();
                    break;
                }
            }
        }

        public void Click(MouseDownEvent evt) {
            if (!PanelComponents.main.layout.Contains(evt.mousePosition) && !Api.RequestHandler.postRequestRunning) {
                ActiveWindow = WindowType.None;
                SetWindowTypes();
            }
        }

        public void SetDataType(ChangeEvent<string> evt) {
            currentDataType = evt.newValue;

            if (!string.IsNullOrEmpty(currentDataType)) {
                PlayerPrefs.SetString(ticketTypeKey, currentDataType);
            }

            VisualElement visualElement = evt.currentTarget as VisualElement;
            if (string.IsNullOrEmpty(currentDataType) || string.IsNullOrWhiteSpace(currentDataType)) {
                visualElement.RemoveFromClassList("hidePreviewText");
            } else {
                visualElement.AddToClassList("hidePreviewText");
            }

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

            if (IsOpen()) {
                SetupAttachmentUI();
            }
        }

        public void HelpBtn_clicked(ClickEvent evt) {
            settings.Adapter.OpenUrl(settings.helpLink, settings.openHelpLinkWithFallback);
        }

        public void OverviewBtn_clicked(ClickEvent evt) {
            settings.Adapter.OpenUrl(settings.overviewLink, settings.openOverviewLinkWithFallback);
        }
        #endregion

        #region Handle Data 
        private void OnDataReceived(ReportTags reportTags) {
            SetLoadingStatus(false);

            if (reportTags.enum_options == null || reportTags.enum_options.Count == 0) {
                Debug.Log($"[FeedbackTool] No ReportTags received.");
            } else {
                Debug.Log($"[FeedbackTool] {reportTags.enum_options.Count} ReportTags received.");
            }
        }

        private void SendData() {
            if (string.IsNullOrEmpty(currentDataType)) {
                Prompt.Show("Failure", "Please specify the type.");
                return;
            }

            AsanaProject asanaProject = settings.GetProjectByName(currentDataType);
            bool loggedIn = Api.RequestHandler.User != null;

            if (loggedIn) {
                if (!asanaProject.visibleForDev) {
                    DataSendEvent(false);
                }
            } else {
                if (!asanaProject.visibleForPlayer) {
                    DataSendEvent(false);
                }
            }

            if (string.IsNullOrWhiteSpace(PanelComponents.taskTitleTxt.text)) {
                PanelComponents.taskTitleTxt.Focus();
                DataSendEvent(false);
            }

            if (Api is AsanaAPI) {
                var asanaAPI = (AsanaAPI)Api;
            }

            List<Texture2D> textureList = new List<Texture2D> {
                MergeTextures(screenshot, DrawImage.drawSurfaceTexture)
            };
            DrawImage.drawingCanBeDestroyed = true;

            List<AsanaTicketRequest.Attachment> attachments = fileLoader.LoadAttachments(asanaProject, textureList, errorHandler);

            RequestData data = new RequestData(PanelComponents.taskTitleTxt.text, PanelComponents.taskDescriptionTxt.text, attachments, asanaProject);

            SetLoadingStatus(true);
            Loading.Show("send feedback...", false);

            FeedbackSendEvent -= DataSendEvent;
            FeedbackSendEvent += DataSendEvent;

            PostData(data);
        }

        private void DataSendEvent(bool success) {
            SetLoadingStatus(false);

            if (success) {
                if (ErrorHandler.HasErrors) {
                    errorHandler.SetUnstable();
                }

                fileLoader.ClearTemp();

                AsanaProject asanaProject = settings.GetProjectByName(currentDataType);
                string title = string.IsNullOrEmpty(asanaProject.successTitle) ? "Success" : asanaProject.successTitle;

                Prompt.Show(title, asanaProject.successMessageText, () => {
                    ActiveWindow = WindowType.None;
                    SetWindowTypes();
                }, dontShowAgainFlag: true, extraButtonText: asanaProject.successButtonText, extraCallback: () => {
                    if (!string.IsNullOrEmpty(asanaProject.successButtonLink)) {
                        settings.Adapter.OpenUrl(asanaProject.successButtonLink, asanaProject.openSuccessButtonLinkWithFallback);
                    }
                });

                Reset();
            } else {
                Prompt.Show("Failure", "An error occurred while sending your report, please try again.");
            }
        }
        #endregion

        #region Screenshot
        public void CaptureScreenshotExternal(Action<Texture2D> afterScreenshot) {
            StartCoroutine(CaptureScreenshot(afterScreenshot));
        }

        protected override void OnAfterScreenshotCapture(Texture2D screenshot) {
            PanelComponents.screenshotContainer.style.backgroundImage = screenshot;
            PanelComponents.imageContainer.UnregisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
            PanelComponents.imageContainer.RegisterCallback<GeometryChangedEvent>(UpdateScreenshotUiScale);
            this.screenshot = screenshot;
            screenshot.hideFlags = HideFlags.HideAndDontSave;
            screenshot.name = "Screenshot";
            screenshot.Apply();
            settings.adapter.OnAfterScreenshot();
            DrawImage.Setup(PanelComponents, screenshot, settings);
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
        Report = 1,
    }
}