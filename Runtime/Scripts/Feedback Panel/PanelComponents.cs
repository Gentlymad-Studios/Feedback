using Game.UI;
using UnityEngine.UIElements;

namespace Feedback {
    public class PanelComponents {
        public UIDocument uiDocument;
        public VisualElement root;

        public VisualElement main;

        // Tab Buttons
        public VisualElement reportTab;
        public Button reportBtn;
        public Label tabDescriptionLbl;
        public Label howToDescLbl;
        public Label howToLbl;

        // Login Section
        public VisualElement loginSection;
        public Button loginBtn;
        public VisualElement userImg;

        // Report Section
        public DropdownField taskTypeDrpDwn;
        public DropdownField taskTagDrpDwn;
        public ScrollView tagContainer;
        public TextField taskTitleTxt;
        public TextField taskDescriptionTxt;
        public Button taskSubmitBtn;
        public Button taskCancelBtn;

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

            reportTab = uiDocument.rootVisualElement.Q("ReportTab");
            reportBtn = uiDocument.rootVisualElement.Q("reportBtn") as Button;
            tabDescriptionLbl = uiDocument.rootVisualElement.Q("tabDescriptionLbl") as Label;
            howToLbl = uiDocument.rootVisualElement.Q("howToLbl") as Label;
            howToDescLbl = uiDocument.rootVisualElement.Q("howTo") as Label;

            loginSection = uiDocument.rootVisualElement.Q("loginSection");
            loginSection.SetEnabled(false);
            loginBtn = uiDocument.rootVisualElement.Q("loginBtn") as Button;
            userImg = uiDocument.rootVisualElement.Q("loginAvatar");

            taskTypeDrpDwn = uiDocument.rootVisualElement.Q("taskTypeDrpDwn") as DropdownField;
            taskTagDrpDwn = uiDocument.rootVisualElement.Q("taskTagDrpDwn") as DropdownField;
            taskTagDrpDwn.SetValueWithoutNotify("add tag");
            taskTitleTxt = uiDocument.rootVisualElement.Q("taskTitleTxt") as TextField;
            taskDescriptionTxt = uiDocument.rootVisualElement.Q("taskDescriptionTxt") as TextField;
            taskSubmitBtn = uiDocument.rootVisualElement.Q("taskSubmitBtn") as Button;
            taskCancelBtn = uiDocument.rootVisualElement.Q("taskCancelBtn") as Button;
            tagContainer = uiDocument.rootVisualElement.Q("tagHolder") as ScrollView;

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