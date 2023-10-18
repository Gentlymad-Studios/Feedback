using Game.UI;
using UnityEngine.UIElements;

public class PanelComponents {
    public UIDocument uiDocument;
    public VisualElement root;

    // Tab Buttons
    public VisualElement searchTab;
    public VisualElement reportTab;
    public Button searchBtn;
    public Button reportBtn;
    public Label tabDescriptionLbl;
    public Label howToLbl;

    // Login Section
    public VisualElement loginSection;
    public Button loginBtn;
    public VisualElement userImg;

    // Search Section
    public TextField searchTxtFld;
    public Button searchSubmitBtn;
    public Button searchCancelBtn;
    public ScrollView taskContainer;

    // Report Section
    public DropdownField taskTypeDrpDwn;
    public DropdownField taskTagDrpDwn;
    public ScrollView tagContainer;
    public TextField taskTitleTxt;
    public TextField taskDescriptionTxt;
    public Button taskSubmitBtn;
    public Button taskCancelBtn;
    public VisualElement mentionedTicketsContainer;
    public ScrollView mentionedTickets;

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

        searchTab = uiDocument.rootVisualElement.Q("SearchTab");
        reportTab = uiDocument.rootVisualElement.Q("ReportTab");
        searchBtn = uiDocument.rootVisualElement.Q("searchBtn") as Button;
        reportBtn = uiDocument.rootVisualElement.Q("reportBtn") as Button;
        tabDescriptionLbl = uiDocument.rootVisualElement.Q("tabDescriptionLbl") as Label;
        howToLbl = uiDocument.rootVisualElement.Q("howToLbl") as Label;

        loginSection = uiDocument.rootVisualElement.Q("loginSection");
        loginSection.SetEnabled(false);
        loginBtn = uiDocument.rootVisualElement.Q("loginBtn") as Button;
        userImg = uiDocument.rootVisualElement.Q("loginAvatar");

        searchTxtFld = uiDocument.rootVisualElement.Q("searchTxt") as TextField;
        searchSubmitBtn = uiDocument.rootVisualElement.Q("searchSubmitBtn") as Button;
        searchCancelBtn = uiDocument.rootVisualElement.Q("searchCancelBtn") as Button;
        taskContainer = uiDocument.rootVisualElement.Q("taskContainer") as ScrollView;

        taskTypeDrpDwn = uiDocument.rootVisualElement.Q("taskTypeDrpDwn") as DropdownField;
        taskTagDrpDwn = uiDocument.rootVisualElement.Q("taskTagDrpDwn") as DropdownField;
        taskTagDrpDwn.SetValueWithoutNotify("add tag");
        taskTitleTxt = uiDocument.rootVisualElement.Q("taskTitleTxt") as TextField;
        taskDescriptionTxt = uiDocument.rootVisualElement.Q("taskDescriptionTxt") as TextField;
        taskSubmitBtn = uiDocument.rootVisualElement.Q("taskSubmitBtn") as Button;
        taskCancelBtn = uiDocument.rootVisualElement.Q("taskCancelBtn") as Button;
        tagContainer = uiDocument.rootVisualElement.Q("tagHolder") as ScrollView;
        mentionedTicketsContainer = uiDocument.rootVisualElement.Q("MentionedContainer");
        mentionedTickets = uiDocument.rootVisualElement.Q("MentionedTickets") as ScrollView;

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
