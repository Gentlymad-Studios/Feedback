using Game.UI;
using UnityEngine.UIElements;

namespace Feedback {
    public class PanelComponents {
        public UIDocument uiDocument;
        public VisualElement root;

        public VisualElement main;

        // Header Section
        public Label titleLbl;
        public Label tabDescriptionLbl;
        public Button helpButton;
        public Button xButton;
        public Button overviewButton;

        // Login Section
        public VisualElement loginSection;
        public Button loginBtn;
        public VisualElement userImg;

        // Report Section
        public DropdownField taskTypeDrpDwn;
        public DropdownField taskTagDrpDwn;
        public TextField taskTitleTxt;
        public TextField taskDescriptionTxt;
        public Button taskSubmitBtn;
        public Button taskCancelBtn;
        public VisualElement tagContainer;
        public ScrollView tagHolder;
        public TextField attachmentTxt;
        public VisualElement attachmentContainer;

        // Screenshot Section
        public VisualElement imageContainer;
        public VisualElement screenshotContainer;
        public VisualElement overpaintContainer;

        //Paint Toolbar
        public Button brushSizeUpBtn;
        public Button brushSizeDownBtn;
        public Button brushBtn;
        public Button eraseBtn;
        public Button clearBtn;
        public ColorField colorField;

        public void Initialize(UIDocument uiDocument) {
            this.uiDocument = uiDocument;

            root = uiDocument.rootVisualElement;
            root.panel.visualTree.styleSheets.Add(root.styleSheets[0]);

            main = uiDocument.rootVisualElement.Q("Main");

            titleLbl = uiDocument.rootVisualElement.Q("Title") as Label;
            tabDescriptionLbl = uiDocument.rootVisualElement.Q("tabDescriptionLbl") as Label;
            helpButton = uiDocument.rootVisualElement.Q("HelpButton") as Button;
            xButton = uiDocument.rootVisualElement.Q("XButton") as Button;
            overviewButton = uiDocument.rootVisualElement.Q("OverviewButton") as Button;

            loginSection = uiDocument.rootVisualElement.Q("loginSection");
            loginSection.SetEnabled(false);
            loginBtn = uiDocument.rootVisualElement.Q("loginBtn") as Button;
            userImg = uiDocument.rootVisualElement.Q("loginAvatar");

            taskTypeDrpDwn = uiDocument.rootVisualElement.Q("taskTypeDrpDwn") as DropdownField;
            taskTagDrpDwn = uiDocument.rootVisualElement.Q("taskTagDrpDwn") as DropdownField;
            taskTagDrpDwn.SetValueWithoutNotify("add tag (optional)");
            taskTitleTxt = uiDocument.rootVisualElement.Q("taskTitleTxt") as TextField;
            taskDescriptionTxt = uiDocument.rootVisualElement.Q("taskDescriptionTxt") as TextField;
            taskSubmitBtn = uiDocument.rootVisualElement.Q("taskSubmitBtn") as Button;
            taskCancelBtn = uiDocument.rootVisualElement.Q("taskCancelBtn") as Button;
            tagContainer = uiDocument.rootVisualElement.Q("TagContainer");
            tagHolder = uiDocument.rootVisualElement.Q("tagHolder") as ScrollView;
            attachmentContainer = uiDocument.rootVisualElement.Q("AttachmentContainer");
            attachmentTxt = uiDocument.rootVisualElement.Q("AttachmentTxt") as TextField;
            attachmentTxt.focusable = false;

            imageContainer = uiDocument.rootVisualElement.Q("imageContainer");
            screenshotContainer = uiDocument.rootVisualElement.Q("screenshotContainer");
            overpaintContainer = uiDocument.rootVisualElement.Q("overpaintContainer");

            brushSizeUpBtn = uiDocument.rootVisualElement.Q("brushSizeUpBtn") as Button;
            brushSizeDownBtn = uiDocument.rootVisualElement.Q("brushSizeDownBtn") as Button;
            brushBtn = uiDocument.rootVisualElement.Q("brushBtn") as Button;
            eraseBtn = uiDocument.rootVisualElement.Q("eraseBtn") as Button;
            clearBtn = uiDocument.rootVisualElement.Q("clearCanvasBtn") as Button;
            colorField = uiDocument.rootVisualElement.Q("color-field") as ColorField;
            colorField.PopupRoot = root;
        }
    }
}